using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker.CodeAnalysis
{
    public class Analyzer
    {
        List<RuleEvaluator> RuleEvaluators = new List<RuleEvaluator>();
        
        public Analyzer()
        {
            RuleEvaluators.Add(new RulePascalNaming());
            RuleEvaluators.Add(new RuleNewLineBefore());
            RuleEvaluators.Add(new SingularNaming());
            RuleEvaluators.Add(new PluralNaming());
        }

        public AnalysisResult Analyze(string folder, List<string> files, string filterBySubFolder)
        {
            AnalysisResult result = new AnalysisResult();
            foreach (string file in files)
            {
                if (!file.Contains(filterBySubFolder))
                    continue;
                string fileContent = System.IO.File.ReadAllText($"{folder}/{file}");
                foreach (RuleEvaluator evaluator in RuleEvaluators)
                {
                    evaluator.Evaluate(fileContent);

                    List<string> offendingItems = evaluator.OffendingItems();

                    AnalysisResultItem item = result.ByName(evaluator.UserFriendlyName());
                    if (item == null)
                    {
                        item = new AnalysisResultItem() { Rule = evaluator.UserFriendlyName() };
                        result.OffendingItems.Add(item);
                    }

                    foreach (string offendingItem in offendingItems)
                    {
                        string description = $"{offendingItem} ({System.IO.Path.GetFileName(file)})";
                        if (!item.Items.Contains(description))
                            item.Items.Add(description);
                    }
                }
            }
            return result;
        }
    }
}
