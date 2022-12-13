using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GitRepoTracker.CodeAnalysis
{
    public class RulePascalNaming : RuleEvaluator
    {
        
        protected override List<string> RegexPatterns()
        {
            return new List<string>()
            {
                @"public\s+(?:interface|class|int|bool|string|List<\w+>|void)\s+([a-z]\w+)",
            };
        }
        public override string ProcessMatch(Match match)
        {
            return match.Groups[1].Value;
        }
        public override string UserFriendlyName()
        {
            return "Public classes/properties/methods must begin with upper case";
        }
    }
}
