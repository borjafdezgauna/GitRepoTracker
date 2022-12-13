using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker.Evaluation
{
    public class ItemList : IEvaluationSubItem
    {
        List<string> m_items;
        string m_label;
        public ItemList(string label, List<string> items)
        {
            m_label = label;
            m_items = items;
        }

        public string Html(StudentGroup group)
        {
            string output = null;
            if (m_items?.Count > 0)
            {
                string divId = Report.RandomDivId();
                output += Report.ToggleSwitch($"{m_items.Count} {m_label}",
                    "reportSubItem", divId);
                output += $"<div id=\"{divId}\" style=\"display:none\">";
                foreach (string item in m_items)
                    output += $"<div class=\"reportSubSubItem\">{item}</div>";
                output += "</div>";
            }
            return output;
        }
    }
}
