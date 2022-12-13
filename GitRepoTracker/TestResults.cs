using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace GitRepoTracker
{
    [Serializable]
    public class TestResults
    {
        [XmlElement]
        public string TestName { get; set; }

        [XmlElement]
        public List<string> Passed { get; set; } = new List<string>();
        [XmlElement]
        public List<string> Failed { get; set; } = new List<string>();

        public int NumTests => Passed.Count + Failed.Count;

        public int PercentPassed()
        {
            if (Passed.Count + Failed.Count == 0)
                return 0;
            return (int) Math.Round(100 * ((double)Passed.Count / (double)NumTests));
        }
    }
}
