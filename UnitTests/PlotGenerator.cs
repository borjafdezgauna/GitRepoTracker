using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using GitRepoTracker;
using System.Threading;

namespace UnitTests
{
    public class STATestMethodAttribute : TestMethodAttribute
    {
        public override TestResult[] Execute(ITestMethod testMethod)
        {
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                return Invoke(testMethod);

            TestResult[] result = null;
            var thread = new Thread(() => result = Invoke(testMethod));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return result;
        }

        private TestResult[] Invoke(ITestMethod testMethod)
        {
            return new[] { testMethod.Invoke(null) };
        }
    }
    [TestClass]
    public class PlotGenerator
    {
        [TestMethod]
        public void ActivityPlotPufflos()
        {
            string xml = System.IO.File.ReadAllText("..\\..\\..\\..\\Data\\Tests\\group3-ABD.xml");
            GitRepoTracker.Report report = GitRepoTracker.Report.Deserialize<GitRepoTracker.Report>(xml);
            GitRepoTracker.Plots.PlotGenerator.UserActivityPlot(report.Commits, "test-plot-3.png");

            Assert.IsTrue(System.IO.File.Exists("test-plot-3.png"));
        }
        [TestMethod]
        public void ActivityPlot()
        {
            List<Commit> commits = new List<Commit>()
            {
                new Commit(){Id = "1234", Author = "Jacinto", Date = new System.DateTime(2020, 3, 1, 8, 0, 10)},
                new Commit(){Id = "1234", Author = "María", Date = new System.DateTime(2020, 3, 1, 8, 30, 56)},
                new Commit(){Id = "1234", Author = "Jacinto", Date = new System.DateTime(2020, 3, 3, 9, 10, 3)},
                new Commit(){Id = "1234", Author = "Pepito", Date = new System.DateTime(2020, 3, 3, 9, 16, 4)},
                new Commit(){Id = "1234", Author = "María", Date = new System.DateTime(2020, 3, 3, 9, 30, 12)},
                new Commit(){Id = "1234", Author = "Jacinto", Date = new System.DateTime(2020, 3, 8, 8, 0, 20)},
                new Commit(){Id = "1234", Author = "María", Date = new System.DateTime(2020, 3, 8, 9, 40, 20)},
                new Commit(){Id = "1234", Author = "Pepito", Date = new System.DateTime(2020, 3, 10, 17, 0, 20)},
                new Commit(){Id = "1234", Author = "Jacinto", Date = new System.DateTime(2020, 3, 10, 17, 32, 10)},
                new Commit(){Id = "1234", Author = "Jacinto", Date = new System.DateTime(2020, 3, 10, 17, 33, 05)},
                new Commit(){Id = "1234", Author = "Jacinto", Date = new System.DateTime(2020, 3, 10, 18, 40, 02)},
                new Commit(){Id = "1234", Author = "Pepito", Date = new System.DateTime(2020, 3, 10, 20, 01, 0)},
                new Commit(){Id = "1234", Author = "Jacinto", Date = new System.DateTime(2020, 3, 15, 8, 20, 26)},
                new Commit(){Id = "1234", Author = "Jacinto", Date = new System.DateTime(2020, 3, 15, 8, 25, 10)},
                new Commit(){Id = "1234", Author = "María", Date = new System.DateTime(2020, 3, 17, 9, 23, 0)},
                new Commit(){Id = "1234", Author = "María", Date = new System.DateTime(2020, 3, 17, 9, 26, 3)},
                new Commit(){Id = "1234", Author = "Jacinto", Date = new System.DateTime(2020, 3, 17, 9, 40, 15)},
                new Commit(){Id = "1234", Author = "Jacinto", Date = new System.DateTime(2020, 3, 17, 10, 0, 2)},
            };

            GitRepoTracker.Plots.PlotGenerator.UserActivityPlot(commits, "test-plot.png");

            Assert.IsTrue(System.IO.File.Exists("test-plot.png"));
        }

        [TestMethod]
        public void DeadlinesPlot()
        {
            string xml = System.IO.File.ReadAllText("..\\..\\..\\..\\Data\\Tests\\group3-ABD.xml");
            GitRepoTracker.Report report = GitRepoTracker.Report.Deserialize<GitRepoTracker.Report>(xml);

            List<GitRepoTracker.Evaluation.Deadline> deadlines = new List<GitRepoTracker.Evaluation.Deadline>()
            {
                new GitRepoTracker.Evaluation.Deadline()
                {
                    Name = "Parser",
                    Start = new System.DateTime(2022, 2, 21),
                    End = new System.DateTime(2022, 3, 2)
                },
                new GitRepoTracker.Evaluation.Deadline()
                {
                    Name = "Queries",
                    Start = new System.DateTime(2022, 2, 21),
                    End = new System.DateTime(2022, 3, 20)
                }
            };

            GitRepoTracker.Plots.PlotGenerator.DeadlinesProgressPlot(report.Commits, deadlines, "test-plot-ii.png");

            Assert.IsTrue(System.IO.File.Exists("test-plot-ii.png"));
        }
    }
}
