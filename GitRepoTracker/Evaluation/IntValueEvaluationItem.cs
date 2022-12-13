using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker.Evaluation
{
    public class IntValueEvaluationItem : EvaluationItem
    {
        int m_value;
        public override string Value() => m_value.ToString();
        
        public override double Score()
        {
            if (m_value >= 0 && Settings.Minimum != 0)
            {
                if (m_value >= Settings.Minimum)
                    return Math.Max(0, Math.Min(MaxScore, Settings.Weight * (0.5 + 
                        0.5 * ((double)m_value - Settings.Minimum) / (Settings.Maximum - Settings.Minimum))));
                return Math.Max(0, Math.Min(MaxScore, Settings.Weight * 0.5 * (m_value / Settings.Minimum)));
            }
            else
                return Math.Min(MaxScore, Math.Max(-MaxScore, Settings.Weight * ((double) m_value / Settings.Maximum)));
        }
        public override bool Pass()
        {
            if (Settings.Minimum == 0)
                return true;
            if (Math.Max(0, m_value) >= Settings.Minimum && Score() >= MaxScore*0.5)
                return true;
            return false;
        }
        
        public IntValueEvaluationItem(string name, int value, IEvaluationSubItem subItems = null)
            :base(name, subItems)
        {
            m_value = value;
        }

        public IntValueEvaluationItem(EvaluationItemSettings settings, int value, IEvaluationSubItem subItems = null)
            : base(settings, subItems)
        {
            m_value = value;
        }
    }
}
