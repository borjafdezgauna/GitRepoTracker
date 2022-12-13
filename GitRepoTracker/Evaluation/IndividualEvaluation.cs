using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker.Evaluation
{
    public class IndividualEvaluation
    {
        public List<EvaluationItem> EvaluationItems { get; private set; } = new List<EvaluationItem>();

        public IndividualEvaluation(string author, Report report, bool isLeader)
        {
            EvaluationItems.Add(new Bonus("Leader", isLeader));

            if (report.LastCommit.Stats.AuthorStats != null)
            {
                EvaluationItems.Add(new IntValueEvaluationItem("Valid commit messages (+10 chars / issue #) %",
                    Commit.ValidMessagePercent(report.Commits, author,
                    Program.Config.EvaluationSettings("Valid commit messages (+10 chars / issue #) %").Start)));
            }

            IncrementalStats incrementalStats = Commit.IncrementalStats(report.Commits, author);
            
            EvaluationItems.Add(new RealValueEvaluationItem("Contribution to pass tests %",
                incrementalStats.PassedTestsPercent,
                new CommitLinkedItemsList(incrementalStats.PassedTestsChanges, "changes")));
            EvaluationItems.Add(new RealValueEvaluationItem("Contribution to test coverage %",
                incrementalStats.CoveragePercent,
                new CommitLinkedItemsList(incrementalStats.CoverageChanges, "changes")));

            for (int i= 0; i< Program.Config.Deadlines.Count; i++)
            {
                double value = 0;
                IEvaluationSubItem deadlineChanges = null;

                if (i < incrementalStats.PassedDeadlineTestsPercent.Count && i < incrementalStats.DeadlineChanges.Count)
                {
                    value = incrementalStats.PassedDeadlineTestsPercent[i];
                    deadlineChanges = new CommitLinkedItemsList(incrementalStats.DeadlineChanges[i], "changes");
                }
                
                EvaluationItems.Add(new RealValueEvaluationItem($"Contribution to pass Deadline '{Program.Config.Deadlines[i].Name}' tests %",
                    value, deadlineChanges));
            }
            EvaluationItems.Add(new RealValueEvaluationItem("Valid commits pushed %",
                incrementalStats.PushedValidPercent(Program.Config.EvaluationSettings("Valid commits pushed %").Start),
                new CommitList(incrementalStats.PushedNonBuilding.FindAll(
                    c => c.Date >= Program.Config.EvaluationSettings("Valid commits pushed %").Start))));
            EvaluationItems.Add(new RealValueEvaluationItem("Contribution to style/naming rules",
                incrementalStats.AnalysisScore,
                new CommitLinkedItemsList(incrementalStats.AnalysisChanges, "changes")));

            EvaluationItems.Add(new RealValueEvaluationItem("Commit regularity %",
                Commit.PeriodWithCommitsPercent(report.Commits, author,
                    Program.Config.EvaluationSettings("Commit regularity %").Start, 3.5),
                Commit.PeriodsWithoutCommits(report.Commits.FindAll(c=> c.Author == author),
                    Program.Config.EvaluationSettings("Commit regularity %").Start, 3.5)));
        }

        public double TotalScore(out double maxScore, out bool allMinimumsMet)
        {
            double totalScore = 0;
            maxScore = 0;
            allMinimumsMet = true;
            foreach (EvaluationItem item in EvaluationItems)
            {
                if (!item.IsBonus)
                    maxScore += item.MaxScore;
                double score = item.Score();
                if (!item.Pass())
                    allMinimumsMet = false;
                totalScore += score;
            }
            allMinimumsMet &= totalScore >= 0.0;
            allMinimumsMet &= maxScore > 0;
           
            return Math.Max(0, Math.Min(maxScore, totalScore));
        }
    }
}
