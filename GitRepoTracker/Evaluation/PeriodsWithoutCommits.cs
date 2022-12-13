using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker.Evaluation
{
    public class Period
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
    public class PeriodsWithoutCommits : IEvaluationSubItem
    {
        List<Period> m_periods;
        public PeriodsWithoutCommits(List<Period> periods)
        {
            m_periods = periods;
        }

        public string Html(StudentGroup group)
        {
            string output = null;

            if (m_periods.Count > 0)
            {
                string divId = Report.RandomDivId();
                output += Report.ToggleSwitch($"{m_periods.Count} periods without commits", "reportSubItem", divId);
                output += $"<div id=\"{divId}\" style=\"display:none\">";
                foreach (Period period in m_periods)
                {
                    output += $"<div class=\"reportSubSubItem\">{period.Start}-{period.End}</div>";
                    
                }
                output += "</div>";
            }

            return output;
        }
    }
}
