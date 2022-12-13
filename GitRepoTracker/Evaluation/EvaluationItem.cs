using System;
using System.Collections.Generic;
using System.Text;

namespace GitRepoTracker.Evaluation
{
    public abstract class EvaluationItem
    {
        protected EvaluationItemSettings Settings;
        public IEvaluationSubItem SubItems { get; }
        public EvaluationItem(string itemName, IEvaluationSubItem subItems = null)
        {
            EvaluationItemSettings settings = Program.Config.EvaluationSettings(itemName);

            if (settings == null)
            {
                Console.WriteLine($"Warning: evaluation item \"{itemName}\" not found in configuration file");
                return;
            }
            Settings = settings;
            SubItems = subItems;
        }

        public EvaluationItem(EvaluationItemSettings settings, IEvaluationSubItem subItems = null)
        {
            Settings = settings;
            SubItems = subItems;
        }
        public string Name { get { return Settings?.ItemName; } }
        public bool IsBonus { get { return Settings?.Bonus == true; } }
        public DateTime Start { get { return Settings.Start; } }
        public abstract string Value();

        public string MinimumValue 
        {
            get
            {
                return Settings.Minimum != 0 ? Utils.DoubleToString(Settings.Minimum, 1) : null;
            }
        }
        public string MaximumValue
        {
            get
            {
                return Settings.Maximum != 0 ? Utils.DoubleToString(Settings.Maximum, 1) : null;
            }
        }
        public abstract bool Pass();
        public abstract double Score();
        public double MaxScore { get { return Settings != null ? Settings.Weight : 0.0; } }
    }
}
