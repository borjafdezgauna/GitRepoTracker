using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker.Evaluation
{
    public class Deadline
    {
        public string Name { get; set; }
        public string ProjectFolder { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
