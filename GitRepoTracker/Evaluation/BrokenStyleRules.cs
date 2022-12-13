using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker.Evaluation
{
    public class BrokenStyleRules : IEvaluationSubItem
    {
        CodeAnalysis.AnalysisResult m_result;
        public BrokenStyleRules(CodeAnalysis.AnalysisResult result)
        {
            m_result = result;
        }

        public string Html(StudentGroup group)
        {
            string output = null;

            if (m_result == null)
                return output;

            foreach (CodeAnalysis.AnalysisResultItem rule in m_result.OffendingItems)
            {
                int numOffendingItems = rule.Items.Count;

                if (numOffendingItems > 0)
                {
                    string divId = Report.RandomDivId();
                    output += Report.ToggleSwitch($"{numOffendingItems} items broke rule '{rule.Rule}'", "reportSubItem", divId);
                    output += $"<div id=\"{divId}\" style=\"display:none\">";
                    foreach (string item in rule.Items)
                        output += $"<div class=\"reportSubSubItem\">{item}</div>";
                    output += "</div>";
                }
            }

            return output;
        }
    }
}
