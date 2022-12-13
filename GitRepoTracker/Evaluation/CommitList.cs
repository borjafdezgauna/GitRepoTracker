using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker.Evaluation
{
    public class CommitList : IEvaluationSubItem
    {
        string m_label;
        List<Commit> m_commits;
        public CommitList(List<Commit> wrongPushedCommits, string label = " commits pushed that didn't build/pass tests")
        {
            m_commits = wrongPushedCommits;
            m_label = label;
        }

        public string Html(StudentGroup group)
        {
            string output = null;
            if (m_commits?.Count > 0)
            {
                string divId = Report.RandomDivId();
                output += Report.ToggleSwitch($"{m_commits.Count} {m_label}", "reportSubItem", divId);
                output += $"<div id=\"{divId}\" style=\"display:none\">";
                foreach (Commit commit in m_commits)
                    output += $"<div class=\"reportSubSubItem\">{Report.CommitToHtmlLink(group.Project,commit)}</div>";
                output += "</div>";
            }
            return output;
        }
    }
}
