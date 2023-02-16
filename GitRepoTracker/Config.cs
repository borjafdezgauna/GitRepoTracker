using System;
using System.Collections.Generic;
using System.Xml;

namespace GitRepoTracker
{
    public class Config
    {
        //Execution parameters
        public bool UploadReports { get; set; } = true;
        public bool FullUpdate { get; set; } = false;

        //Project parameters
        public string Subject { get; private set; } = null;
        public string GithubUsername { get; private set; } = null;
        public string GithubToken { get; private set; } = null;

        public GitRepo ReportRepo { get; private set; }
        public string ReportFolder { get; private set; }
        public List<GitRepo> UserRepos { get; private set; } = new List<GitRepo>();
        public List<GitRepo> TestRepos { get; private set; } = new List<GitRepo>();
        public string TestedProjectName { get; set; }

        public List<StudentGroup> Groups { get; private set; } = new List<StudentGroup>();

        public List<Evaluation.EvaluationItemSettings> EvaluationItemWeights { get; set; }
            = new List<Evaluation.EvaluationItemSettings>();

        public DateTime TermStartDate { get; set; }
        public DateTime TermEndDate { get; set; }
        public List<int> HolidayHalfWeeks { get; set; } = new List<int>();

        public Evaluation.EvaluationItemSettings EvaluationSettings(string name)
        {
            return EvaluationItemWeights.Find(it => it.ItemName == name);
        }

        public DateTime StartDate
        {
            get
            {
                DateTime start = DateTime.Now;
                foreach (Evaluation.Deadline deadline in Deadlines)
                {
                    if (deadline.Start < start)
                        start = deadline.Start;
                }
                return start;
            }
        }

        public List<Evaluation.Deadline> Deadlines { get; private set; } = new List<Evaluation.Deadline>();

        public void LoadXml(string inputFile)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(inputFile);

                //Subject
                if (doc.DocumentElement.Attributes.GetNamedItem("Subject") != null)
                    Subject = doc.DocumentElement.Attributes["Subject"].Value;

                //GitHub credentials
                XmlNode credentialsNode = doc.DocumentElement.SelectSingleNode("GitHubCredentials");
                GithubUsername = credentialsNode.Attributes["Username"].Value;
                GithubToken = credentialsNode.Attributes["Token"].Value;

                //Student projects
                XmlNodeList studentProjectNodes = doc.DocumentElement.SelectNodes("Repositories/StudentProject");
                foreach (XmlNode node in studentProjectNodes)
                {
                    UserRepos.Add(new GitRepo(node.InnerText));
                }

                //Test projects
                XmlNodeList testProjectNodes = doc.DocumentElement.SelectNodes("Repositories/TestProject");
                foreach (XmlNode node in testProjectNodes)
                {
                    if (node.Attributes.GetNamedItem("TestedProject") != null)
                        Program.Config.TestedProjectName = node.Attributes["TestedProject"].Value;
                    TestRepos.Add(new GitRepo(node.InnerText));
                }

                //Reports project
                XmlNode reportProjectNode = doc.DocumentElement.SelectSingleNode("Repositories/Reports");
                if (reportProjectNode != null)
                {
                    ReportRepo = new GitRepo(reportProjectNode.InnerText);
                    if (reportProjectNode.Attributes.GetNamedItem("Folder") != null)
                        ReportFolder = reportProjectNode.Attributes["Folder"].Value;
                }

                //Calendar
                XmlNode startDateNode = doc.DocumentElement.SelectSingleNode("Calendar/TermStartDate");
                if (startDateNode != null)
                {
                    if (DateTime.TryParse(startDateNode.InnerText, out DateTime startDate))
                        TermStartDate = startDate;
                }
                XmlNode endDateNode = doc.DocumentElement.SelectSingleNode("Calendar/TermEndDate");
                if (endDateNode != null)
                {
                    if (DateTime.TryParse(endDateNode.InnerText, out DateTime endDate))
                        TermEndDate = endDate;
                }
                XmlNodeList holidayHalfWeekNodes = doc.DocumentElement.SelectNodes("Calendar/Holidays/HalfWeek");
                foreach (XmlNode holidayHalfWeekNode in holidayHalfWeekNodes)
                {
                    if (int.TryParse(holidayHalfWeekNode.InnerText, out int halfweekIndex))
                        HolidayHalfWeeks.Add(halfweekIndex);
                }

