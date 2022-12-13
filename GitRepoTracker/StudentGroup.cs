using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace GitRepoTracker
{
    [Serializable]
    public class StudentGroup
    {
        [XmlElement]
        public string Name { get; set; }

        [XmlElement]
        public string Project { get; set; }

        [XmlElement]
        public List<Student> Members { get; } = new List<Student>();
    }
}
