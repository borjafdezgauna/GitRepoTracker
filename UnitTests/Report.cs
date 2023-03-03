using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using GitRepoTracker;

namespace UnitTests
{
    [TestClass]
    public class Report
    {
        [TestMethod]
        public void CorrectBuildTimePercent()
        {
            List<Commit> commits = new List<Commit>();
            Commit commit = new Commit() { Date = new System.DateTime(2020, 1, 2), Id = "1" };
            commit.Stats.Builds = true;
            commit.Stats.PushedToServer = true;
            commits.Add(commit);
            commit = new Commit() { Date = new System.DateTime(2020, 1, 3), Id = "2" };
            commit.Stats.Builds = true;
            commit.Stats.PushedToServer = true;
            commits.Add(commit);
            commit = new Commit() { Date = new System.DateTime(2020, 1, 5), Id = "3" };
            commit.Stats.Builds = false;
            commit.Stats.PushedToServer = true;
            commits.Add(commit);
            commit = new Commit() { Date = new System.DateTime(2020, 1, 8), Id = "4" };
            commit.Stats.Builds = false;
            commit.Stats.PushedToServer = true;
            commits.Add(commit);
            commit = new Commit() { Date = new System.DateTime(2020, 2, 4), Id = "5" };
            commit.Stats.Builds = true;
            commit.Stats.PushedToServer = true;
            commits.Add(commit);
            commit = new Commit() { Date = new System.DateTime(2020, 2, 5), Id = "6" };
            commit.Stats.Builds = true;
            commit.Stats.PushedToServer = true;
            commits.Add(commit);
            commit = new Commit() { Date = new System.DateTime(2020, 2, 6), Id = "7" };
            commit.Stats.Builds = true;
            commit.Stats.PushedToServer = true;
            commits.Add(commit);
            commit = new Commit() { Date = new System.DateTime(2020, 2, 7), Id = "8" };
            commit.Stats.Builds = true;
            commit.Stats.PushedToServer = true;
            commits.Add(commit);
            Assert.AreEqual(19, Commit.CorrectBuildTimePercent(commits, new System.DateTime(2020, 2, 8)), 0.5);
        }
        
        List<GitRepoTracker.Report> CreateTestReports()
        {
            StudentGroup group = new StudentGroup() { Project = "Group1-Project", Name = "Group1-Name" };
            group.Members.Add(new Student() { Alias = "Pepito", Emails = new List<string>() { "student1@email.com" } });
            group.Members.Add(new Student() { Alias = "Jacinto", Emails = new List<string>() { "student2@email.com" } });
            group.Members.Add(new Student() { Alias = "Maria", Emails = new List<string>() { "student3@email.com" } });
            GitRepoTracker.Report report1 = new GitRepoTracker.Report(group);

            Commit commit;
            
            report1.Images.Add("test-plot.png");

            report1.MasterHeadCommit = new Commit() { Author = "Pepito", Date = new System.DateTime(2022, 1, 2), Id = "1234123411234" };
            report1.MasterHeadCommit.Stats.Builds = true;

            commit = new Commit()
            { 
                Author = "Pepito",
                Date = new System.DateTime(2022, 1, 3),
                Id = "640148ba483de62761579620f37c5b2820ca56be",
                Message = "#1 First commit and counting",
            };
            commit.Stats.Builds = true;
            commit.Stats.UserTestsResults.CoveragePercent = 20;
            commit.Stats.UserTestsResults.Failed = new List<string>() {  };
            commit.Stats.UserTestsResults.Passed = new List<string>() { };
            report1.Commits.Add(commit);
            commit = new Commit()
            {
                Author = "Pepito",
                Date = new System.DateTime(2022, 1, 4),
                Id = "839075b9d5cb746d37c519435f7f73b4938e205b",
                Message = "#2,3 Second commit and counting"
            };
            commit.Stats.Builds = true;
            commit.Stats.UserTestsResults.CoveragePercent = 0;
            commit.Stats.UserTestsResults.Passed = new List<string>() { "Parser.Test1" };
            commit.Stats.UserTestsResults.Failed = new List<string>() { "Parser.Test2" };
            report1.Commits.Add(commit);
            commit = new Commit() 
            {
                Author = "Maria",
                Date = new System.DateTime(2022, 1, 14),
                Id = "f4147de7cc1784b0687f35e3071ffc33fceb229b"
            };
            commit.Stats.Builds = false;
            commit.Stats.UserTestsResults.CoveragePercent = 0.0;
            report1.Commits.Add(commit);
            commit = new Commit()
            {
                Author = "Maria",
                Date = new System.DateTime(2022, 1, 15),
                Id = "0fde84cca1890264af87c769e0038cf9e7e91e98",
                Message = "Merge commit"
            };
            commit.Stats.Builds = true;
            commit.Stats.UserTestsResults.CoveragePercent = 0.0;
            commit.Stats.UserTestsResults.Failed = new List<string>() { "Parser.Test1", "Parser.Test2" };
            report1.Commits.Add(commit);
            commit = new Commit()
            { 
                Author = "Pepito",
                Date = new System.DateTime(2022, 1, 18),
                Id = "00c2fb8de6394b4fd69b9b6965605a999a8d1666",
                Message = "#1 Third commit and counting"
            };
            commit.Stats.Builds = true;
            commit.Stats.UserTestsResults.CoveragePercent = 30;
            report1.Commits.Add(commit);

            commit.Stats.UserTestsResults = new TestResults();
            commit.Stats.UserTestsResults.Passed.Add("Parser.Test1"); 
            commit.Stats.UserTestsResults.Passed.Add("Parser.Test2");
            commit.Stats.UserTestsResults.Passed.Add("Parser.Test3");
            commit.Stats.UserTestsResults.Passed.Add("Parser.Test4");
            TestResults deadlineTestResults = new TestResults();
            commit.Stats.DeadlineTestsResults.Add(deadlineTestResults);
            deadlineTestResults.Passed.Add("Parser.Test1");
            deadlineTestResults.Passed.Add("Parser.Test2");
            deadlineTestResults.Failed.Add("Parser.Test3");
            deadlineTestResults.Passed.Add("Parser.Test4");

            commit.Stats.AuthorStats.Add(new AuthorStats("Pepito")
            {
                NumBlamedLines = 30,
                NumBlamedChars = 400,
                BlamedCodeCharsPercent = 56,
                NumCommits = 12
            });
            commit.Stats.AuthorStats.Add(new AuthorStats("Maria")
            {
                NumBlamedLines = 15,
                NumBlamedChars = 100,
                BlamedCodeCharsPercent = 16,
                NumCommits = 13
            });
            

            return new List<GitRepoTracker.Report>() { report1 };
        }
        [TestMethod]
        public void Serialize()
        {
            GitRepoTracker.Report source = CreateTestReports()[0];
            string xml = GitRepoTracker.Report.Serialize(source);
            GitRepoTracker.Report deserialized = GitRepoTracker.Report.Deserialize<GitRepoTracker.Report>(xml);

            Assert.AreEqual(source.Group.Project, deserialized.Group.Project);
            Assert.AreEqual(source.Commits.Count, deserialized.Commits.Count);
        }

      

    }
    [TestClass]
    public class GitOutputParser
    {
     
