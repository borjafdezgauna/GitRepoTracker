using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using GitRepoTracker;

namespace UnitTests
{
    [TestClass]
    public class CodeAnalysis
    {
        string pascalNamingTestCode = "public class CorrectClass {     public int Property1;     public bool Property2;     public string Property3;     public List<string> Property4;          public void Method1(string arg1)     {     }      public List<string> Method2()     {     } }   public class incorrectClass {     public int property1;     public bool property2;     public string property3;     public List<string> property4;          public void method1(string arg1)     {     }      public List<string> method2()     {     } }";
        string newLineTestCode = "public MethodCorrect() \n{ \n    if (condition) \n    { \n       int x = x+1; \n    } \n    else \n    { \n    int x = x+2; \n    } \n} \n \npublic MethodIncorrect(){ \n    if (condition)    { \n       int x = x+1; \n    } \n    else    { \n    int x = x+2; \n    } \n}\n\nstring harl = $\"{variable}\";\nstring result = database.Select(\"People\", new List() { \"Name\", \"Age\", \"Height\" });\nAssert.Equal(\"{'Name','Age','Height'}...\", result);";
        string singularAndPluralsTestCode = "public class CorrectClass \n{ \n    public int Property1; \n    public bool Property2; \n    public string Property3; \n    public List<string> Property4; \n     \n    public void Method1(string arg1) \n    { \n    } \n \n    public List<string> Method2s() \n    { \n    } \n \n public List<string> IndicesWhereIsTrue() \n    { \n    } \n \n    public string Method2Corrects(List<string> values) \n    { \n    } \n \n    foreach (User user in myUsers) \n    { \n    } \n} \n \n \npublic class incorrectClass \n{ \n    public int Property1s; \n    public bool Property2s; \n    public string Property3s; \n    public List<string> Property4; \n     \n    public void Method1s(string arg1) \n    { \n    } \n \n    public List<string> Method2() \n    { \n    } \n \n    foreach (User users in myUser) \n    { \n    } \n}";


        [TestMethod]
        public void PascalNaming()
        {
            GitRepoTracker.CodeAnalysis.RulePascalNaming rule = new GitRepoTracker.CodeAnalysis.RulePascalNaming();

            double evaluation = rule.Evaluate(pascalNamingTestCode);

            List<string> offendingItems = rule.OffendingItems();

            Assert.AreEqual(7, offendingItems.Count);
            Assert.IsTrue(offendingItems.Contains("incorrectClass"));
            Assert.IsTrue(offendingItems.Contains("property1"));
            Assert.IsTrue(offendingItems.Contains("property2"));
            Assert.IsTrue(offendingItems.Contains("property3"));
            Assert.IsTrue(offendingItems.Contains("property4"));
            Assert.IsTrue(offendingItems.Contains("method1"));
            Assert.IsTrue(offendingItems.Contains("method2"));
        }

        [TestMethod]
        public void NewLineBeforeBraces()
        {
            GitRepoTracker.CodeAnalysis.RuleNewLineBefore rule = new GitRepoTracker.CodeAnalysis.RuleNewLineBefore();

            double evaluation = rule.Evaluate(newLineTestCode);

            List<string> offendingItems = rule.OffendingItems();
            Assert.AreEqual(2, offendingItems.Count);
        }

        [TestMethod]
        public void SingularNames()
        {
            GitRepoTracker.CodeAnalysis.SingularNaming rule = new GitRepoTracker.CodeAnalysis.SingularNaming();

            double evaluation = rule.Evaluate(singularAndPluralsTestCode);

            List<string> offendingItems = rule.OffendingItems();
            Assert.AreEqual(4, offendingItems.Count);

            Assert.IsTrue(offendingItems.Contains("Property1s"));
            Assert.IsTrue(offendingItems.Contains("Property2s"));
            Assert.IsTrue(offendingItems.Contains("Property3s"));
            Assert.IsTrue(offendingItems.Contains("Method1s"));
        }

        [TestMethod]
        public void PluralNames()
        {
            GitRepoTracker.CodeAnalysis.PluralNaming rule = new GitRepoTracker.CodeAnalysis.PluralNaming();

            double evaluation = rule.Evaluate(singularAndPluralsTestCode);

            List<string> offendingItems = rule.OffendingItems();
            Assert.AreEqual(2, offendingItems.Count);

            Assert.IsTrue(offendingItems.Contains("Property4"));
            Assert.IsTrue(offendingItems.Contains("Method2"));
        }

        [TestMethod]
        public void ChangesFrom()
        {
            GitRepoTracker.CodeAnalysis.AnalysisResult currentResult =
                new GitRepoTracker.CodeAnalysis.AnalysisResult();
            currentResult.OffendingItems.Add(new GitRepoTracker.CodeAnalysis.AnalysisResultItem()
            {
                Rule = "Rule 1",
                Items = new List<string>() { "Item 1 (Database.cs)", "Item 2 (Filename.cs)" }
            });
            currentResult.OffendingItems.Add(new GitRepoTracker.CodeAnalysis.AnalysisResultItem()
            {
                Rule = "Rule 2"
            });
            GitRepoTracker.CodeAnalysis.AnalysisResult previousResult =
                new GitRepoTracker.CodeAnalysis.AnalysisResult();
            previousResult.OffendingItems.Add(new GitRepoTracker.CodeAnalysis.AnalysisResultItem()
            {
                Rule = "Rule 1",
                Items = new List<string>() { "Item 1 (Database.cs)" }
            });
            previousResult.OffendingItems.Add(new GitRepoTracker.CodeAnalysis.AnalysisResultItem()
            {
                Rule = "Rule 2",
                Items = new List<string>() { "Item 3 (Filename.cs)" }
            });

            List<string> changes = currentResult.ChangesFrom(previousResult);

            Assert.AreEqual(2, changes.Count);
            Assert.IsNotNull(changes.Find(it => it.Contains("broken")));
            Assert.IsNotNull(changes.Find(it => it.Contains("fixed")));
        }

        [TestMethod]
        public void ChangesFrom2()
        {
            GitRepoTracker.CodeAnalysis.AnalysisResult currentResult =
                new GitRepoTracker.CodeAnalysis.AnalysisResult();
            currentResult.OffendingItems.Add(new GitRepoTracker.CodeAnalysis.AnalysisResultItem()
            {
                Rule = "Rule 1",
                Items = new List<string>() { "Item 1 (Database.cs)", "Item 2 (Filename.cs)" }
            });
            currentResult.OffendingItems.Add(new GitRepoTracker.CodeAnalysis.AnalysisResultItem()
            {
                Rule = "Rule 2"
            });
            GitRepoTracker.CodeAnalysis.AnalysisResult previousResult =
                new GitRepoTracker.CodeAnalysis.AnalysisResult();
            previousResult.OffendingItems.Add(new GitRepoTracker.CodeAnalysis.AnalysisResultItem()
            {
                Rule = "Rule 1"
            });
            previousResult.OffendingItems.Add(new GitRepoTracker.CodeAnalysis.AnalysisResultItem()
            {
                Rule = "Rule 2"
            });

            List<string> changes = currentResult.ChangesFrom(previousResult);

            Assert.AreEqual(2, changes.Count);
            Assert.IsNotNull(changes.Find(it => it.Contains("broken")));
            Assert.IsNull(changes.Find(it => it.Contains("fixed")));
        }
    }
}
