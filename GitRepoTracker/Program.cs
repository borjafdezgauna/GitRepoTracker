using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;

namespace GitRepoTracker
{
    public class Program
    {
        public static string ReportFilename(string project)
        {
            return project.Replace("/", "-") + ".xml";
        }

        public static Config Config = new Config();

        [STAThread]
        static void Main(string[] args)
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Directory.SetCurrentDirectory(directory);
            Console.WriteLine($"Running GitRepoTracker on folder: {directory}");

            string inputFile = null;


            foreach (string arg in args)
            {
                if (arg.StartsWith("-config-file="))
                    inputFile = arg.Substring("-config-file=".Length);
                else if (arg.StartsWith("-full-update"))
                    Config.FullUpdate = true;
                else if (arg.StartsWith("-no-upload"))
                    Config.UploadReports = false;
            }

            if (inputFile == null)
            {
                Console.WriteLine("ERROR. Usage: gitrepotracker -config-file=<config-file>");
                Console.ReadKey();
                return;
            }

            if (!File.Exists(inputFile))
            {
                Console.WriteLine("ERROR. Couldn't find config file");
                Console.ReadKey();
                return;
            }

            Config.LoadXml(inputFile);

            //Download black-box test projects
            CloneTestRepos();

            //Process user repos
            List<Report> reports = ProcessUserRepos();
            foreach (Report report in reports)
            {
                string reportFilename = ReportFilename(report.Group.Project);
                File.WriteAllText(reportFilename, Report.Serialize(report));
            }
            string html = Report.ToHtml(reports, "../../../report-style.css");
            File.WriteAllText("report.html", html);

            if (Config.UploadReports)
            {
                //Upload to GitHub pages
                if (Config.ReportRepo != null)
                {
                    string reportsFolder = Config.ReportFolder;
                    if (string.IsNullOrEmpty(reportsFolder))
                        reportsFolder = "Report";
                    
                    Config.ReportRepo.Clone("GitHubPages", null);
                    Config.ReportRepo.Pull(); //If changes have been made upstream, clone will fail. We need to pull
                    Directory.CreateDirectory($"GitHubPages/{reportsFolder}");
                    File.Copy("report.html", $"GitHubPages/{reportsFolder}/index.html", true);

                    //copy images
                    foreach (string imageFile in Directory.GetFiles(".","*.png"))
                    {
                        File.Copy(imageFile, $"GitHubPages/{reportsFolder}/{imageFile}",true);
                    }
                    
                    Config.ReportRepo.AddAll();
                    Config.ReportRepo.CommitAll("Report updated");
                    Config.ReportRepo.Push();
                }
            }
        }