        public void ParseLog()
        {
            CommitStats stats = new CommitStats();
            List<Commit> commits = GitRepoTracker.GitOutputParser.ParseCommits(log, null);
            Assert.AreEqual(38, commits.Count);

            commits = GitRepoTracker.GitOutputParser.ParseCommits(logWithStats, null);
            Assert.AreEqual(4, commits.Count);
        }

        
        private const string logWithStats = @"commit f5a1f153e51abd4c16e63848134615925c5a2ba7 (HEAD -> develop, origin/develop)
Author: Garazi Pe<C3><B1>a <garaziupv@gmail.com>
Date:   Mon Feb 22 13:20:34 2021 +0100

    Constructor change ColumnTest

 1 file changed, 1 insertion(+), 1 deletion(-)

commit 2076b690c42295e9a0204696c3e55c6b8f0ecfce
Merge: 1a74a84 3023f1d
Author: Laura <leguiluz005@ikasle.ehu.es>
Date:   Mon Feb 22 13:10:34 2021 +0100

    Merge branch 'ColumnTest' into develop

commit 3023f1dcda3fd64c4a84cca098168e40c585ec5d (origin/ColumnTest)
Author: Laura <leguiluz005@ikasle.ehu.es>
Date:   Mon Feb 22 13:09:54 2021 +0100

    Change reference

 1 file changed, 1 insertion(+), 1 deletion(-)

commit 1a74a84d53d869849dd6dc3ec815415790036ffa
Author: Laura <leguiluz005@ikasle.ehu.es>
Date:   Mon Feb 22 13:07:09 2021 +0100

    Merge commit


";

        private const string log = @"commit b9eb4163056c7109c6590ab3bb8ed3cd669a6513
Author: Álvaro Martínez Gómez <alvamargo@gmail.com>
Date:   Thu Feb 18 13:26:07 2021 +0100

    method delete in table

commit 92b7ee3f99a28a69ff46cd81c4044824dcd9b311
Author: Álvaro Martínez Gómez <alvamargo@gmail.com>
Date:   Thu Feb 18 12:40:13 2021 +0100

    corrections

commit d2320eda6b0e2cad55cbe401c90ed536aedfa605
Merge: 6647e03 c8ab6c0
Author: Álvaro Martínez Gómez <alvamargo@gmail.com>
Date:   Thu Feb 18 12:37:12 2021 +0100

