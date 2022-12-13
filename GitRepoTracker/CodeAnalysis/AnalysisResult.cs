using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace GitRepoTracker.CodeAnalysis
{
    [Serializable]
    public class AnalysisResultItem
    {
        [XmlElement]
        public string Rule { get; set; }

        [XmlElement]
        public List<string> Items { get; set; } = new List<string>();
    }
    [Serializable]
    public class AnalysisResult
    {
        [XmlElement]
        public List<AnalysisResultItem> OffendingItems { get; private set; } = new List<AnalysisResultItem>();

        public AnalysisResultItem ByName(string name)
        {
            foreach (AnalysisResultItem item in OffendingItems)
            {
                if (item.Rule == name)
                    return item;
            }
            return null;
        }

        public int NumOffendingItems()
        {
            int numItems = 0;
            foreach (AnalysisResultItem item in OffendingItems)
                numItems += item.Items.Count;
            return numItems;
        }
        public double Score()
        {
            double score = 100.0;
            int maxOffendingItemsPerRule = 4;
            double maxScorePerItem = OffendingItems?.Count > 0 ? score / OffendingItems.Count : 0;
            foreach (AnalysisResultItem item in OffendingItems)
            {
                int numOffendingItems = Math.Min(maxOffendingItemsPerRule, item.Items.Count);
                score -= maxScorePerItem * ((double)numOffendingItems / (double)maxOffendingItemsPerRule);
            }
            return score;
        }

        public List<string> ChangesFrom(AnalysisResult prev)
        {
            List<string> changes = new List<string>();

            //Fixed items
            foreach (AnalysisResultItem prevOffendingItem in prev.OffendingItems)
            {
                AnalysisResultItem currentOffendingItem = OffendingItems.Find(it =>
                    it.Rule == prevOffendingItem.Rule);
                if (currentOffendingItem != null)
                {
                    foreach (string item in prevOffendingItem.Items)
                    {
                        if (!currentOffendingItem.Items.Contains(item))
                            changes.Add($"Rule fixed: {item}");
                    }
                }
            }

            //Broken items
            foreach (AnalysisResultItem current in OffendingItems)
            {
                AnalysisResultItem previous = prev.OffendingItems.Find(it =>
                    it.Rule == current.Rule);
                if (previous != null)
                {
                    foreach (string item in current.Items)
                    {
                        if (!previous.Items.Contains(item))
                            changes.Add($"Rule broken: {item}");
                    }
                }
            }
            return changes;
        }
    }
}
