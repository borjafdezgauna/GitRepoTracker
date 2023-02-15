using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker.Evaluation
{
    public class GroupEvaluation
    {
        public List<EvaluationItem> EvaluationItems { get; private set; } = new List<EvaluationItem>();

        public GroupEvaluation(Report report)
        {
            if (report != null)
            {
                EvaluationItems.Add(new Requirement("Master branch build",
                    report.MasterHeadCommit.Stats.Builds));
                EvaluationItems.Add(new Requirement("Develop branch build",
                    report.LastCommit.Stats.Builds));
                EvaluationItems.Add(new Requirement("Group tests passed",
                    report.LastCommit.Stats.UserTestsResults.PercentPassed() == 100));
                EvaluationItems.Add(new RealValueEvaluationItem("Group tests coverage %",
                    report.LastCommit.Stats.CoveragePercent));

                for (int i= 0; i< Program.Config.Deadlines.Count; i++)
                {
                    double value = 0;
                    if (i < report.LastCommit.Stats.DeadlineTestsResults.Count)
                        value = report.LastCommit.Stats.DeadlineTestsResults[i].PercentPassed();

                    EvaluationItems.Add(new RealValueEvaluationItem(
                            $"Deadline '{Program.Config.Deadlines[i].Name}' tests passed %",
                            value,
                            new ItemList("failed tests", i < report.LastCommit.Stats.DeadlineTestsResults.Count ? 
                            report.LastCommit.Stats.DeadlineTestsResults[i].Failed : null)));
                }
                EvaluationItems.Add(new RealValueEvaluationItem("Valid master branch build/test time %",
                    Commit.CorrectBuildTimePercent(report.Commits, DateTime.Now)));
                EvaluationItems.Add(new RealValueEvaluationItem("Code style/naming rules %",
                    report.LastCommit.Stats.AnalysisResult != null ? report.LastCommit.Stats.AnalysisResult.Score() : 0,
                    new BrokenStyleRules(report.LastCommit.Stats.AnalysisResult)));
            }
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
            allMinimumsMet &= totalScore >= (maxScore * 0.5);
            allMinimumsMet &= maxScore > 0;

            return Math.Max(0, Math.Min(maxScore, totalScore));
        }
    }
}
