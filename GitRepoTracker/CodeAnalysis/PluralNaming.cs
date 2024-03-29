﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GitRepoTracker.CodeAnalysis
{
    public class PluralNaming : RuleEvaluator
    {
        
        protected override List<string> RegexPatterns()
        {
            return new List<string>()
            {
                @"(?:List\s*<[^>]+>)\s+(((?!s[A-Z])\w)+[^s])[;\(]"
            };
        }
        public override string ProcessMatch(Match match)
        {
            return match.Groups[1].Value;
        }
        public override string UserFriendlyName()
        {
            return "Multi-valued variables (i.e. lists) and multi-valued value-returning methods should end with an 's' (Indices()) " +
                "or should begin with a word ending in 's' (IndicesWhereIsTrue()). It is missleading";
        }
    }
}
