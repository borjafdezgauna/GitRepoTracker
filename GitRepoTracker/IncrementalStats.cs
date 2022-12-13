using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace GitRepoTracker
{
    [Serializable]
    public class IncrementalStats
    {
        [XmlElement]
        public string Author { get; private set; }
        [XmlElement]
        public double PassedTestsPercent { get; set; }

        [XmlElement]
        public List<Evaluation.CommitLinkedItem> PassedTestsChanges { get; set; } = new List<Evaluation.CommitLinkedItem>();

        [XmlElement]
        public double CoveragePercent { get; set; }

        [XmlElement]
        public List<Evaluation.CommitLinkedItem> CoverageChanges { get; set; } = new List<Evaluation.CommitLinkedItem>();

        [XmlElement]
        public double PassedCoveragePercent { get; set; }

        [XmlElement]
        public List<double> PassedDeadlineTestsPercent { get; set; } = new List<double>();

        [XmlElement]
        public List<List<Evaluation.CommitLinkedItem>> DeadlineChanges { get; set; } =
            new List<List<Evaluation.CommitLinkedItem>>();


        [XmlElement]

        public int BuildFixed { get; set; }
        [XmlElement]
        public int BuildBroken { get; set; }
        [XmlElement]
        public List<Commit> PushedNonBuilding { get; set; } = new List<Commit>();
        [XmlElement]
        public List<Commit> PushedBuilding { get; set; } = new List<Commit>();
        [XmlElement]
        public double AnalysisScore { get; set; }
        [XmlElement]
        public List<Evaluation.CommitLinkedItem> AnalysisChanges { get; set; } = new List<Evaluation.CommitLinkedItem>();

        public int PushedValidPercent(DateTime start)
        {
            List<Commit> valid = PushedBuilding.FindAll(c => c.Date >= start);
            List<Commit> invalid = PushedNonBuilding.FindAll(c => c.Date >= start);
            if (valid.Count + invalid.Count == 0)
                return 0;
            return (int)(Math.Round(100*(double) valid.Count / (double)(valid.Count + invalid.Count)));
        }

        public IncrementalStats(string author)
        {
            Author = author;
        }
    }
}
