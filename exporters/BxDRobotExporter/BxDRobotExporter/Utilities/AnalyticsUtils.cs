using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BxDRobotExporter.Utilities
{
    public static class AnalyticsUtils
    {
        private const string BASE_URL = "https://www.google-analytics.com/collect";
        private const string TRACKING = "UA-81892961-4";
        
        private static string userId = "unknown";
        
        private static readonly HttpClient client = new HttpClient();
        
        public static void SetUser(string user)
        {
            userId = GetHashString(user);
        }

        private static IEnumerable<byte> GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        private static string GetHashString(string inputString)
        {
            var sb = new StringBuilder();
            foreach (var b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        private static string GetBaseURL()
        {
            var res = BASE_URL;
            res += "?v=1";
            res += "&tid=" + TRACKING;
            res += "&uid="+ userId;
            return res;
        }

        private static async Task PostAsync(string para)
        {
            if (!RobotExporterAddInServer.Instance.AddInSettings.UseAnalytics)
            {
                return;
            }
            var values = new Dictionary<string, string>
            {
                { "thing1", "hello" },
                { "thing2", "world" }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(para, content);

            var responseString = await response.Content.ReadAsStringAsync();
        }


        public static void StartSession()
        {
            var url = GetBaseURL();
            url += "&sc=start";
            PostAsync(url);
        }

        public static void EndSession()
        {
            var url = GetBaseURL();
            url += "&sc=end";
            PostAsync(url);
        }
        public static void LogPage(string page)
        {
            var url = GetBaseURL();
            url += "&t=pageview";
            url += "&dh=inventor.plugin";
            url += "&dp=" + page;
            PostAsync(url);
        }

        public static void LogEvent(string catagory, string action, string label, int value = 0)
        {
            var url = GetBaseURL();
            url += "&t=event";
            url += "&ec="+catagory;
            url += "&ea=" + action;
            url += "&el=" + label;
            url += "&ev=" + value;
            PostAsync(url);
        }

    }
}
