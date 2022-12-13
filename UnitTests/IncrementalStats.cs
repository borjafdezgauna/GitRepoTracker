using GitRepoTracker;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTests
{
    [TestClass]
    public class IncrementalStatsTests
    {

        [TestMethod]
        public void IncrementalTestsCoverage()
        {
            List<Commit> commits = new List<Commit>();
            Commit commit = new Commit() { Author = "Pepito", Id = "1" };
            commit.Stats.Builds = false;
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "2" };
            commit.Stats.Builds = true;
            commit.Parents.Add("1");
            commit.Stats.CoveragePercent = 30;
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "3" };
            commit.Stats.Builds = false;
            commit.Parents.Add("2");
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "4" };
            commit.Stats.Builds = true;
            commit.Parents.Add("3");
            commit.Stats.CoveragePercent = 50;
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "5" };
            commit.Stats.Builds = true;
            commit.Parents.Add("4");
            commit.Stats.CoveragePercent = 40;
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "6" };
            commit.Stats.Builds = true;
            commit.Parents.Add("5");
            commit.Stats.CoveragePercent = 60;
            commits.Add(commit);

            IncrementalStats incrementalStats = Commit.IncrementalStats(commits, "Pepito");
            Assert.AreEqual(30, incrementalStats.CoveragePercent);
        }

        [TestMethod]
        public void IncrementalTestsCoverage2()
        {
            List<Commit> commits = new List<Commit>();
            Commit commit = new Commit() { Author = "Pepito", Id = "1" };
            commit.Stats.Builds = false;
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "2" };
            commit.Stats.Builds = true;
            commit.Parents.Add("1");
            commit.Stats.CoveragePercent = 30;
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "3" };
            commit.Stats.Builds = false;
            commit.Parents.Add("2");
            commits.Add(commit);
            commit = new Commit() { Author = "Maria", Id = "4" };
            commit.Stats.Builds = true;
            commit.Parents.Add("3");
            commit.Stats.CoveragePercent = 50;
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "5" };
            commit.Stats.Builds = true;
            commit.Parents.Add("4");
            commit.Stats.CoveragePercent = 40;
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "6" };
            commit.Stats.Builds = true;
            commit.Parents.Add("5");
            commit.Stats.CoveragePercent = 60;
            commits.Add(commit);

            IncrementalStats incrementalStats = Commit.IncrementalStats(commits, "Pepito");
            Assert.AreEqual(10, incrementalStats.CoveragePercent);

            incrementalStats = Commit.IncrementalStats(commits, "Maria");
            Assert.AreEqual(20, incrementalStats.CoveragePercent);
        }

        [TestMethod]
        public void IncrementalTestsCoverageWithNoImprovementMerges()
        {
            List<Commit> commits = new List<Commit>();
            Commit commit = new Commit() { Author = "Pepito", Id = "1" };
            commit.Stats.Builds = false;
            commits.Add(commit);
            commit = new Commit() { Author = "Maria", Id = "2" };
            commit.Stats.Builds = true;
            commit.Stats.CoveragePercent = 30;
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "3" };
            commit.Stats.Builds = true;
            commit.Parents.Add("1");
            commit.Parents.Add("2");
            commit.Stats.CoveragePercent = 30;
            commits.Add(commit);
            commit = new Commit() { Author = "Maria", Id = "4" };
            commit.Stats.Builds = true;
            commit.Parents.Add("2");
            commit.Stats.CoveragePercent = 50;
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "5" };
            commit.Stats.Builds = true;
            commit.Parents.Add("3");
            commit.Stats.CoveragePercent = 40;
            commits.Add(commit);
            commit = new Commit() { Author = "Maria", Id = "6" };
            commit.Stats.Builds = true;
            commit.Parents.Add("4");
            commit.Parents.Add("5");
            commit.Stats.CoveragePercent = 50;
            commits.Add(commit);

            IncrementalStats incrementalStats = Commit.IncrementalStats(commits, "Pepito");
            Assert.AreEqual(10, incrementalStats.CoveragePercent);

            incrementalStats = Commit.IncrementalStats(commits, "Maria");
            Assert.AreEqual(20, incrementalStats.CoveragePercent);
        }

        [TestMethod]
        public void IncrementalTestsCoverageWithNegativeImprovementMerges()
        {
            List<Commit> commits = new List<Commit>();
            Commit commit = new Commit() { Author = "Pepito", Id = "1" };
            commit.Stats.Builds = false;
            commits.Add(commit);
            commit = new Commit() { Author = "Maria", Id = "2" };
            commit.Stats.Builds = true;
            commit.Stats.CoveragePercent = 30;
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "3" };
            commit.Stats.Builds = true;
            commit.Parents.Add("1");
            commit.Parents.Add("2");
            commit.Stats.CoveragePercent = 30;
            commits.Add(commit);
            commit = new Commit() { Author = "Maria", Id = "4" };
            commit.Stats.Builds = true;
            commit.Parents.Add("2");
            commit.Stats.CoveragePercent = 50;
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "5" };
            commit.Stats.Builds = true;
            commit.Parents.Add("3");
            commit.Stats.CoveragePercent = 40;
            commits.Add(commit);
            commit = new Commit() { Author = "Maria", Id = "6" };
            commit.Stats.Builds = true;
            commit.Parents.Add("4");
            commit.Parents.Add("5");
            commit.Stats.CoveragePercent = 20;
            commits.Add(commit);

            IncrementalStats incrementalStats = Commit.IncrementalStats(commits, "Pepito");
            Assert.AreEqual(10, incrementalStats.CoveragePercent);

            incrementalStats = Commit.IncrementalStats(commits, "Maria");
            Assert.AreEqual(20, incrementalStats.CoveragePercent);
        }

        [TestMethod]
        public void RegularityPaco()
        {
            Program.Config.TermStartDate = new DateTime(2022, 01, 24);
            Program.Config.TermEndDate = new DateTime(2022, 3, 13);
            string xml = System.IO.File.ReadAllText("..\\..\\..\\..\\Data\\Tests\\group1-ABD.xml");
            GitRepoTracker.Report report = GitRepoTracker.Report.Deserialize<GitRepoTracker.Report>(xml);
            int periodPercent = Commit.PeriodWithCommitsPercent(report.Commits, "Paco", new DateTime(2022, 2, 21), 3.5);
            Assert.AreEqual(100, periodPercent);
        }

        [TestMethod]
        public void Regularity()
        {
            Program.Config.TermStartDate = new DateTime(2020, 1, 1);
            Program.Config.TermEndDate = new DateTime(2020, 1, 26);

            //Week      1 2 3 4 5 6 7 8
            //Pepito    y y y y y y y y
            //Maria     y n n y n y n y

            List<Commit> commits = new List<Commit>();
            //Half-week 1
            Commit commit = new Commit() { Author = "Pepito", Date = new DateTime(2020, 1, 1) };
            commits.Add(commit);
            commit = new Commit() { Author = "Maria", Date = new DateTime(2020, 1, 1) };
            commits.Add(commit);
            //Half-week 2
            commit = new Commit() { Author = "Pepito", Date = new DateTime(2020, 1, 4) };
            commits.Add(commit);
            //Half-week 3
            commit = new Commit() { Author = "Pepito", Date = new DateTime(2020, 1, 8) };
            commits.Add(commit);
            //Half-week 4
            commit = new Commit() { Author = "Pepito", Date = new DateTime(2020, 1, 12) };
            commits.Add(commit);
            commit = new Commit() { Author = "Maria", Date = new DateTime(2020, 1, 12) };
            commits.Add(commit);
            //Half-week 5
            commit = new Commit() { Author = "Pepito", Date = new DateTime(2020, 1, 15) };
            commits.Add(commit);
            //Half-week 6
            commit = new Commit() { Author = "Pepito", Date = new DateTime(2020, 1, 19) };
            commits.Add(commit);
            commit = new Commit() { Author = "Maria", Date = new DateTime(2020, 1, 19) };
            commits.Add(commit);
            //Half-week 7
            commit = new Commit() { Author = "Pepito", Date = new DateTime(2020, 1, 22) };
            commits.Add(commit);
            //Half-week 8
            commit = new Commit() { Author = "Pepito", Date = new DateTime(2020, 1, 26) };
            commits.Add(commit);
            commit = new Commit() { Author = "Maria", Date = new DateTime(2020, 1, 26) };
            commits.Add(commit);

            double regularity = Commit.PeriodWithCommitsPercent(commits, "Pepito", new DateTime(2020, 1, 1), 3.5);
            Assert.AreEqual(100, regularity);
            regularity = Commit.PeriodWithCommitsPercent(commits, "Maria", new DateTime(2020, 1, 1), 3.5);
            Assert.AreEqual(50, regularity);

            //Set as holidays two of the weeks maria doesn't have commits
            Program.Config.HolidayHalfWeeks.Add(5);
            Program.Config.HolidayHalfWeeks.Add(7);
            regularity = Commit.PeriodWithCommitsPercent(commits, "Pepito", new DateTime(2020, 1, 1), 3.5);
            Assert.AreEqual(133, regularity);
            regularity = Commit.PeriodWithCommitsPercent(commits, "Maria", new DateTime(2020, 1, 1), 3.5);
            Assert.AreEqual(66, regularity);

            //Set as holidays two more weeks where Maria doesn't have commits
            Program.Config.HolidayHalfWeeks.Add(2);
            Program.Config.HolidayHalfWeeks.Add(3);
            regularity = Commit.PeriodWithCommitsPercent(commits, "Pepito", new DateTime(2020, 1, 1), 3.5);
            Assert.AreEqual(200, regularity);
            regularity = Commit.PeriodWithCommitsPercent(commits, "Maria", new DateTime(2020, 1, 1), 3.5);
            Assert.AreEqual(100, regularity);

            //Set as holidays the last 4 weeks. The two weeks without commits are compensated with two of the holiday weeks
            Program.Config.HolidayHalfWeeks.Clear();
            Program.Config.HolidayHalfWeeks.Add(5);
            Program.Config.HolidayHalfWeeks.Add(6);
            Program.Config.HolidayHalfWeeks.Add(7);
            Program.Config.HolidayHalfWeeks.Add(8);
            regularity = Commit.PeriodWithCommitsPercent(commits, "Pepito", new DateTime(2020, 1, 1), 3.5);
            Assert.AreEqual(200, regularity);
            regularity = Commit.PeriodWithCommitsPercent(commits, "Maria", new DateTime(2020, 1, 1), 3.5);
            Assert.AreEqual(100, regularity);
        }

        [TestMethod]
        public void IncrementalStatsPassedTests()
        {
            List<Commit> commits = new List<Commit>();
            Commit commit = new Commit() { Author = "Pepito", Id = "1" };
            commit.Stats.Builds = true;
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "2" };
            commit.Stats.Builds = true;
            commit.Parents.Add("1");
            commit.Stats.UserTestsResults.Passed = new List<string>() { "Test1" };
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "3" };
            commit.Stats.Builds = false;
            commit.Parents.Add("2");
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "4" };
            commit.Stats.Builds = true;
            commit.Parents.Add("3");
            commit.Stats.UserTestsResults.Passed = new List<string>() { "Test1", "Test2" };
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "5" };
            commit.Stats.Builds = true;
            commit.Parents.Add("4");
            commit.Stats.UserTestsResults.Passed = new List<string>() { "Test1" };
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "6" };
            commit.Stats.Builds = true;
            commit.Parents.Add("5");
            commit.Stats.UserTestsResults.Passed = new List<string>() { "Test1", "Test2", "Test3" };
            commits.Add(commit);

            IncrementalStats incrementalStats = Commit.IncrementalStats(commits, "Pepito");
            Assert.AreEqual(100, incrementalStats.PassedTestsPercent);
        }

        [TestMethod]
        public void IncrementalStatsPassedTests2()
        {
            List<Commit> commits = new List<Commit>();
            Commit commit = new Commit() { Author = "Pepito", Id = "1" };
            commit.Stats.Builds = true;
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "2" };
            commit.Stats.Builds = true;
            commit.Parents.Add("1");
            commit.Stats.UserTestsResults.Passed = new List<string>() { "Test1" };
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "3" };
            commit.Stats.Builds = false;
            commit.Parents.Add("2");
            commits.Add(commit);
            commit = new Commit() { Author = "Maria", Id = "4" };
            commit.Stats.Builds = true;
            commit.Parents.Add("3");
            commit.Stats.UserTestsResults.Passed = new List<string>() { "Test1", "Test2" };
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "5" };
            commit.Stats.Builds = true;
            commit.Parents.Add("4");
            commit.Stats.UserTestsResults.Passed = new List<string>() { "Test1" };
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "6" };
            commit.Stats.Builds = true;
            commit.Parents.Add("5");
            commit.Stats.UserTestsResults.Passed = new List<string>() { "Test1", "Test2", "Test3" };
            commits.Add(commit);

            IncrementalStats incrementalStats = Commit.IncrementalStats(commits, "Pepito");
            Assert.AreEqual(66.66, incrementalStats.PassedTestsPercent, 0.1);
            incrementalStats = Commit.IncrementalStats(commits, "Maria");
            Assert.AreEqual(33.33, incrementalStats.PassedTestsPercent, 0.1);
        }

        [TestMethod]
        public void IncrementalStatsPassedTests3()
        {
            List<Commit> commits = new List<Commit>();
            Commit commit = new Commit() { Author = "Pepito", Id = "1" };
            commit.Stats.Builds = true;
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "2" };
            commit.Stats.Builds = true;
            commit.Parents.Add("1");
            commit.Stats.UserTestsResults.Passed = new List<string>() { "Test1" };
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "3" };
            commit.Stats.Builds = false;
            commit.Parents.Add("2");
            commits.Add(commit);
            commit = new Commit() { Author = "Maria", Id = "4" };
            commit.Stats.Builds = true;
            commit.Parents.Add("3");
            commit.Stats.UserTestsResults.Passed = new List<string>() { "Test1", "Test2" };
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "5" };
            commit.Stats.Builds = true;
            commit.Parents.Add("4");
            commit.Stats.UserTestsResults.Passed = new List<string>() { "Test1", "Test2" };
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "6" };
            commit.Stats.Builds = true;
            commit.Parents.Add("5");
            commit.Stats.UserTestsResults.Passed = new List<string>() { "Test1", "Test2", "Test3" };
            commits.Add(commit);

            IncrementalStats incrementalStats = Commit.IncrementalStats(commits, "Pepito");
            Assert.AreEqual(66.66, incrementalStats.PassedTestsPercent, 0.1);
            incrementalStats = Commit.IncrementalStats(commits, "Maria");
            Assert.AreEqual(33.33, incrementalStats.PassedTestsPercent, 0.1);
        }

        [TestMethod]
        public void IncrementalStatsPassedTestsWithMerges()
        {
            List<Commit> commits = new List<Commit>();
            Commit commit = new Commit() { Author = "Pepito", Id = "1" };
            commit.Stats.Builds = true;
            commits.Add(commit);
            commit = new Commit() { Author = "Maria", Id = "2" };
            commit.Stats.Builds = true;
            commit.Stats.UserTestsResults.Passed = new List<string>() { "Test1" };
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "3" };
            commit.Stats.Builds = true;
            commit.Parents.Add("1");
            commit.Parents.Add("2");
            commits.Add(commit);
            commit = new Commit() { Author = "Maria", Id = "4" };
            commit.Stats.Builds = true;
            commit.Parents.Add("2");
            commit.Stats.UserTestsResults.Passed = new List<string>() { "Test1", "Test2" };
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "5" };
            commit.Stats.Builds = true;
            commit.Parents.Add("3");
            commit.Stats.UserTestsResults.Passed = new List<string>() { "Test3" };
            commits.Add(commit);
            commit = new Commit() { Author = "Maria", Id = "6" };
            commit.Stats.Builds = true;
            commit.Parents.Add("4");
            commit.Parents.Add("5");
            commit.Stats.UserTestsResults.Passed = new List<string>() { "Test1", "Test2", "Test3" };
            commits.Add(commit);

            IncrementalStats incrementalStats = Commit.IncrementalStats(commits, "Pepito");
            Assert.AreEqual(33.33, incrementalStats.PassedTestsPercent, 0.1);
            incrementalStats = Commit.IncrementalStats(commits, "Maria");
            Assert.AreEqual(33.33, incrementalStats.PassedTestsPercent, 0.1);
        }

        [TestMethod]
        public void IncrementalStatsDeadlines()
        {
            List<Commit> commits = new List<Commit>();
            Commit commit = new Commit() { Author = "Pepito", Id = "1" };
            commit.Stats.Builds = true;
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "2" };
            commit.Stats.Builds = true;
            commit.Parents.Add("1");
            commit.Stats.DeadlineTestsResults.Add(new TestResults() { TestName = "Deadline1", Passed = new List<string>() { "Test1" } });
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "3" };
            commit.Stats.Builds = false;
            commit.Parents.Add("2");
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "4" };
            commit.Stats.Builds = true;
            commit.Parents.Add("3");
            commit.Stats.DeadlineTestsResults.Add(new TestResults() { TestName = "Deadline1", Passed = new List<string>() { "Test1", "Test2" } });
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "5" };
            commit.Stats.Builds = true;
            commit.Parents.Add("4");
            commit.Stats.DeadlineTestsResults.Add(new TestResults() { TestName = "Deadline1", Passed = new List<string>() { "Test1" } });
            commits.Add(commit);
            commit = new Commit() { Author = "Pepito", Id = "6" };
            commit.Stats.Builds = true;
            commit.Parents.Add("5");
            commit.Stats.DeadlineTestsResults.Add(new TestResults() { TestName = "Deadline1", Passed = new List<string>() { "Test1", "Test2", "Test3" } });
            commits.Add(commit);

            IncrementalStats incrementalStats = Commit.IncrementalStats(commits, "Pepito");
            Assert.AreEqual(100, incrementalStats.PassedDeadlineTestsPercent[0]);
        }

        [TestMethod]
        public void PushValidPercent()
        {
            IncrementalStats stats = new IncrementalStats("Pepito");
            stats.PushedBuilding.Add(new Commit() { Date = new DateTime(2020, 1, 1) });
            stats.PushedBuilding.Add(new Commit() { Date = new DateTime(2020, 2, 1) });
            stats.PushedBuilding.Add(new Commit() { Date = new DateTime(2020, 3, 1) });
            stats.PushedNonBuilding.Add(new Commit() { Date = new DateTime(2020, 1, 1) });
            stats.PushedNonBuilding.Add(new Commit() { Date = new DateTime(2020, 1, 1) });
            stats.PushedNonBuilding.Add(new Commit() { Date = new DateTime(2020, 1, 1) });
            stats.PushedNonBuilding.Add(new Commit() { Date = new DateTime(2020, 1, 1) });
            stats.PushedNonBuilding.Add(new Commit() { Date = new DateTime(2020, 1, 1) });
            stats.PushedNonBuilding.Add(new Commit() { Date = new DateTime(2020, 1, 1) });

            Assert.AreEqual(100, stats.PushedValidPercent(new DateTime(2020, 2, 1)));
        }
    }
}