    Merge branch 'develop' of https://github.com/AlvaroMtz22/AdminBD into develop

commit 6647e032c3ee48a046022506119377f546a32b83
Author: Álvaro Martínez Gómez <alvamargo@gmail.com>
Date:   Thu Feb 18 12:36:56 2021 +0100

    class compareWhere done

commit c8ab6c049046429850661913d83bba4947e5c0d2
Author: Unai Ruiz Martínez De Murguía <unairuiz.info@gmail.com>
Date:   Thu Feb 18 12:34:45 2021 +0100

    created getType() of column

commit 79455d5cfb2f0031a82f97bfa93f25aa9d97053f
Author: Álvaro Martínez Gómez <alvamargo@gmail.com>
Date:   Thu Feb 18 12:30:19 2021 +0100

    class CompareWhere created

commit c1eff4cc2c73150c75f62b01d561da1cac311394
Author: Unai Ruiz Martínez De Murguía <unairuiz.info@gmail.com>
Date:   Thu Feb 18 12:05:45 2021 +0100

    corrections to the methods

commit 38061cc7e388e3acfda3ed8e8b468935613ec124
Author: Unai Ruiz Martínez De Murguía <unairuiz.info@gmail.com>
Date:   Thu Feb 18 12:04:40 2021 +0100

    new methods implemented

commit 84f1eb96f69b562905ec0a5ceeb3d1a92aa76399
Author: Unai Ruiz Martínez De Murguía <unairuiz.info@gmail.com>
Date:   Thu Feb 18 11:48:30 2021 +0100

    created new Unai class due to problems with conflicts

commit edd2ab963019f6daf1b106888a06bd41d68b4604
Author: Álvaro Martínez Gómez <alvamargo@gmail.com>
Date:   Mon Feb 15 13:23:04 2021 +0100

    changes in Table and Database

commit 1d528de46b755c6edb8c40f871303d84b2616390
Merge: b0f43e8 9273254
Author: Álvaro Martínez Gómez <alvamargo@gmail.com>
Date:   Mon Feb 15 13:14:50 2021 +0100

    Merge branch 'develop' into alvaro

commit 927325477ece9b25535768df812f7b50a2440ac8
Merge: 9d3e496 61cdec7
Author: Ronny Caiza Guerrero <ronnycaiza@hotmail.com>
Date:   Mon Feb 15 13:07:04 2021 +0100

    Merge branch 'develop' of https://github.com/AlvaroMtz22/AdminBD into develop

commit 9d3e496c2e01f2c6c54d8b913263b5b25da72c23
Author: Ronny Caiza Guerrero <ronnycaiza@hotmail.com>
Date:   Mon Feb 15 13:03:13 2021 +0100

    get method for each class from the solucion

commit 61cdec70043a6bca2def8046f383fc025394bdd9
Author: Aitor Urabain Alvarez de Arcaya <aitoruaa@gmail.com>
Date:   Mon Feb 15 12:54:35 2021 +0100

    unit test made

commit 06c566b1553ceeb5e5372236e6b0e2200f0b4f0a
Author: Ronny Caiza Guerrero <ronnycaiza@hotmail.com>
Date:   Thu Feb 11 13:28:58 2021 +0100

    Adding some provisional changes into the database class

commit b0f43e894e23e4b6aa61f913b9e1d14500195275
Author: Álvaro Martínez Gómez <alvamargo@gmail.com>
Date:   Thu Feb 11 13:28:23 2021 +0100

    changes in class Table

commit 5c0a796059dbb8e405cba0bd01a113e597f02881
Author: Unai Ruiz Martínez De Murguía <unairuiz.info@gmail.com>
Date:   Thu Feb 11 12:57:41 2021 +0100

    structure created

commit b2a80610ef5135188a9bfcd03621ebeecfecab2a
Author: Unai Ruiz Martínez De Murguía <unairuiz.info@gmail.com>
Date:   Thu Feb 11 12:05:42 2021 +0100

    Created new proyect

commit 0cdb947a69349e9ea4f7f986f6f92022a4b80ef0
Author: Ronny Caiza Guerrero <ronnycaiza@hotmail.com>
Date:   Thu Feb 4 12:16:11 2021 +0100

    Changes into Agenda ( Count) and UnitTests

commit f9b6d8c4da4f62e7c810c4a82455f749f6bc581e
Merge: b50e806 6435264
Author: Ronny Caiza Guerrero <ronnycaiza@hotmail.com>
Date:   Thu Feb 4 12:12:15 2021 +0100

