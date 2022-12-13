using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker.Evaluation
{
    public class CommitLinkedItem
    {
        public Commit Commit { get; private set; }
        public string Item { get; private set; }

        public CommitLinkedItem (Commit commit, string item)
        {
            Commit = commit;
            Item = item;
        }
    }
    public class CommitLinkedItemsList : IEvaluationSubItem
    {
        string m_label;
        List<CommitLinkedItem> m_items;
        public CommitLinkedItemsList(List<CommitLinkedItem> items, string label)
        {
            m_items = items;
            m_label = label;
        }

        public string Html(StudentGroup group)
        {
            string output = null;
            if (m_items?.Count > 0)
            {
                string divId = Report.RandomDivId();
                output += Report.ToggleSwitch($"{m_items.Count} {m_label}", "reportSubItem", divId);
                output += $"<div id=\"{divId}\" style=\"display:none\">";
                foreach (CommitLinkedItem item in m_items)
                    output += $"<div class=\"reportSubSubItem\">{Report.CommitLinkedItemToHtmlLink(group.Project, item)}</div>";
                output += "</div>";
            }
            return output;
        }
    }
}
