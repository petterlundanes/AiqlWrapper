using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using AiqlWrapper.ObjectBuilder;
using AiqlWrapper.Tables;

namespace AiqlWrapper
{
    public class ApplicationInsightsClient
    {
        public ApplicationInsightsClient(string appid, string apiKey)
        {
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            HttpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
            BaseUrl = $"https://api.applicationinsights.io/beta/apps/{appid}/";

            UseTopOptimization = true;

            //TODO make dynamic over all tables
            Requests = new Query<Request>(new QueryProvider(this));
        }

        public IQueryable<Request> Requests { get; }

        /// <summary>
        /// Try to optimize OrderBy(foo).Take(bar) to Top(bar, foo) calls. Default true
        /// </summary>
        public bool UseTopOptimization { get; set; }

        protected HttpClient HttpClient;
        protected readonly string BaseUrl;

        protected virtual HttpResponseMessage ExecuteRequest(string request)
        {
            var str = $"{BaseUrl}{request}";
            return HttpClient.GetAsync(str).Result;
        }

        public string RunAnalytics(string query)
        {
            var request = $"query?query={WebUtility.UrlEncode(query)}";
            return GetResult(ExecuteRequest(request));
        }

        protected static string GetResult(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                throw new ApplicationInsightsException(response.ReasonPhrase);
            }
        }
    }

    public class ApplicationInsightsException : Exception
    {
        public string Reason { get; }

        public ApplicationInsightsException(string reason) : base(reason)
        {
            Reason = reason;
        }
    }
}
