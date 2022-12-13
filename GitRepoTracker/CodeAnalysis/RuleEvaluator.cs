using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GitRepoTracker.CodeAnalysis
{
    public abstract class RuleEvaluator
    {
        protected List<string> m_offendingItems = new List<string>();
        public double Evaluate(string code)
        {
            m_offendingItems.Clear();

            List<string> regexPatterns = RegexPatterns();

            //Remove comments
            code = Regex.Replace(code, "//[^\n]*\n", "\n");

            foreach (string regexPattern in regexPatterns)
            {
                foreach (Match match in Regex.Matches(code, regexPattern))
                {
                    string processedMatch = ProcessMatch(match);
                    if (processedMatch != null && !m_offendingItems.Contains(processedMatch))
                        m_offendingItems.Add(processedMatch);
                }
            }
            return m_offendingItems.Count;
        }

        protected abstract List<string> RegexPatterns();
        public abstract string UserFriendlyName();
        public abstract string ProcessMatch(Match match);
        public List<string> OffendingItems() => m_offendingItems;
    }
}
