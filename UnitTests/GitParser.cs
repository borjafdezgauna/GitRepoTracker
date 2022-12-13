using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using GitRepoTracker;

namespace UnitTests
{
    [TestClass]
    public class GitParser
    {
       

        [TestMethod]
        public void FirstBuildingParent()
        {
            
            List<Commit> commits = new List<Commit>();
            Commit commit1 = new Commit() { Id = "1", Author = "Author1" };
            commits.Add(commit1);
            Commit commit2 = new Commit() { Id = "2", Author = "Author1" };
            commit2.Parents.Add("1");
            commits.Add(commit2);
            Commit commit3 = new Commit() { Id = "3", Author = "Author2" };
            commit3.Parents.Add("2");
            commit3.Stats.Builds = true;
            commits.Add(commit3);
            Commit commit4 = new Commit() { Id = "4", Author = "Author1" };
            commit4.Parents.Add("2");
            commit4.Stats.Builds = true;
            commits.Add(commit4);
            Commit commit5 = new Commit() { Id = "5", Author = "Author2" };
            commit5.Parents.Add("3");
            commit5.Stats.Builds = true;
            commits.Add(commit5);
            Commit commit6 = new Commit() { Id = "6", Author = "Author1" };
            commit6.Parents.Add("4");
            commits.Add(commit6);
            Commit commit7 = new Commit() { Id = "7", Author = "Author2" };
            commit7.Parents.Add("5");
            commit7.Parents.Add("6");
            commit7.Stats.Builds = true;
            commits.Add(commit7);
            Commit commit8 = new Commit() { Id = "8", Author = "Author1" };
            commit8.Parents.Add("7");
            commit8.Stats.Builds = true;
            commits.Add(commit8);
            Commit commit9 = new Commit() { Id = "9", Author = "Author1" };
            commit9.Parents.Add("8");
            commits.Add(commit9);
            Commit commit10 = new Commit() { Id = "10", Author = "Author1" };
            commit10.Parents.Add("7");
            commit10.Parents.Add("9");
            commit10.Stats.Builds = true;
            commits.Add(commit10);

            Assert.AreEqual(null, commit2.BuildingPredecessor(commits));
            Assert.AreEqual(null, commit3.BuildingPredecessor(commits));
            Assert.AreEqual("3", commit5.BuildingPredecessor(commits).Id);
            Assert.AreEqual(null, commit4.BuildingPredecessor(commits));
            Assert.AreEqual("4", commit6.BuildingPredecessor(commits).Id);
            Assert.AreEqual(null, commit7.BuildingPredecessor(commits));
            Assert.AreEqual("7", commit8.BuildingPredecessor(commits).Id);
            Assert.AreEqual("8", commit9.BuildingPredecessor(commits).Id);
            Assert.AreEqual(null, commit10.BuildingPredecessor(commits));
        }

        [TestMethod]
        public void ParseFullLogWithMergeParentsOfDifferentAuthors()
        {
            string log = System.IO.File.ReadAllText("..\\..\\..\\..\\Data\\Tests\\git-log-parents-2.txt");

            StudentGroup group = new StudentGroup() { Project = "Test project" };
            group.Members.Add(new Student() { Alias = "Paco", Emails = new List<string>() { "student1@email.com" } });
            group.Members.Add(new Student() { Alias = "Nayra", Emails = new List<string>() { "student2@email.com" } });
            group.Members.Add(new Student() { Alias = "Ane", Emails = new List<string>() { "student3@email.com" } });
            group.Members.Add(new Student() { Alias = "Noelia", Emails = new List<string>() { "student4@email.com" } });
            List<Commit> commits = GitRepoTracker.GitOutputParser.ParseCommits(log, group);

            Commit commit = commits.Find(c => c.Id == "6e301eb2d2aaed7c0fb66032a19b31a9e31b1d34");
            Commit parent = commit.Predecessor(commits);
            Assert.IsNull(parent);
        }


        [TestMethod]
        public void ParseFullLogWithParentsAndSquashedCommits()
        {
            string log = System.IO.File.ReadAllText("..\\..\\..\\..\\Data\\Tests\\git-log.txt");
            StudentGroup group = new StudentGroup() { Project = "Test project" };
            group.Members.Add(new Student() { Alias = "Arkaitz", Emails = new List<string>() { "student1@email.com" } });
            group.Members.Add(new Student() { Alias = "Julen", Emails = new List<string>() { "student2@email.com" } });
            group.Members.Add(new Student() { Alias = "Ander", Emails = new List<string>() { "student3@email.com", "student3b@email.com" } });
            group.Members.Add(new Student() { Alias = "Julen", Emails = new List<string>() { "student4@email.com" } });
            List<Commit> commits = GitRepoTracker.GitOutputParser.ParseCommits(log, group);
            Assert.AreEqual(167, commits.Count);

            Assert.AreEqual("a6f291f9e39d127e74d8c5a12bd0cf45147eeba3",
                commits.Find(c => c.Id == "8fb720e67eb3aa5d28fc1624fd236e7db15ed085").Predecessor(commits).Id);

            Assert.AreEqual(null, commits.Find(c =>
            c.Id == "625662dd0265f4366f637855df602587537e343f").Predecessor(commits));

            Assert.AreEqual(null, commits.Find(c => 
                c.Id == "183e4c415a1650a7b81a0f7033af59309294f2c8").Predecessor(commits));

            Assert.AreEqual("193074add4af2ff350358fd3ab063da12eaa7b28",
                commits.Find(c => c.Id == "c4a3e31d675091e5108b596f3b84021eb41414b1").Predecessor(commits).Id);

            Assert.AreEqual(2, commits.Find(c => c.Id == "6025baa6614c4af848720df7989b9418d9a3e618").Parents.Count);

            //Check squashed commits have the right parents, (NONE now)
            Assert.AreEqual("c4a3e31d675091e5108b596f3b84021eb41414b1",
                commits.Find(c => c.Id == "faed4f19736695d33d66e0d6d90b29f20a062e93").Predecessor(commits).Id);
            Assert.AreEqual("892f9c12c184af521a56807734b8d85d022a199a",
                commits.Find(c => c.Id == "193074add4af2ff350358fd3ab063da12eaa7b28").Predecessor(commits).Id);
            Assert.AreEqual("8fb720e67eb3aa5d28fc1624fd236e7db15ed085",
                commits.Find(c => c.Id == "54ec1e1591b3980b79184dca5ac52a0711678842").Predecessor(commits).Id);

            //Check commits inside a squashed commits have the right parents
            Assert.AreEqual("35f438014a70357f49e88d354cc0aea84086fb6c",
                commits.Find(c => c.Id == "ba946f1b46f9f98dc9670ac4002fa2ef285794d2").Predecessor(commits).Id);
            Assert.AreEqual("b7feb6dd32bf85b47661c4779ab8f4bbf8c685e7",
                commits.Find(c => c.Id == "b9179213da9ed07fdeb7757142d16a033af565a3").Parents[0]);
            Assert.AreEqual("f868b0a702a9b8528e225fe96f7692cb2b31ed07",
                commits.Find(c => c.Id == "b9179213da9ed07fdeb7757142d16a033af565a3").Parents[1]);

            List<Commit> orphaned = commits.FindAll(c => c.Parents.Count == 0);
            Assert.AreEqual(1, orphaned.Count);
        }
    }
}
