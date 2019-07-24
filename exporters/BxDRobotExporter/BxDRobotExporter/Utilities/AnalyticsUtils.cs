using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BxDRobotExporter.ControlGUI;

namespace BxDRobotExporter
{
    public class AnalyticsUtils
    {
        public static string BaseUrl = "https://www.google-analytics.com/collect";
        public static string Tracking = "UA-81892961-4";
        public static string UserId = "unknown";
        private static readonly HttpClient Client = new HttpClient();
        public static void SetUser(string user)
        {
            UserId = GetHashString(user);
        }

        public static byte[] GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }



        private static string GetBaseUrl()
        {
            string res = BaseUrl;
            res += "?v=1";
            res += "&tid=" + Tracking;
            res += "&uid="+ UserId;
            return res;
        }

        public static async Task PostAsync(string para)
        {
            if (!SynthesisGui.PluginSettings.UseAnalytics)
            {
                return;
            }
            var values = new Dictionary<string, string>
            {
                { "thing1", "hello" },
                { "thing2", "world" }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await Client.PostAsync(para, content);

            var responseString = await response.Content.ReadAsStringAsync();
        }


        public static void StartSession()
        {
            string url = GetBaseUrl();
            url += "&sc=start";
            PostAsync(url);
        }

        public static void EndSession()
        {
            string url = GetBaseUrl();
            url += "&sc=end";
            PostAsync(url);
        }
        public static void LogPage(string page)
        {
            string url = GetBaseUrl();
            url += "&t=pageview";
            url += "&dh=inventor.plugin";
            url += "&dp=" + page;
            PostAsync(url);
        }

        public static void LogEvent(string catagory, string action, string label, int value = 0)
        {
            string url = GetBaseUrl();
            url += "&t=event";
            url += "&ec="+catagory;
            url += "&ea=" + action;
            url += "&el=" + label;
            url += "&ev=" + value;
            PostAsync(url);
        }

    }
}
