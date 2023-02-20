using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace GitRepoTracker
{
    [Serializable]
    public class Commit : IEquatable<Commit>
    {
        [XmlElement]
        public string Id { get; set; }
        [XmlElement]
        public string Author { get; set; }
        [XmlElement]
        public DateTime Date { get; set; }
        [XmlElement]
        public string Message { get; set; }

        [XmlElement]
        public List<string> Parents { get; set; } = new List<string>();

        public bool IsMergeCommit { get { return Parents?.Count > 1; } }

        [XmlElement]
        public CommitStats Stats { get; set; } = new CommitStats();

        public bool Equals(Commit other)
        {
            return Id == other.Id;
        }

        public static int ValidMessagePercent(List<Commit> commits, string author, DateTime start)
        {
            List<Commit> authorCommits = commits.FindAll(c => c.Author == author);

            int numCommits = 0;
            int numValidCommitMessages = 0;
            foreach (Commit commit in authorCommits)
            {
                if (commit.Date < start)
                    continue;

                bool commitMessageLongEnough = commit.Message != null &&
                                               (commit.Message.Length >= 10
                                                || (commit.Message.ToLower().Contains("merge") && commit.Message.Contains("commit")));
                bool commitIssueNumberOk = commit.Message != null && Regex.IsMatch(commit.Message, "#\\d+");

                if (commitMessageLongEnough && commitIssueNumberOk)
                    numValidCommitMessages++;
                numCommits++;
            }
            if (numCommits == 0)
                return 0;
            return (int)(100.0 * numValidCommitMessages / (double)numCommits);
        }

        public static int CorrectBuildTimePercent(List<Commit> commits, DateTime until)
        {
            List<Commit> pushedCommits = commits.FindAll(c => c.Stats.PushedToServer);

            if (pushedCommits.Count == 0)
                return 0;

            DateTime endDate = Program.Config.TermEndDate;
            if (until < endDate)
                endDate = until;

            pushedCommits.Sort((x, y) => x.Date.CompareTo(y.Date));
            double hoursOk = 0;
            double hoursFailed = 0;

            for (int i = 0; i < pushedCommits.Count - 1; i++)
            {
                bool ok = pushedCommits[i].Stats.BuildsAndPassesTests;
                double hoursTillNextCommit = ((pushedCommits[i + 1].Date - pushedCommits[i].Date).TotalHours);
                if (ok)
                    hoursOk += hoursTillNextCommit;
                else
                    hoursFailed += hoursTillNextCommit;
            }

            bool lastCheckOk = pushedCommits[pushedCommits.Count - 1].Stats.BuildsAndPassesTests;
            double sinceLastCheck = (until - pushedCommits[pushedCommits.Count - 1].Date).TotalHours;
            if (lastCheckOk)
                hoursOk += sinceLastCheck;
            else
                hoursFailed += sinceLastCheck;

            if (hoursOk + hoursFailed == 0)
                return 0;
            return (int)Math.Round(100 * (hoursOk / (hoursOk + hoursFailed)));
        }

        public static IncrementalStats IncrementalStats(List<Commit> commits, string author)
        {

            //Student member = group.Members.Find(m => m.Emails.Contains(commit.Author) || m.Alias == commit.Author);
            //string alias = member != null ? member.Alias : "Unknown";

            IncrementalStats incrementalStats = new IncrementalStats(author);

            if (commits == null || commits.Count == 0)
                return incrementalStats;

            List<Commit> processedBuildingParents = new List<Commit>();
            int index = -1;
            foreach (Commit commit in commits)
            {
                index++;
                Commit prevCommit = null;
                Commit prevBuildingCommit = null;

                if (commit.Author == author)
                {
                    //Find parent commit
                    prevCommit = commit.Predecessor(commits);

                    if (prevCommit == null)
                        prevCommit = commit;

                    //Find 1st building commit
                    prevBuildingCommit = commit.BuildingPredecessor(commits);

                    if (prevBuildingCommit == null)
                        prevBuildingCommit = commit;

                    Commit alreadyProcessedBuildingParent = processedBuildingParents.Find(c => c.Id == prevBuildingCommit.Id);
                    if (alreadyProcessedBuildingParent != null)
                    {
                        Console.WriteLine("Building parent already processed: " +
                            commit.Id + "(" + commit.Author + ")" +
                            alreadyProcessedBuildingParent.Id + "(" + alreadyProcessedBuildingParent.Author + ")");
                    }
                    else if (commit.Stats.Builds)
                        processedBuildingParents.Add(prevBuildingCommit);

                    //Compare with previous stats and update incremental stats
                    if (commit.Date >= Program.Config.StartDate && commit.Stats.Builds)
                    {
                        double coveragePercentInc = commit.Stats.CoveragePercent - prevBuildingCommit.Stats.CoveragePercent;
                        incrementalStats.CoveragePercent += coveragePercentInc;

                        if (coveragePercentInc != 0)
                        {
                            incrementalStats.CoverageChanges.Add(new Evaluation.CommitLinkedItem(commit,
                                $"{Utils.DoubleToString(coveragePercentInc,2)}% ({Utils.DoubleToString(prevBuildingCommit.Stats.CoveragePercent, 2)}% " +
                                $"-> {Utils.DoubleToString(commit.Stats.CoveragePercent, 2)}%)"));
                        }

                        double passedTestsPercentInc = commit.Stats.UserTestsResults.Passed.Count
                            - prevBuildingCommit.Stats.UserTestsResults.Passed.Count;
                        incrementalStats.PassedTestsPercent += passedTestsPercentInc;

                        if (passedTestsPercentInc != 0)
                        {
                            incrementalStats.PassedTestsChanges.Add(new Evaluation.CommitLinkedItem(commit,
                                $"{Utils.DoubleToString(passedTestsPercentInc,2)}% ({prevBuildingCommit.Stats.UserTestsResults.Passed.Count}/{prevBuildingCommit.Stats.UserTestsResults.NumTests} " +
                                $"-> {commit.Stats.UserTestsResults.Passed.Count}/{commit.Stats.UserTestsResults.NumTests})"));
                        }
                        

                        int numDeadlines = Math.Max(commit.Stats.DeadlineTestsResults.Count,
                            incrementalStats.PassedDeadlineTestsPercent.Count);
                        for (int i = 0; i < numDeadlines; i++)
                        {
                            if (i < Program.Config.Deadlines.Count && 
                                prevBuildingCommit.Date < Program.Config.Deadlines[i].Start)
                                continue;

                            List<Evaluation.CommitLinkedItem> deadlineChanges = new List<Evaluation.CommitLinkedItem>();

                            int currentDeadlineTestsPassed = 0;
                            int prevDeadlineTestsPassed = 0;
                            int deadlineTests = 0;
                            if (commit.Stats.DeadlineTestsResults.Count > i)
                            {
                                currentDeadlineTestsPassed = commit.Stats.DeadlineTestsResults[i].Passed.Count;
                                deadlineTests = commit.Stats.DeadlineTestsResults[i].NumTests;
                            }

                            if (prevBuildingCommit.Stats.DeadlineTestsResults.Count > i)
                                prevDeadlineTestsPassed = prevBuildingCommit.Stats.DeadlineTestsResults[i].Passed.Count;

                            if (incrementalStats.PassedDeadlineTestsPercent.Count == i)
                                incrementalStats.PassedDeadlineTestsPercent.Add(0);

                            incrementalStats.PassedDeadlineTestsPercent[i] += currentDeadlineTestsPassed - prevDeadlineTestsPassed;

                            if (currentDeadlineTestsPassed - prevDeadlineTestsPassed != 0)
                            {
                                deadlineChanges.Add(new Evaluation.CommitLinkedItem(commit,
                                    $"{prevDeadlineTestsPassed}/{deadlineTests} " +
                                    $"-> {currentDeadlineTestsPassed}/{deadlineTests}"));
                            }
                            if (incrementalStats.DeadlineChanges.Count <= i)
                                incrementalStats.DeadlineChanges.Add(deadlineChanges);
                            else if (deadlineChanges.Count > 0)
                                incrementalStats.DeadlineChanges[i].AddRange(deadlineChanges);
                        }
                    }
                    incrementalStats.AnalysisScore += commit.Stats.AnalysisResult.Score() 
                        - prevCommit.Stats.AnalysisResult.Score();
                    List<string> changes = commit.Stats.AnalysisResult.ChangesFrom(
                        prevCommit.Stats.AnalysisResult);
                    foreach (string change in changes)
                        incrementalStats.AnalysisChanges.Add(new Evaluation.CommitLinkedItem(commit, change));

                    bool fixedBuild = commit.Stats.BuildsAndPassesTests && !prevCommit.Stats.BuildsAndPassesTests &&
                        prevCommit.Author != commit.Author;

                    bool brokeBuild = !commit.Stats.BuildsAndPassesTests && prevCommit.Stats.BuildsAndPassesTests &&
                        prevCommit.Author != commit.Author;

                    incrementalStats.BuildFixed += fixedBuild ? 1 : 0;
                    incrementalStats.BuildBroken += brokeBuild ? 1 : 0;

                    if (commit.Stats.PushedToServer)
                    {
                        if (commit.Stats.BuildsAndPassesTests)
                            incrementalStats.PushedBuilding.Add(commit);
                        else
                            incrementalStats.PushedNonBuilding.Add(commit);
                    }
                }
            }

            //Convert numbers of tests passed to percentages
            if (commits.Count > 0)
            {
                Commit lastCommit = commits[commits.Count - 1];

                if (lastCommit.Stats.UserTestsResults.NumTests > 0)
                    incrementalStats.PassedTestsPercent =
                            100 * (incrementalStats.PassedTestsPercent / (double)lastCommit.Stats.UserTestsResults.NumTests);

                for (int i= 0; i< lastCommit.Stats.DeadlineTestsResults.Count; i++)
                {
                    if (incrementalStats.PassedDeadlineTestsPercent.Count == i)
                        incrementalStats.PassedDeadlineTestsPercent.Add(0);

                    if (lastCommit.Stats.DeadlineTestsResults[i].NumTests > 0)
                        incrementalStats.PassedDeadlineTestsPercent[i] = 100 * (incrementalStats.PassedDeadlineTestsPercent[i] 
                            / lastCommit.Stats.DeadlineTestsResults[i].NumTests);
                    else
                        incrementalStats.PassedDeadlineTestsPercent[i] = 0;
                }
            }
            return incrementalStats;
        }

        public Commit Predecessor(List<Commit> commits)
        {
            if (Parents?.Count == 0)
                return null;

            if (Parents?.Count == 1)
                return commits.Find(c => c.Id == Parents[0]);

            //I think the right thing to do here is to return null if there are multiple parent
            //commits. Predecessors are only interesting for calculating incremental stats, and
            //in a merge commit, the most fair thing to do is to compare with the same commit so 
            //that no incremental merit is added / removed
            /*
            foreach (string parentId in Parents)
            {
                Commit parent = commits.Find(c => c.Id == parentId);
                if (parent != null && parent.Author == Author)
                    return parent;
            }*/
            return null;
        }

        public Commit BuildingPredecessor(List<Commit> commits)
        {
            Commit parent = Predecessor(commits);

            while (parent != null && !parent.Stats.Builds)
            {
                parent = parent.Predecessor(commits);
            }
            return parent;
        }

        public static int PeriodWithCommitsPercent(List<Commit> commits, string author, DateTime from, double periodLengthInDays)
        {
            int numPeriodsAuthor = NumPeriodsWithCommits(commits.FindAll(c => c.Author == author), from, periodLengthInDays, true);
            int numPeriods = NumPeriodsWithCommits(commits, from, periodLengthInDays, false);
            if (numPeriods == 0)
                return 0;
            return (int) (100 * (numPeriodsAuthor / (double)numPeriods));
        }

        public static int NumPeriodsWithCommits(List<Commit> commits, DateTime from, double periodLengthInDays, bool countOnlyWithCommits)
        {
            int numPeriods = 0;
            DateTime to = from + TimeSpan.FromDays(periodLengthInDays);

            DateTime endDate = Program.Config.TermEndDate;
            if (DateTime.Now < endDate)
                endDate = DateTime.Now;

            while (from < endDate)
            {
                if (countOnlyWithCommits)
                {
                    List<Commit> authorCommits = commits.FindAll(c => c.Date >= from && c.Date <= to);

                    if (authorCommits.Count > 0)
                        numPeriods++;
                }
                else
                {
                    //Skip halweeks marked as holiday
                    double daysSinceStart = (from - Program.Config.TermStartDate).TotalDays;
                    int halfweekIndex = 1 + (int)(daysSinceStart / periodLengthInDays);
                    if (!Program.Config.HolidayHalfWeeks.Contains(halfweekIndex))
                        numPeriods++;
                }

                from += TimeSpan.FromDays(periodLengthInDays);
                to += TimeSpan.FromDays(periodLengthInDays);
            }
            return numPeriods;
        }

        public static Evaluation.PeriodsWithoutCommits PeriodsWithoutCommits(List<Commit> commits, DateTime from, double periodLengthInDays)
        {
            List<Evaluation.Period> periods = new List<Evaluation.Period>();
            DateTime to = from + TimeSpan.FromDays(periodLengthInDays);

            DateTime endDate = Program.Config.TermEndDate;
            if (DateTime.Now < endDate)
                endDate = DateTime.Now;

            while (to < endDate || (to - endDate).TotalDays < periodLengthInDays)
            {
                //Skip halweeks marked as holiday
                double daysSinceStart = (from - Program.Config.TermStartDate).TotalDays;
                int halfweekIndex = 1 + (int)(daysSinceStart / periodLengthInDays);
                if (!Program.Config.HolidayHalfWeeks.Contains(halfweekIndex))
                {
                    List<Commit> authorCommits = commits.FindAll(c => c.Date >= from && c.Date <= to);

                    if (authorCommits.Count == 0)
                        periods.Add(new Evaluation.Period() { Start = from, End = to});
                }

                from += TimeSpan.FromDays(periodLengthInDays);
                to += TimeSpan.FromDays(periodLengthInDays);
            }
            return new Evaluation.PeriodsWithoutCommits(periods);
        }
    }
}
