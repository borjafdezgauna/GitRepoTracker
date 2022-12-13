using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker.Evaluation
{
    public class Penalization : EvaluationItem
    {
        int m_value;

        public override string Value() => Math.Abs(m_value).ToString();
        public override double Score()
        {
            return Math.Min(Settings.Maximum, m_value * Settings.Weight);
        }

        public override bool Pass()
        {
            if (m_value > 0)
                return true;
            if (m_value < 0)
                return false;
            return true;
        }
        public Penalization(string name, int value)
            :base(name)
        {
            if (value < 0 && Settings.Maximum > 0)
                throw new Exception("Weight and max penalty need to have the same sign");
            if (value > 0 && Settings.Maximum < 0)
                throw new Exception("Weight and max penalty need to have the same sign");

            m_value = value;
        }
    }
}
