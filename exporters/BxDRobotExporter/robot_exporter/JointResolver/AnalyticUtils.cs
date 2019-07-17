using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;


    public class AnalyticUtils
    {
        public static string BASE_URL = "https://www.google-analytics.com/collect";
        public static string TRACKING = "UA-73624215-5";
        public static string USER_ID = "unknown";
        private static readonly HttpClient client = new HttpClient();
        public static void SetUser(string user)
        {
            USER_ID = GetHashString(user);
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



        private static string GetBaseURL()
        {
            string res = BASE_URL;
            res += "?v=1";
            res += "&tid=" + TRACKING;
            res += "&uid="+ USER_ID;
            return res;
        }

        public static async Task PostAsync(string para)
        {
        if (!SynthesisGUI.PluginSettings.useAnalytics)
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
            string url = GetBaseURL();
            url += "&sc=start";
            PostAsync(url);
        }

        public static void EndSession()
        {
            string url = GetBaseURL();
            url += "&sc=end";
            PostAsync(url);
        }
        public static void LogPage(string page)
        {
            string url = GetBaseURL();
            url += "&t=pageview";
            url += "&dh=inventor.plugin";
            url += "&dp=" + page;
            PostAsync(url);
        }

        public static void LogEvent(string catagory, string action, string label, int value = 0)
        {
            string url = GetBaseURL();
            url += "&t=event";
            url += "&ec="+catagory;
            url += "&ea=" + action;
            url += "&el=" + label;
            url += "&ev=" + value;
            PostAsync(url);
        }

    }
