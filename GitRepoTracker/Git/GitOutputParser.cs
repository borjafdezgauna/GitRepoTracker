using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace GitRepoTracker
{
    public static class GitOutputParser
    {
        private static List<string> m_unknownUsers = new List<string>();
        public static List<string> UnknownUsers { get { return m_unknownUsers; } }

        public static void ParseBlameOutput(string output, CommitStats stats, StudentGroup group, DateTime startDate)
        {
            string[] lines = output.Split("\n");
            foreach (string line in lines)
            {
                Match match = Regex.Match(line, "<([^>]+)>\\s+(\\d{4})-(\\d{2})-(\\d{2})");
                if (match.Success)
                {
                    string user = match.Groups[1].Value.Trim(' ');
                    DateTime commitDate = DateTime.Now;
                    if (int.TryParse(match.Groups[2].Value, out int year) &&
                        int.TryParse(match.Groups[3].Value, out int month) &&
                        int.TryParse(match.Groups[4].Value, out int day))
                    {
                        commitDate = new DateTime(year, month, day);
                    }

                    //Ignore lines in first commmit??
                    Student member = group.Members.Find(m => m.Emails.Contains(user));
                    string alias = member != null ? member.Alias : "Unknown";

                    if (member == null && !m_unknownUsers.Contains(user))
                        m_unknownUsers.Add(user);

                    AuthorStats authorStats = stats.StatsByAuthor(alias);
                    if (commitDate > startDate &&
                        authorStats != null)
                    {
                        authorStats.NumBlamedLines++;
                        authorStats.NumBlamedChars += line.Length - match.Length;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }

        static void ParseCoverageResults(string file, out double coverage)
        {
            coverage = 0;

            if (string.IsNullOrEmpty(file))
                return;
            if (!System.IO.File.Exists(file))
                return;

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(System.IO.File.ReadAllText(file));

                XmlNode rootNode = doc.DocumentElement;
                if (rootNode != null)
                {
                    //<coverage line-rate="0.2" branch-rate="0.07139999999999999" version="1.9" timestamp="1645085224" lines-covered="18" lines-valid="90" branches-covered="1" branches-valid="14">
                    
                    if (rootNode.Attributes.GetNamedItem("line-rate") != null)
                    {
                        string lineRateAttr = rootNode.Attributes["line-rate"].Value;
                        if (double.TryParse(lineRateAttr, out double lineCoverage))
                            coverage += lineCoverage;
                    }
                    if (rootNode.Attributes.GetNamedItem("branch-rate") != null)
                    {
                        string lineRateAttr = rootNode.Attributes["branch-rate"].Value;
                        if (double.TryParse(lineRateAttr, out double branchCoverage))
                            coverage += branchCoverage;
                    }
                    coverage = coverage * 50;
                }
            }
            catch
            { }
        }

        public static TestResults ParseTestResults(string output, bool calculateCoverage)
        {
            TestResults testResults = new TestResults();

            string testResultRegex = @"(Passed|Failed) ([\w\.]+) \[";
            foreach (Match match in Regex.Matches(output, testResultRegex))
            {
                if (match.Groups[1].Value == "Passed")
                    testResults.Passed.Add(match.Groups[2].Value);
                else
                    testResults.Failed.Add(match.Groups[2].Value);
            }

            if (calculateCoverage)
            {
                if (output.IndexOf("Attachments:") < 0)
                    return testResults;

                //Parse test coverage results
                double totalCoverage = 0;
                string attachmentsSection = output.Substring(output.IndexOf("Attachments:"));
                string coverageFileRegex = "\\s+([\\w:\\\\\\.-]+.xml)";
                MatchCollection matches = Regex.Matches(attachmentsSection, coverageFileRegex);
                foreach (Match match in matches)
                {
                    string coverageFile = match.Groups[1].Value;
                    ParseCoverageResults(coverageFile, out double testCoverage);
                    totalCoverage += testCoverage;
                }
                testResults.CoveragePercent = totalCoverage / matches.Count;
            }
            return testResults;
        }

        public static List<Commit> ParseCommits(string log, StudentGroup group)
        {
            List<Commit> commits = new List<Commit>();

            //commit 3023f1dcda3fd64c4a84cca098168e40c585ec5d (origin/ColumnTest)
            string regexCommitPattern = @"commit (\w+)(\s\w+)*[\n\r]+(?:Merge: [^\n]+[\r\n]+)?" +
                @"Author: [^\n]*?([^<]+)<([^>]+)>[\r\n]+Date:\s+([^\n]+)\n+\s+([^\r\n]+)";

            Commit orphaned = null;
            
            foreach (Match match in Regex.Matches(log, regexCommitPattern))
            {
                int numGroups = match.Groups.Count;
                string email = match.Groups[numGroups - 3].Value.Trim(' ');
                string commitId = match.Groups[1].Value;
                string commitMessage = match.Groups[numGroups - 1].Value;

                if (orphaned != null)
                {
                    orphaned.Parents.Add(commitId);
                    orphaned = null;
                }

                List<string> parents = new List<string>();

                foreach (Capture captureGroup in match.Groups[2].Captures)
                {
                    string parentId = captureGroup.Value.Trim(' ');
                    parents.Add(parentId);
                }

                string dateString = match.Groups[numGroups - 2].Value;
                if (!Utils.ParseDateFromGitLog(dateString, out DateTime date))
                    continue;

                //Already parsed???
                Commit matchedCommit = commits.Find(c => c.Id == commitId);
                if (matchedCommit != null)
                    continue;

                Student member = group.Members.Find(m => m.Emails.Contains(email));
                string alias = member != null ? member.Alias : "Unknown";

                Commit commit = new Commit() { Author = alias, Id = commitId, Date = date, Message = commitMessage, Parents = parents };

                if (parents.Count == 0 && commits.Count > 0)
                    orphaned = commit;

                commits.Add(commit);
            }
            
            commits.Sort((x, y) => x.Date.CompareTo(y.Date));

            return commits;
        }
    }
}
