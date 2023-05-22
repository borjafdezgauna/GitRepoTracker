using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitRepoTracker
{
    public class GitRepo
    {
        public string ProjectName { get; private set; }
        public string ProjectUsername { get; private set; }
        public string Folder { get; private set; }

        public string CurrentBranch { get; private set; }

        private string m_repoUrl;

        private StudentGroup m_group;
        
        public string GitHubUserAndProject { get; private set; }

        public GitRepo(string githubRepoUserAndProject)
        {
            GitHubUserAndProject = githubRepoUserAndProject;

            int lastSlashPos = githubRepoUserAndProject.LastIndexOf('/');
            if (lastSlashPos >= 0)
            {
                ProjectName = githubRepoUserAndProject.Substring(lastSlashPos + 1);
                ProjectUsername = githubRepoUserAndProject.Substring(0, lastSlashPos);
            }

            m_repoUrl = $"https://github.com/{githubRepoUserAndProject}.git";
        }

        public static void RecursiveDelete(string baseDir)
        {
            if (!Directory.Exists(baseDir))
                return;

            foreach (var file in Directory.GetFiles(baseDir))
            {
                FileAttributes attr = File.GetAttributes(file);
                if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    attr = attr & ~FileAttributes.ReadOnly;
                    File.SetAttributes(file, attr);
                }
            }
            foreach (var dir in Directory.GetDirectories(baseDir, "*.*", SearchOption.AllDirectories))
            {
                RecursiveDelete(dir);
            }
            Directory.Delete(baseDir, true);
        }
        public void Clone(string outputFolder, StudentGroup group)
        {
            m_group = group;

            if (m_repoUrl.StartsWith("https://"))
            {
                m_repoUrl = $"https://{Program.Config.GithubUsername}:{Program.Config.GithubToken}@{m_repoUrl.Substring("https://".Length)}";
            }

            Folder = outputFolder;

            if (File.Exists(outputFolder + "/.git"))
            {
                Console.WriteLine("Deleting local Git repository: " + Folder);
                
                try
                {
                    RecursiveDelete(Folder);
                }
                catch (IOException)
                {
                    Console.WriteLine("Error deleting folder");
                }
            }
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);
            Console.WriteLine("Cloning Git repository: " + m_repoUrl);
            RunCommand($"clone {m_repoUrl} {outputFolder}", "git", false);
        }

        public void Pull()
        {
            //Console.WriteLine("Pulling Git repository: ");
            RunCommand($"pull","git", true);
        }

        public void AddAll()
        {
            RunCommand($"add -A", "git", true);
        }

        public void CommitAll(string message)
        {
            RunCommand($"commit -m \"{message}\"", "git", true);
        }

        public void Push()
        {
            RunCommand($"push", "git", true);
        }

        public bool CheckoutBranch(string branch)
        {
            string branches = RunCommand($"branch --all", "git", true);
            if (!branches.Contains(branch))
                return false;
            string result = RunCommand($"checkout {branch}", "git", true);
            if (result.Contains("error"))
                return false;

            CurrentBranch = branch;
            return true;
        }

        public bool CheckoutCommit(string commitId)
        {
            string result = RunCommand($"checkout {commitId}", "git", true);
            if (result.Contains("fatal error"))
                return false;

            return true;
        }


        public bool CheckProjectBuilds()
        {
            string [] solutionFiles = Directory.GetFiles($"{Folder}/", "*.sln");
            if (solutionFiles == null || solutionFiles.Length == 0)
                return false;
            string output = RunCommand($"build \"{Path.GetFileName(solutionFiles[0].Replace("/","\\"))}\"", "dotnet", true);
            return output.Contains("Build succeeded");
        }

        public TestResults Test(bool calculateCoverage, string testProject)
        {
            string args = string.IsNullOrEmpty(testProject) ?
                "test" : $"test \"{testProject}\"";
            args += calculateCoverage ?
                $" --collect:\"XPlat Code Coverage\" -l \"console;verbosity=normal\"" 
                : $" -l \"console;verbosity=normal\"";

            //Clean test output folder
            string testOutputFolder = Path.GetDirectoryName(testProject);
            string outputFolder = testOutputFolder + "\\bin\\Debug\\net6.0";
            if (!Directory.Exists(outputFolder))
                outputFolder = testOutputFolder + "\\bin\\Debug\\net6";

            //Clean files
            if (Directory.Exists(outputFolder))
            {
                List<string> allowedExtensions = new List<string> { ".json", ".dll", ".pdb", ".exe" };
                foreach (string file in Directory.GetFiles(outputFolder))
                {
                    string fileExtension = Path.GetExtension(file);
                    if (!allowedExtensions.Contains(fileExtension))
                        File.Delete(file);
                }
                //Clean directories
                foreach (string dir in Directory.GetDirectories(outputFolder))
                {
                    Directory.Delete(dir, true);
                }
            }

            string output = RunCommand(args, "dotnet", true);

            return GitOutputParser.ParseTestResults(output, calculateCoverage);
        }

        public string FindProjectByName(string projectName)
        {
            string [] files = Directory.GetFiles(Folder, projectName, SearchOption.AllDirectories);
            if (files.Length > 0)
                return Path.GetFullPath(files[0]);
            return null;
        }

        public string FindProjectInFolder(string projectFolder)
        {
            string folder = Folder + "\\" + projectFolder;
            string[] files = Directory.GetFiles(folder, "*.csproj", SearchOption.AllDirectories);
            if (files.Length > 0)
                return Path.GetFullPath(files[0]);
            return null;
        }

        public List<string> GetAllProjects()
        {
            string[] files = Directory.GetFiles(Folder, "*.csproj", SearchOption.AllDirectories);
            List<string> projects = new List<string>();
            foreach (string file in files)
                projects.Add(Path.GetFullPath(file));
            return projects;
        }

        public void Restore()
        {
            string result = RunCommand($"restore .", "git", true);
        }

        public static IEnumerable<string> GetFiles(string path, string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException("Null extension array");
            IEnumerable<string> files = Enumerable.Empty<string>();
            foreach (string ext in extensions)
            {
                files = files.Concat(Directory.GetFiles(path, ext, SearchOption.AllDirectories));
            }
            return files;
        }

        string ProjectReference(string project)
        {
            return $"  <ItemGroup>\n    <ProjectReference Include=\"{project}\" />\n  </ItemGroup>\n\n";
        }
        public void RemoveReferenceToProject(string blackTestProject, string mainLibProject)
        {
            try
            {
                string projectFile = File.ReadAllText(blackTestProject);
                projectFile = projectFile.Replace(ProjectReference(mainLibProject), "");
                File.WriteAllText(blackTestProject, projectFile);
            }
            catch
            { }
        }

        public void AddReferenceToProject(string blackTestProject, string mainLibProject)
        {
            try
            {
                string projectFile = File.ReadAllText(blackTestProject);
                projectFile = projectFile.Replace("</Project>", ProjectReference(mainLibProject) + "</Project>");
                File.WriteAllText(blackTestProject, projectFile);
            }
            catch
            { }
        }


        public List<string> CodeFilesInFolder()
        {
            string[] extensions = new string[] { ".cpp", ".c", ".cs", ".h" };
            string[] ignoredFiles = new string[] { "AssemblyInfo.cs", "AssemblyAttributes.cs" };

            string solutionFolder = $"{Folder}/";

            string[] files = Directory.GetFiles($"{solutionFolder}", "*.*", SearchOption.AllDirectories);
            List<string> codeFiles = new List<string>();
            foreach (string file in files)
            {
                bool ignore = false;
                foreach (string ignored in ignoredFiles)
                {
                    if (file.EndsWith(ignored))
                    {
                        ignore = true;
                        break;
                    }
                }
                if (ignore)
                    continue;

                foreach (string extension in extensions)
                {
                    if (file.EndsWith(extension))
                    {
                        codeFiles.Add(file.Substring(solutionFolder.Length));
                        break;
                    }
                }
            }
            return codeFiles;
        }
        public List<Commit> Commits(StudentGroup group)
        {
            string output = RunCommand($"log {CurrentBranch} --reverse --parents", "git", true);
            return GitOutputParser.ParseCommits(output, group);
        }

        public void Blame(StudentGroup group, Commit commit, DateTime startDate)
        { 
            List<string> codeFiles = CodeFilesInFolder();

            foreach (string file in codeFiles)
            {
                string fixedFilename = file.Replace("/", "\\");

                string output = RunCommand("blame -w -e " + fixedFilename, "git", true);

                GitOutputParser.ParseBlameOutput(output, commit.Stats, group, startDate);
            }
            //Sum blamed chars/lines
            int numChars = 0;
            int numLines = 0;

            foreach (AuthorStats authorStats in commit.Stats.AuthorStats)
            {
                numChars += authorStats.NumBlamedChars;
                numLines += authorStats.NumBlamedLines;
            }

            foreach (AuthorStats authorStats in commit.Stats.AuthorStats)
            {
                if (authorStats.NumBlamedChars == 0 || numChars == 0)
                    authorStats.BlamedCodeCharsPercent = 0;
                else
                    authorStats.BlamedCodeCharsPercent =
                        (int)(100 * (authorStats.NumBlamedChars) / (double) numChars);

                if (authorStats.NumBlamedLines == 0 || numLines == 0)
                    authorStats.BlamedCodeLinesPercent = 0;
                else
                    authorStats.BlamedCodeLinesPercent =
                        (int)(100 * (authorStats.NumBlamedLines) / (double)numLines);
            }
        }

        private string RunCommand(string arguments, string command = "git", bool setWorkingDirectory = true)
        {
            Console.WriteLine($"{command} {arguments}");
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = command;

            if (setWorkingDirectory)
                startInfo.WorkingDirectory = Folder;
            else
                startInfo.WorkingDirectory = Directory.GetCurrentDirectory();

            startInfo.Arguments = arguments;
            
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.StandardOutputEncoding = new UTF8Encoding(false);

            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            if (string.IsNullOrEmpty(output))
                output = process.StandardError.ReadToEnd();

            process.WaitForExit();

            return output;
        }
    }
}
