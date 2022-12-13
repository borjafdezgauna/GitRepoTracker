using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GitRepoTracker.CodeAnalysis
{
    public class SingularNaming : RuleEvaluator
    {
        
        protected override List<string> RegexPatterns()
        {
            return new List<string>()
            {
                @"(?:int|bool|string|void)\s+(\w+s)(;|\(([^\)]+)\))",
            };
        }
        public override string ProcessMatch(Match match)
        {
            if (match.Groups.Count == 3)
                return match.Groups[1].Value;
            if (match.Groups.Count == 4 && ( !match.Groups[2].Value.Contains("List")
                 && !match.Groups[2].Value.Contains("[]")) )
                return match.Groups[1].Value;
            return null;
        }
        public override string UserFriendlyName()
        {
            return "Single-valued variables and methods with single-valued inputs/outputs should not end with an 's'. It is missleading";
        }
    }
}
