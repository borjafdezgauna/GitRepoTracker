using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker.Evaluation
{
    public class Bonus : EvaluationItem
    {
        bool m_value;

        public override string Value() => m_value.ToString();
        public override double Score()
        {
            return m_value ? Settings.Weight : 0;
        }

        public override bool Pass()
        {
            return true;
        }
        public Bonus(string name, bool value)
            :base(name)
        {
            m_value = value;
        }
    }
}
