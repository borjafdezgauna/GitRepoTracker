using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker.Evaluation
{
    public class EvaluationItemSettings
    {
        public bool Bonus { get; set; } = false;
        public double Minimum { get; set; }
        public double Maximum { get; set; }
        public string ItemName { get; set; }
        public double Weight { get; set; }
        public DateTime Start { get; set; }
    }
}
