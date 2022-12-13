using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GitRepoTracker
{
    [Serializable]
    public class CommitStats
    {
        [XmlElement]
        public List<AuthorStats> AuthorStats { get; private set; }= new List<AuthorStats>();

        public AuthorStats StatsByAuthor(string author)
        {
            foreach (AuthorStats stats in AuthorStats)
            {
                if (stats.Author == author)
                    return stats;
            }
            return null;
        }

        [XmlElement]
        public TestResults UserTestsResults { get; set; } = new TestResults() { TestName = "User tests" };

        [XmlElement]
        public List<TestResults> DeadlineTestsResults { get; set; } = new List<TestResults>();

        [XmlElement]
        public bool BuildsAndPassesTests => Builds && UserTestsResults.Failed.Count == 0;

        [XmlElement]
        public double CoveragePercent { get; set; }

        [XmlElement]
        public CodeAnalysis.AnalysisResult AnalysisResult { get; set; } =
            new CodeAnalysis.AnalysisResult();


        public int TotalNumCodeLines()
        {
            int count = 0;
            foreach (AuthorStats stats in AuthorStats)
            {
                count += stats.NumBlamedLines;
            }

            return count;
        }

        public int TotalNumCodeChars()
        {
            int count = 0;
            foreach (AuthorStats stats in AuthorStats)
            {
                count += stats.NumBlamedChars;
            }

            return count;
        }

        [XmlElement]
        public bool Builds { get; set; }

        [XmlElement]
        public bool PushedToServer { get; set; }


        [XmlElement]
        public double PassedTestsAccPercent { get; set; }
        

    }
}