        static void CloneTestRepos()
        {
            foreach (GitRepo gitRepo in Config.TestRepos)
            {
                gitRepo.Clone(gitRepo.ProjectUsername, Config.Groups.Find(gr => gr.Project == gitRepo.GitHubUserAndProject));
                gitRepo.Pull();
            }
        }
        static List<Report> ProcessUserRepos()
        {
            List<Report> Reports = new List<Report>();
            GitHubClient client = new GitHubClient(Config.GithubUsername, Config.GithubToken);
            CodeAnalysis.Analyzer analyzer = new CodeAnalysis.Analyzer();

            foreach (GitRepo gitRepo in Config.UserRepos)
            {
                StudentGroup group = Config.Groups.Find(gr => gr.Project == gitRepo.GitHubUserAndProject);
                if (group == null)
                {
                    Console.WriteLine($"Group not found in config file: {gitRepo.GitHubUserAndProject}");
                    continue;
                }

                Report report = null;
                if (!Config.FullUpdate && File.Exists(Program.ReportFilename(gitRepo.GitHubUserAndProject)))
                    report = Report.Deserialize<Report>(File.ReadAllText(Program.ReportFilename(gitRepo.GitHubUserAndProject)));
                else
                    report = new Report(group);
                    

                Reports.Add(report);
                report.Group = group;

                GitOutputParser.UnknownUsers.Clear();
                gitRepo.Clone(gitRepo.ProjectUsername, group);

                bool success = gitRepo.CheckoutBranch("master");
                gitRepo.Pull();

                report.MasterHeadCommit = new Commit();
                report.MasterHeadCommit.Stats.Builds = gitRepo.CheckProjectBuilds();

                List<string> pushedCommits = client.GetPushedCommits(gitRepo.ProjectUsername, gitRepo.ProjectName, gitRepo.CurrentBranch).Result;

                gitRepo.Pull();

                //GitOutputParser.UnknownUsers.Clear();
                List<Commit> commits = gitRepo.Commits(group);
                  
                foreach (Commit commit in commits)
                {
                    if (report.Commits.Find(c => c.Id == commit.Id) != null)
                        continue;

                    success = gitRepo.CheckoutCommit(commit.Id);
                    if (success)
                    {
                        commit.Stats.Builds = gitRepo.CheckProjectBuilds();
                        bool lastCommit = commit.Equals(commits[commits.Count - 1]);
                        commit.Stats.PushedToServer = pushedCommits.Contains(commit.Id);
                            
                        report.Commits.Add(commit);

                        //Blame code?
                        if (lastCommit)
                            gitRepo.Blame(group, commit, Config.StartDate);

                        RunUserTests(gitRepo, commit);

                        bool buildsAndPassesTests = commit.Stats.PushedToServer && commit.Stats.UserTestsResults.PercentPassed() == 100;                          

                        //Run black-box tests
                        RunBlackBoxTests(gitRepo, commit);

                        commit.Stats.AnalysisResult = analyzer.Analyze(gitRepo.Folder,
                            gitRepo.CodeFilesInFolder(), "DBManager");
                    }
                    else
                    {
                        Console.WriteLine("Error: not sure what the problem is");
                    }

                    //After processing all commits
                    //1. set unknown users
                    foreach (string unknownUser in GitOutputParser.UnknownUsers)
                    {
                        if (!report.UnknowUsers.Contains(unknownUser))
                            report.UnknowUsers.Add(unknownUser);
                    }
                    GitOutputParser.UnknownUsers.Clear();

                    report.Images.Clear();

                    //2. generate activity plot
                    string imageFile = $"activity-plot-{group.Project.Replace("/", "-").Replace("\\", "-")}.png";
                    Plots.PlotGenerator.UserActivityPlot(report.Commits, imageFile);
                    report.Images.Add(imageFile);

                    //3. generate deadlines plot
                    imageFile = $"deadlines-plot-{group.Project.Replace("/", "-").Replace("\\", "-")}.png";
                    Plots.PlotGenerator.DeadlinesProgressPlot(report.Commits, Config.Deadlines, imageFile);
                    report.Images.Add(imageFile);
                }

                //#if DEBUG
                //#else

                //#endif

                gitRepo.CheckoutBranch("master"); //<-restore master brach so that next pull works
            }

            return Reports;
        }

        private static void RunUserTests(GitRepo gitRepo, Commit commit)
        {
            List<string> projects = gitRepo.GetAllProjects();
            //Watch out! we're assuming that all test projects contain "Test" either in the project's name
            //or in the folder
            projects = projects.FindAll(proj => proj.Contains("Test"));
            TestResults globalResults = new TestResults();
            foreach (string project in projects)
            {
                TestResults projectTestResults = gitRepo.Test(true, project);
                globalResults.Merge(projectTestResults);
            }
            //Run user's tests
            commit.Stats.UserTestsResults = globalResults;
        }

        private static void RunBlackBoxTests(GitRepo gitRepo, Commit commit)
        {
            string mainLibProject = gitRepo.FindProjectByName(Config.TestedProjectName);
            if (!string.IsNullOrEmpty(mainLibProject))
            {
                foreach (GitRepo testRepo in Config.TestRepos)
                {
                    foreach (Evaluation.Deadline deadline in Config.Deadlines)
                    {
                        string blackTestProject = testRepo.FindProjectInFolder(deadline.ProjectFolder);
                        if (!string.IsNullOrEmpty(blackTestProject))
                        {
                            if (!string.IsNullOrEmpty(blackTestProject) && !string.IsNullOrEmpty(mainLibProject))
                                testRepo.AddReferenceToProject(blackTestProject, mainLibProject);

                            if (commit.Date >= deadline.Start)
                            {
                                TestResults testResults = testRepo.Test(false, blackTestProject);
                                if (testResults != null)
                                {
                                    commit.Stats.DeadlineTestsResults.Add(testResults);
                                }
                            }

                            if (!string.IsNullOrEmpty(blackTestProject) && !string.IsNullOrEmpty(mainLibProject))
                                testRepo.RemoveReferenceToProject(blackTestProject, mainLibProject);
                        }
                    }

                    testRepo.Restore();
                }
            }

        }
    }
}
