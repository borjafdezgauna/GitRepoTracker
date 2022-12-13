using System;
using System.Xml.Serialization;

namespace GitRepoTracker
{
    [Serializable]
    public class AuthorStats
    {
        [XmlElement]
        public string Author { get; set; }
        [XmlElement]
        public int NumCommits { get; set; }

        [XmlElement]
        public int NumBlamedLines { get; set; }
        [XmlElement]
        public int NumBlamedChars { get; set; }

        [XmlElement]
        public int BlamedCodeCharsPercent { get; set; }
        [XmlElement]
        public int BlamedCodeLinesPercent { get; set; }

        public AuthorStats(string author)
        {
            Author = author;
        }

        public AuthorStats() { }
    }
}
