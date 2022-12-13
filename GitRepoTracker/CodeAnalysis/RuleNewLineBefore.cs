using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GitRepoTracker.CodeAnalysis
{
    public class RuleNewLineBefore : RuleEvaluator
    {
        
        protected override List<string> RegexPatterns()
        {
            return new List<string>()
            {
                @"\n([^\n\r]+{)[^\n}]*\n",
            };
        }

        public override string ProcessMatch(Match match)
        {
            string item = match.Groups[1].Value;
            string trimmed = item.Trim(' ').Trim('\n').Trim('\r').Trim('\t');
            if (trimmed == "{")
                return null;
            return item;
        }
        public override string UserFriendlyName()
        {
            return "Braces ('{') should begin in a new line unless it is closed in the same code line";
        }
    }
}
