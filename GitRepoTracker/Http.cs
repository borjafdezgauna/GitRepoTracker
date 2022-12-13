using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace GitRepoTracker
{
    public class Http
    {
        const int shortenedLinkMaxLength = 40;
        public static string ShortenLink(string uri)
        {
            if (uri.Length < shortenedLinkMaxLength)
                return uri;
            else
                return uri.Substring(0, shortenedLinkMaxLength) + "...";
        }

        const string httpPrefix = "http://";
        const string httpsPrefix = "https://";

        public static string PrefixFromUri(string uri)
        {
            if (uri.StartsWith(httpPrefix)) return httpPrefix;
            else return httpsPrefix;
        }

        public static string FullDomainFromUri(string uri)
        {
            string host = HostFromUri(uri);
            string protocol = PrefixFromUri(uri);
            return protocol + host;
        }

        public static string UriWithoutGetParameters(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                return null;
            int questionMarkPos = uri.IndexOf('?');
            if (questionMarkPos > 0)
                return uri.Substring(0, questionMarkPos);
            return uri;
        }

        public static string FormUriWithGetParameters(string uri, Dictionary<string, string> parameters)
        {
            string fullUri = uri;
            if (!fullUri.EndsWith("?")) fullUri += "?";

            if (parameters == null)
                return fullUri;

            bool first = true;
            foreach(string parameterName in parameters.Keys)
            {
                if (first)
                {
                    fullUri += parameterName + "=" + parameters[parameterName];
                    first = false;
                }
                else
                    fullUri += "&" + parameterName + "=" + parameters[parameterName];
            }
            return fullUri;
        }

        public static string DomainFromUri (string uri)
        {
            //we only leave the uppermost subdomain
            string domain;
            string host = HostFromUri(uri);
            string protocol = PrefixFromUri(uri);
            string[] splitParts = host.Split('.');
            if (splitParts.Length >= 2)
                domain = protocol + splitParts[splitParts.Length - 2] + "." + splitParts[splitParts.Length - 1];
            else domain = uri;
            return domain;
        }
        public static string HostFromUri(string uri)
        {
            string uriServer;

            if (uri.StartsWith(httpPrefix)) uriServer = uri.Substring(httpPrefix.Length);
            else if (uri.StartsWith(httpsPrefix)) uriServer = uri.Substring(httpsPrefix.Length);
            else uriServer = uri;

            int firstSlash = uriServer.IndexOf('/');
            if (firstSlash > 0)
                return uriServer.Substring(0, firstSlash);
            return uriServer;
        }

        public static bool IsRedirection(HttpResponseMessage response)
        {
            int statusCode = (int)response.StatusCode;
            if (statusCode >= 300 && statusCode <= 399)
                return true;
            return false;
        }

        

        public static string RedirectionAbsoluteUri(string targetUri, HttpResponseMessage response)
        {
            if (response == null || response.Headers.Location == null)
                return null;

            string redirectedUri = response.Headers.Location.ToString();

            redirectedUri = redirectedUri.Replace("&amp;", "");

            if (response.Headers.Location.IsAbsoluteUri)
                return redirectedUri;
            else
            {
                redirectedUri = redirectedUri.TrimStart('/');
                return PrefixFromUri(targetUri) + HostFromUri(targetUri) + "/" + redirectedUri;
            }
        }

        static public Dictionary<string, string> ParseGetParametersFromUri(string uri)
        {
            Dictionary<string, string> parsedParameters = new Dictionary<string, string>();

            if (uri == null)
                return parsedParameters;

            int firstQuestionMarkPos = uri.IndexOf('?');
            string getParameters;

            //if there is no '?', then the whole string will be split
            if (firstQuestionMarkPos < 0)
                getParameters = uri;
            else
                getParameters = uri.Substring(firstQuestionMarkPos + 1);

            string[] parameters = getParameters.Split('&');
            foreach (string parameter in parameters)
            {
                string[] parameterParts = parameter.Split('=');
                if (parameterParts.Length == 2)
                {
                    parsedParameters[parameterParts[0]] = HttpUtility.UrlDecode(parameterParts[1]);
                }
            }

            return parsedParameters;
        }

        static string GetCookiesHttpHeader(CookieContainer cookies, string domain)
        {
            if (cookies == null) return null;

            string cookiesAsString = null;
            //Workaround to send the cookies that are not sent automatically (http vs https???)
            CookieCollection wosCookieCollection = cookies.GetCookies(new Uri(domain));

            if (wosCookieCollection != null && wosCookieCollection.Count > 0)
            {
                foreach (Cookie c in wosCookieCollection)
                    cookiesAsString += c.Name + "=" + c.Value + "; ";

                if (cookiesAsString != null)
                    cookiesAsString = cookiesAsString.TrimEnd(';', ' ');
            }
            return cookiesAsString;
        }

        static void AddHttpHeaders(HttpRequestMessage httpRequestMessage, string targetUri,
            CookieContainer cookieContainer, Dictionary<string, string> additionalHeaders= null,
            bool addCookieHeaderIfFound = true)
        {
            httpRequestMessage.Headers.Add("Host", HostFromUri(targetUri));
            httpRequestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:95.0) Gecko/20100101 Firefox/95.0");
            if (additionalHeaders == null || !additionalHeaders.ContainsKey("Accept"))
                httpRequestMessage.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            httpRequestMessage.Headers.Add("Accept-Language", "en-US,en;q=0.5");
            httpRequestMessage.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            httpRequestMessage.Headers.Add("Connection", "keep-alive");

            string cookiesHeader = GetCookiesHttpHeader(cookieContainer, FullDomainFromUri(targetUri));
            if (cookiesHeader != null && addCookieHeaderIfFound)
                httpRequestMessage.Headers.Add("Cookie", cookiesHeader);

            httpRequestMessage.Headers.Add("Upgrade-Insecure-Requests", "1");
            httpRequestMessage.Headers.Add("Cache-Control", "max-age=0, no-cache");
            httpRequestMessage.Headers.Add("Pragma", "no-cache");

            if (additionalHeaders != null)
            {
                foreach (string key in additionalHeaders.Keys)
                    httpRequestMessage.Headers.Add(key, additionalHeaders[key]);
            }
        }

        public static HttpRequestMessage CreatePostRequest(string targetUri, Dictionary<string, string> postParameters, CookieContainer cookieContainer,
            Dictionary<string, string> additionalHttpHeaders = null, bool addCookieHeaderIfFound = false)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, targetUri);

            AddHttpHeaders(httpRequestMessage, targetUri, cookieContainer, additionalHttpHeaders, addCookieHeaderIfFound);
            
            httpRequestMessage.Content = new SafeFormUrlEncodedContent(postParameters, null);

            return httpRequestMessage;
        }

        public static HttpRequestMessage CreatePostRequest(string targetUri, Dictionary<string, string> postParameters
            , Dictionary<string, List<string>> multivaluedPostParameters, CookieContainer cookieContainer
            , Dictionary<string, string> additionalHttpHeaders = null,
            bool addCookieHeaderIfFound = false)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, targetUri);

            AddHttpHeaders(httpRequestMessage, targetUri, cookieContainer, additionalHttpHeaders, addCookieHeaderIfFound);

            httpRequestMessage.Content = new SafeFormUrlEncodedContent(postParameters, multivaluedPostParameters, null);

            return httpRequestMessage;
        }

        public static HttpRequestMessage CreatePostRequest(string targetUri, string postMessageContent, CookieContainer cookieContainer,
            Dictionary<string, string> additionalHttpHeaders = null, string customContentType = null)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, targetUri);

            AddHttpHeaders(httpRequestMessage, targetUri, cookieContainer, additionalHttpHeaders);

            if (customContentType == null)
                httpRequestMessage.Content = new StringContent(postMessageContent);
            else
                httpRequestMessage.Content = new StringContent(postMessageContent, Encoding.UTF8, customContentType);

            return httpRequestMessage;
        }

        public static MultipartFormDataContent CreateUploadFileRequest(string filename, Dictionary<string, string> nonFileContent
            , string fileContentName = "file"
            , Dictionary<string, List<string>> multivaluedContent = null)
        {
            //multi-part request
            MultipartFormDataContent formData = new MultipartFormDataContent();

            //Non-file content parts:
            //1) Content-Disposition: form-data; name="evaluacionPositivaFich.tipoAdjunto" -> D
            //2) Content-Disposition: form-data; name="evaluacionPositivaFich.observaciones" ->
            //3) Content-Disposition: form-data; name="_HDIV_STATE_" -> 12-43-F83A62CCAC340F33323C1CD81B44B97E
            foreach (string key in nonFileContent.Keys)
            {
                byte[] valueAsBytes = null;
                if (nonFileContent[key] != null) valueAsBytes= Encoding.ASCII.GetBytes(nonFileContent[key]);

                HttpContent contentPart = new ByteArrayContent(valueAsBytes);
                contentPart.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = "\"" + key + "\"" };
                formData.Add(contentPart);
            }
            if (multivaluedContent != null)
            {
                foreach (string key in multivaluedContent.Keys)
                {
                    foreach (string valueAsString in multivaluedContent[key])
                    {
                        byte[] valueAsBytes = null;
                        if (multivaluedContent[key] != null) valueAsBytes = Encoding.ASCII.GetBytes(valueAsString);

                        HttpContent contentPart = new ByteArrayContent(valueAsBytes);
                        contentPart.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") { Name = "\"" + key + "\"" };
                        formData.Add(contentPart);
                    }
                }
            }
            //File content part:
            //Content-Disposition: form-data; name="file"; filename="borja-2019-tfg-v.pdf"
            //Content-Type: application/pdf

            HttpContent fileContent = new ByteArrayContent(File.ReadAllBytes(filename));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                FileName = "\"" + Path.GetFileName(filename) + "\"",
                Name = $"\"{fileContentName}\""
            };
            formData.Add(fileContent);
            return formData;
        }

        public static HttpRequestMessage CreateGetRequest(string targetUri, CookieContainer cookieContainer, Dictionary<string, string> additionalHttpHeaders = null)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(targetUri));

            AddHttpHeaders(httpRequestMessage, targetUri, cookieContainer, additionalHttpHeaders);

            return httpRequestMessage;
        }

        public static async Task<HttpResponseMessage> FollowPostMultipartRedirections(HttpClient client, CookieContainer cookies
            , string targetUri, MultipartFormDataContent multipartRequest, Dictionary<string,string> additionalHttpHeaders = null)
        {
            HttpResponseMessage response = await client.PostAsync(targetUri, multipartRequest);
            string responseString;
            while (IsRedirection(response))
            {
                targetUri = Http.RedirectionAbsoluteUri(targetUri, response);
                if (targetUri.Contains("#"))
                    targetUri = targetUri.Substring(0, targetUri.IndexOf("#"));

                responseString = await response.Content.ReadAsStringAsync();

                HttpRequestMessage request = Http.CreateGetRequest(targetUri, cookies, additionalHttpHeaders);
                response = await client.SendAsync(request);
            }
            responseString = await response.Content.ReadAsStringAsync();
            return response;
        }

        public static async Task<HttpResponseMessage> FollowPostRedirections(HttpClient client, CookieContainer cookies, string targetUri
            , Dictionary<string, string> postParameters
            , Dictionary<string, string> additionalHttpHeaders = null, Action<string> contentAction = null,
            bool addCookieHeaderIfFound = false)
        {
            string responseString;

            if (targetUri.Contains("#"))
                targetUri = targetUri.Substring(0, targetUri.IndexOf("#"));
            if (targetUri.Contains("&amp;"))
                targetUri = targetUri.Replace("&amp;","");

            HttpRequestMessage httpRequestMessage = Http.CreatePostRequest(targetUri, postParameters, cookies,  additionalHttpHeaders, addCookieHeaderIfFound);

            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            while (Http.IsRedirection(response))
            {
                targetUri = Http.RedirectionAbsoluteUri(targetUri, response);
                if (targetUri.Contains("#"))
                    targetUri = targetUri.Substring(0, targetUri.IndexOf("#"));

                responseString = await response.Content.ReadAsStringAsync();
                contentAction?.Invoke(responseString);

                httpRequestMessage = Http.CreateGetRequest(targetUri, cookies, additionalHttpHeaders);
                response = await client.SendAsync(httpRequestMessage);
            }
            responseString = await response.Content.ReadAsStringAsync();
            contentAction?.Invoke(responseString);
            return response;
        }

        public static async Task<HttpResponseMessage> FollowPostRedirections(HttpClient client, string targetUri
            , Dictionary<string, string> postParameters, Dictionary<string, List<string>> multivaluedPostParameters = null
            , CookieContainer cookieContainer = null)
        {
            HttpRequestMessage httpRequestMessage = Http.CreatePostRequest(targetUri, postParameters, multivaluedPostParameters, cookieContainer);

            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            while (IsRedirection(response))
            {
                targetUri = RedirectionAbsoluteUri(targetUri, response);

                //Copy cookies from last post request
                string cookiesHeader = GetCookiesHttpHeader(cookieContainer, FullDomainFromUri(targetUri));
                httpRequestMessage = CreateGetRequest(targetUri, cookieContainer
                    , new Dictionary<string, string>() { { "Cookies", cookiesHeader } });

                response = await client.SendAsync(httpRequestMessage);
            }
            return response;
        }

        public static async Task<HttpResponseMessage> FollowGetRedirections(HttpClient client, CookieContainer cookies, string targetUri
            , Dictionary<string,string> additionalHttpHeaders= null, Action<string> contentAction = null, Action<string> onRedirection = null)
        {
            string responseString;
            HttpRequestMessage httpRequestMessage = Http.CreateGetRequest(targetUri, cookies, additionalHttpHeaders);

            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            while (Http.IsRedirection(response))
            {
                targetUri = Http.RedirectionAbsoluteUri(targetUri, response);
                onRedirection?.Invoke(targetUri);

                responseString = await response.Content.ReadAsStringAsync();
                contentAction?.Invoke(responseString);
                 
                httpRequestMessage = Http.CreateGetRequest(targetUri, cookies, additionalHttpHeaders);
                response = await client.SendAsync(httpRequestMessage);
            }
            responseString = await response.Content.ReadAsStringAsync();
            contentAction?.Invoke(responseString);
            return response;
        }

        public static string SetGetRequestParameter(string query, string parameter, string value)
        {
            string output = null;
            System.Text.RegularExpressions.Match match = Regex.Match(query, "&" + parameter + "=([^&]+)");
            if (match.Success)
                output = query.Substring(0, match.Index) + "&" + parameter + "=" + value + query.Substring(match.Index + match.Length);
            else
                output = query + "&" + parameter + "=" + value;

            return output;
        }

        public static async Task<string> SafeReadAsync(HttpResponseMessage response, Encoding encoding = null)
        {
            var ms = new MemoryStream();
            await response.Content.CopyToAsync(ms);
            ms.Seek(0, SeekOrigin.Begin);

            StreamReader reader;
            if (encoding == null)
                reader = new StreamReader(ms);
            else
                reader = new StreamReader(ms, encoding);

            string responseString = reader.ReadToEnd();
            return responseString;
        }

        public static async Task RetryIfFailed<T1,T2>(Func<T1, T2, Task> task, T1 param1, T2 param2, int numRetries = 1)
        {
            try
            {
                await task(param1, param2);
            }
            catch
            {
                if (numRetries > 0)
                    await RetryIfFailed(task, param1, param2, numRetries - 1);
            }
        }

        public static async Task RetryIfFailed<T1, T2, T3>(Func<T1, T2, T3, Task> task,
            T1 param1, T2 param2, T3 param3, int numRetries = 1)
        {
            try
            {
                await task(param1, param2, param3);
            }
            catch
            {
                if (numRetries > 0)
                    await RetryIfFailed(task, param1, param2, param3, numRetries - 1);
            }
        }

        public static async Task<T> RetryIfFailed<T1, T2, T3, T>(Func<T1, T2, T3, Task<T>> task,
            T1 param1, T2 param2, T3 param3, int numRetries = 1)
        {
            try
            {
                return await task(param1, param2, param3);
            }
            catch
            {
                if (numRetries > 0)
                    return await RetryIfFailed(task, param1, param2, param3, numRetries - 1);
            }
            return default(T);
        }

        public static async Task RetryIfFailed<T1>(Func<T1, Task> task, T1 param1, int numRetries = 1)
        {
            try
            {
                await task(param1);
            }
            catch
            {
                if (numRetries > 0)
                    await RetryIfFailed(task, param1, numRetries - 1);
            }
        }

        public static async Task<T> RetryIfFailed<T>(Func<Task<T>> task, int numRetries = 1)
        {
            try
            {
                return await task();
            }
            catch
            {
                if (numRetries > 0)
                    return await RetryIfFailed(task, numRetries - 1);
            }
            

            return default(T);
        }

        public static async Task<T> RetryIfFailed<T1, T>(Func<T1, Task<T>> task, T1 param, int numRetries = 1)
        {
            try
            {
                return await task(param);
            }
            catch
            {
                if (numRetries > 0)
                    return await RetryIfFailed(task, param, numRetries - 1);
            }


            return default(T);
        }
        public static async Task<string> RetryIfFailed(Func<Task<string>> task, int numRetries = 1)
        {
            try
            {
                return await task();
            }
            catch
            {
                if (numRetries > 0)
                    return await RetryIfFailed(task, numRetries - 1);
            }

            return null;
        }
    }
}