    Merge branch 'ronny' into develop

commit 64352645b0cb48a0916eb9b6cadbdb9a32b742f2
Author: Ronny Caiza Guerrero <ronnycaiza@hotmail.com>
Date:   Thu Feb 4 12:10:56 2021 +0100

    Part of the class UnitTest with the creation of the methods addAndCount and toString

commit b50e806509a5919197578aa81f1ed46e9a1b72e4
Author: Unai Ruiz Martínez De Murguía <unairuiz.info@gmail.com>
Date:   Thu Feb 4 12:09:20 2021 +0100

    make class Agenda public

commit 2e96ef351b48c4c04ee842b8c7757de123ede4ff
Merge: 5f08c7e 4957fd4
Author: Álvaro Martínez Gómez <alvamargo@gmail.com>
Date:   Thu Feb 4 12:06:58 2021 +0100

    Merge branch 'alvaro' into develop

commit 4957fd437a567692b5f51eebed1dd348946e0154
Author: Álvaro Martínez Gómez <alvamargo@gmail.com>
Date:   Thu Feb 4 12:05:35 2021 +0100

    contact changed to public

commit 5f08c7e8a68790769af7ab5c54749d02982d0de5
Merge: 3c152d5 8a6bc74
Author: Álvaro Martínez Gómez <alvamargo@gmail.com>
Date:   Thu Feb 4 12:03:14 2021 +0100

    Merge branch 'develop' of https://github.com/AlvaroMtz22/AdminBD into develop

commit 3c152d537d28788c88b70f046b11f6c198b81302
Merge: 7f1e95d 2f546cf
Author: Álvaro Martínez Gómez <alvamargo@gmail.com>
Date:   Thu Feb 4 12:02:54 2021 +0100

    merge commit

commit 8a6bc741e2358ec037fe74737579773725108029
Merge: 7f1e95d 8ab4023
Author: Unai Ruiz Martínez De Murguía <unairuiz.info@gmail.com>
Date:   Thu Feb 4 11:56:13 2021 +0100

    Merge branch 'unai' into develop

commit 8ab4023002659b54c50369e1290045cf4190b20f
Author: Unai Ruiz Martínez De Murguía <unairuiz.info@gmail.com>
Date:   Thu Feb 4 11:55:26 2021 +0100

    class Agenda

commit 7f1e95dc618f4747013787109dd5ce23ed31e820
Author: Aitor Urabain Alvarez de Arcaya <aitoruaa@gmail.com>
Date:   Thu Feb 4 11:49:11 2021 +0100

    program is finished and in develop. Unai please make ur class

commit 2f546cfe2cc38d7484c010b682628a6152200a74
Author: Álvaro Martínez Gómez <alvamargo@gmail.com>
Date:   Thu Feb 4 11:49:04 2021 +0100

    contact class done

commit c24ad598dfd567ac69d2291c139f1244bf7f6551
Merge: dbd7547 8481a09
Author: Aitor Urabain Alvarez de Arcaya <aitoruaa@gmail.com>
Date:   Thu Feb 4 11:47:33 2021 +0100

    Merge branch 'aitor' into develop

commit dbd754708ff741e3b4ced10c9d3c756a26fb8dea
Author: Aitor Urabain Alvarez de Arcaya <aitoruaa@gmail.com>
Date:   Thu Feb 4 11:43:01 2021 +0100

    Main program finished. Unai please work a bit

commit 8481a09bd288b97757f3bd4aa58e69a719711d18
Author: Aitor Urabain Alvarez de Arcaya <aitoruaa@gmail.com>
Date:   Thu Feb 4 11:40:58 2021 +0100

    program class finished.

commit efb394cd74f9d9c6e359274bb8797e1fa169f6fe
Author: Aitor Urabain Alvarez de Arcaya <aitoruaa@gmail.com>
Date:   Mon Feb 1 13:12:14 2021 +0100

    in develop

commit 6cc432253c046dc62e74679a6795b5b679b162e8
Author: Aitor Urabain Alvarez de Arcaya <aitoruaa@gmail.com>
Date:   Mon Feb 1 13:10:28 2021 +0100

    metodos sin implementar

commit 1679e84b5254a6f72d39ccaa48e1143954bb0151
Author: Aitor Urabain Alvarez de Arcaya <aitoruaa@gmail.com>
Date:   Mon Feb 1 12:58:12 2021 +0100

    the classes are made.

commit 2ef2c68fe67ee90b3f47f34d04b6cd4f7394ab8f
Author: Álvaro Martínez Gómez <alvamargo@gmail.com>
Date:   Mon Feb 1 12:28:57 2021 +0100

    First commit

commit 19f2a688536372b806922aa2b1808c47f51cc37f
Author: docencia <docencia@U030512>
Date:   Mon Feb 1 12:25:10 2021 +0100

    Add .gitignore and .gitattributes.";
    }

    
}
