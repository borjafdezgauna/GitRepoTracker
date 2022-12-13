using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GitRepoTracker
{
    public class GitHubClient
    {
        HttpClient m_client;
        CookieContainer m_cookies;
        HttpClientHandler m_httpClientHandler;
        string m_username;
        string m_token;

        public GitHubClient(string username, string token)
        {
            m_cookies = new CookieContainer();
            m_httpClientHandler = new HttpClientHandler() { AllowAutoRedirect = false, CookieContainer = m_cookies, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            m_client = new HttpClient(m_httpClientHandler) { Timeout = TimeSpan.FromSeconds(15) };
            m_username = username;
            m_token = token;
        }

        private Dictionary<string, string> AdditionalHeaders()
        {
            string encoded = Base64Encode($"{m_username}:{m_token}");
            return new Dictionary<string, string>()
            {
                { "Authorization", $"Basic {encoded}" },
                { "Accept", "application/vnd.github.v3+json" }
            };
        }

        static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        /*
        public async Task<bool> Authorize()
        {
            string targetUri = $"https://api.github.com/user";
            
            HttpRequestMessage request = Http.CreateGetRequest(targetUri, m_cookies, AdditionalHeaders());
            HttpResponseMessage response = await m_client.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            return true;
        }*/

        private string NextPageLinkFromResponseHeader(HttpResponseMessage response)
        {
            string nextPageUri = null;
            if (response.Headers.TryGetValues("Link", out IEnumerable<string> values))
            {
                foreach (string val in values)
                {
                    string [] valParts = val.Split(',');

                    if (valParts != null)
                    {
                        foreach (string valPart in valParts)
                        {
                            if (!valPart.Contains("\"next\""))
                                continue;

                            int startPos = valPart.IndexOf('<');
                            int endPos = valPart.IndexOf('>');
                            if (startPos >= 0 && endPos > 0)
                            {
                                return valPart.Substring(startPos + 1, endPos - startPos - 1);
                            }
                        }
                    }
                }
            }

            return nextPageUri;
        }

        public async Task<List<string>> GetPushedCommits(string user, string repo, string branch)
        {
            List<string> pushedCommits = new List<string>();
            string targetUri = $"https://api.github.com/repos/{user}/{repo}/events";
            
            Dictionary<string, string> headers = AdditionalHeaders();
            //headers["sha"] = branch;
            headers["per_page"]= "100";

            do
            {
                HttpRequestMessage request = Http.CreateGetRequest(targetUri, m_cookies, headers);
                HttpResponseMessage response = await m_client.SendAsync(request);
                
                //more result pages to process?
                targetUri = NextPageLinkFromResponseHeader(response);

                string json = await response.Content.ReadAsStringAsync();

                pushedCommits.AddRange(GitHubJsonParser.ParsePushedCommits(json, branch));
            } while (targetUri != null);

            return pushedCommits;
        }

        public async Task<List<Commit>> GetCommits(string user, string repo)
        {
            string targetUri = $"https://api.github.com/repos/{user}/{repo}/commits";

            Dictionary<string, string> headers = AdditionalHeaders();
            //headers["sha"] = branch;
            headers["per_page"] = "100";

            List<Commit> commits = new List<Commit>();
            do
            {

                HttpRequestMessage request = Http.CreateGetRequest(targetUri, m_cookies, headers);
                HttpResponseMessage response = await m_client.SendAsync(request);

                targetUri = NextPageLinkFromResponseHeader(response);

                string json = await response.Content.ReadAsStringAsync();

                commits.AddRange(GitHubJsonParser.ParseCommits(json));

            } while (targetUri != null);

            return commits;
        }
    }
}
