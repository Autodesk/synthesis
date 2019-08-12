using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace InventorRobotExporter.Utilities
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
            res += "&ds=" + "app";
            var version = RobotExporterAddInServer.Instance.Application.SoftwareVersion;
            res += "&dr=" + "inventor";
            res += "&ck=" + ToUrlString(version.DisplayVersion);
            res += "&cc=" + ToUrlString(version.BuildIdentifier.ToString());
            return res;
        }

        private static async Task PostAsync(string para)
        {
            if (!RobotExporterAddInServer.Instance.AddInSettingsManager.UseAnalytics)
            {
                return;
            }
            await client.PostAsync(para, null);
        }


        public static void StartSession()
        {
            var url = GetBaseURL();
            url += "&sc=start";
            url += "&t=event";
            url += "&ec=Environment";
            url += "&ea=Opened";
            PostAsync(url);
        }

        public static void EndSession()
        {
            var url = GetBaseURL();
            url += "&sc=end";
            url += "&t=event";
            url += "&ec=Environment";
            url += "&ea=Closed";
            PostAsync(url);
        }
        public static void LogPage(string page)
        {
            LogPage("", page);
        }
        
        public static void LogPage(string baseUrl, string page)
        {
            var url = GetBaseURL();
            url += "&t=pageview";
            url += "&dh=inventor";
            url += "&dp=" + ToUrlString(baseUrl.Replace(" ", string.Empty)) + "/" + ToUrlString(page.Replace(" ", string.Empty));
            url += "&dt=" + ToUrlString(page);
            PostAsync(url);
        }

        public static void LogEvent(string catagory, string action, string label = null)
        {
            var url = GetBaseURL();
            url += "&t=event";
            url += "&ec="+ ToUrlString(catagory);
            url += "&ea=" + ToUrlString(action);
            if (label != null) url += "&el=" + ToUrlString(label);
            PostAsync(url);
        }

        private static string ToUrlString(string input)
        {
            return System.Web.HttpUtility.UrlEncode(Encoding.UTF8.GetString(Encoding.Default.GetBytes(input)));
        }

    }
}
