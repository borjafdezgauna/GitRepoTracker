using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTests
{
    [TestClass]
    public class Evaluation
    {
        [TestMethod]
        public void Limits()
        {
            GitRepoTracker.Evaluation.EvaluationItem evalItem = 
                new GitRepoTracker.Evaluation.RealValueEvaluationItem(new GitRepoTracker.Evaluation.EvaluationItemSettings()
                { Minimum = 0, Maximum = 10, Weight = 10 }, 100);
            Assert.IsTrue(evalItem.Score() == 10);

            evalItem = new GitRepoTracker.Evaluation.RealValueEvaluationItem(new GitRepoTracker.Evaluation.EvaluationItemSettings()
            { Minimum = 0, Maximum = 10, Weight = 10 }, -100);
            Assert.IsTrue(evalItem.Score() == -10);

            evalItem = new GitRepoTracker.Evaluation.IntValueEvaluationItem(new GitRepoTracker.Evaluation.EvaluationItemSettings()
            { Minimum = 0, Maximum = 10, Weight = 10 }, 100);
            Assert.IsTrue(evalItem.Score() == 10);

            evalItem = new GitRepoTracker.Evaluation.IntValueEvaluationItem(new GitRepoTracker.Evaluation.EvaluationItemSettings()
            { Minimum = 0, Maximum = 10, Weight = 10 }, -100);
            Assert.IsTrue(evalItem.Score() == -10);
        }
    }
}
