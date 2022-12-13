using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker.Evaluation
{
    public class Requirement : EvaluationItem
    {
        bool m_value;

        public override string Value() => m_value.ToString();
        public override double Score()
        {
            return 0.0;
        }
        
        public override bool Pass()
        {
            return m_value;
        }
        public Requirement(string name, bool value)
            :base(name)
        {
            m_value = value;
        }
    }
}