                //Groups and members
                XmlNodeList groupNodes = doc.DocumentElement.SelectNodes("Groups/Group");
                foreach (XmlNode groupNode in groupNodes)
                {
                    StudentGroup group = new StudentGroup()
                    {
                        Project = groupNode.Attributes["Project"].Value,
                        Name = groupNode.Attributes["Name"].Value
                    };
                    Groups.Add(group);
                    foreach (XmlNode memberNode in groupNode.SelectNodes("./Member"))
                    {
                        Student member = new Student()
                        {
                            Alias = memberNode.Attributes["Name"].Value
                        };
                        foreach (XmlNode emailNode in memberNode.SelectNodes("./Email"))
                            member.Emails.Add(emailNode.InnerText);

                        group.Members.Add(member);
                    }
                }

                //Deadlines
                XmlNodeList deadlineNodes = doc.DocumentElement.SelectNodes("Evaluation/Deadline");
                foreach (XmlNode node in deadlineNodes)
                {
                    string name = node.Attributes["Name"].Value;
                    string projectFolder = node.Attributes["ProjectFolder"].Value;
                    string startDateString = node.Attributes["StartDate"].Value;
                    DateTime.TryParse(startDateString, out DateTime startDate);
                    string endDateString = node.Attributes["EndDate"].Value;
                    DateTime.TryParse(endDateString, out DateTime endDate);
                    Evaluation.Deadline deadline = new Evaluation.Deadline()
                    {
                        Name = name,
                        ProjectFolder = projectFolder,
                        Start = startDate,
                        End = endDate
                    };
                    Deadlines.Add(deadline);
                }

                //Evaluation weights
                XmlNodeList evaluationWeightNodes = doc.DocumentElement.SelectNodes("Evaluation/Item");
                foreach (XmlNode node in evaluationWeightNodes)
                {
                    string name = node.Attributes["Name"].Value;
                    string weightString = node.Attributes["Weight"].Value;
                    DateTime start = StartDate;
                    if (node.Attributes.GetNamedItem("StartDate") != null)
                        DateTime.TryParse(node.Attributes["StartDate"].Value, out start);
                    double minimum = 0;
                    if (node.Attributes.GetNamedItem("Minimum") != null)
                        double.TryParse(node.Attributes["Minimum"].Value, out minimum);
                    double maximum = 0;
                    if (node.Attributes.GetNamedItem("Maximum") != null)
                        double.TryParse(node.Attributes["Maximum"].Value, out maximum);

                    bool bonus = false;
                    if (node.Attributes.GetNamedItem("Bonus") != null)
                        bool.TryParse(node.Attributes["Bonus"].Value, out bonus);

                    if (double.TryParse(weightString, out double weight))
                    {
                        EvaluationItemWeights.Add(new Evaluation.EvaluationItemSettings()
                        {
                            ItemName = name,
                            Weight = weight,
                            Start = start,
                            Minimum = minimum,
                            Maximum = maximum,
                            Bonus = bonus
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading Xml configuration file: {ex.Message}");
            }
        }

        void AddGroup(string config)
        {
            string[] parts = config.Split('=');

            if (parts.Length == 2)
            {
                string groupName = parts[0];
                string memberConfig = parts[1];
                string[] memberConfigParts = memberConfig.Split(',');

                if (memberConfigParts.Length < 2)
                {
                    Console.WriteLine($"Wrong line in config file: {config}");
                    return;
                }

                StudentGroup group = Groups.Find(gr => gr.Project == groupName);
                if (group == null)
                {
                    group = new StudentGroup() { Project = groupName };
                    Groups.Add(group);
                }
                Student member = new Student() { Alias = memberConfigParts[0] };
                for (int i = 1; i < memberConfigParts.Length; i++)
                    member.Emails.Add(memberConfigParts[i]);
                group.Members.Add(member);
            }
            else
            {
                Console.WriteLine($"Wrong line in config file: {config}");
            }
        }
    }
}
