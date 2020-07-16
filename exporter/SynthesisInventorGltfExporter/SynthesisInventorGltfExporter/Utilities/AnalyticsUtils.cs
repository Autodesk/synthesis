using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SynthesisInventorGltfExporter.Properties;

namespace SynthesisInventorGltfExporter.Utilities
{
    public static class AnalyticsUtils
    {
        private const string BASE_URL = "https://www.google-analytics.com/collect";
        private const string TRACKING = "UA-81892961-4";
        
        private static Guid clientId;
        
        private static readonly HttpClient client = new HttpClient();

        static AnalyticsUtils()
        {
            if (Guid.TryParse(Settings.Default.AnalyticsID, out clientId)) return;
            clientId = Guid.NewGuid();
            Settings.Default.AnalyticsID = clientId.ToString();
            Settings.Default.Save();
        }

        private static string GetBaseURL()
        {
            var res = BASE_URL;
            res += "?v=1";
            res += "&tid=" + TRACKING;
            res += "&cid="+ clientId;
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
