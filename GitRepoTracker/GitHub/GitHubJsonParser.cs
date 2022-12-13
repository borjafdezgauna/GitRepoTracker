using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GitRepoTracker
{
    public class GitHubJsonParser
    {
        public static List<string> ParsePushedCommits(string json, string branch)
        {
            List<string> commits = new List<string>();
            json = json.Replace("\"ref\"", "\"branch\"");
            dynamic jsonObject = JsonConvert.DeserializeObject(json);

            if (jsonObject != null && jsonObject.Type == JTokenType.Array)
            {
                foreach (dynamic pushEvent in jsonObject)
                {
                    string eventType = (string)pushEvent.type;
                    if (eventType != "PushEvent")
                        continue;

                    if (pushEvent.payload != null && pushEvent.payload.head != null)
                    {
                        string commitRef = (string)pushEvent.payload.branch;
                        string commitId = (string) pushEvent.payload.head;

                        if (commitRef.EndsWith(branch))
                            commits.Add(commitId);
                    }
                }
            }

            return commits;
        }

        public static List<Commit> ParseCommits(string json)
        {
            List<Commit> commits = new List<Commit>();

            dynamic jsonObject = JsonConvert.DeserializeObject(json);

            if (jsonObject != null && jsonObject.Type == JTokenType.Array)
            {
                foreach (dynamic commitObj in jsonObject)
                {
                    if (commitObj.sha != null && commitObj.commit != null &&
                        commitObj.commit.author != null &&
                        commitObj.commit.author.email != null)
                    {
                        string committer = (string)commitObj.commit.author.email;
                        string commitId = (string)commitObj.sha;

                        Commit commit = new Commit() { Author = committer, Id = commitId };
                        commits.Add(commit);
                    }
                }
            }

            return commits;
        }
    }
}
