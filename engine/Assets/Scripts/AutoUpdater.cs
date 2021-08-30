using Assets.Scripts;
using Newtonsoft.Json;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Net.Security;
using System.Diagnostics;
using Synthesis.UI.Panels;
using Debug = UnityEngine.Debug;

public class AutoUpdater : MonoBehaviour
{
    // public static string updater;
    public const string LocalVersion = "5.0.0.0"; // must be a version value
    public GameObject updaterPanelPrefab;

    // Start is called before the first frame update
    void Start()
    {
        // var versionText = GameObject.Find("VersionNumber").GetComponent<Text>();
        // versionText.text = "Version " + LocalVersion;
        Debug.Log($"Version {LocalVersion} ALPHA");
        // game = GameObject.Find("UpdatePrompt");

        // Analytics For Client Startup
        var init = new AnalyticsEvent(category: "Startup", action: "Launched", label: $"Version {LocalVersion} ALPHA");
        AnalyticsManager.LogEvent(init);
        AnalyticsManager.PostData();

        if (CheckConnection())
        {
            WebClient client = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
            var json = new WebClient().DownloadString("https://raw.githubusercontent.com/Autodesk/synthesis/master/VersionManager.json");
            VersionManager update = JsonConvert.DeserializeObject<VersionManager>(json);
            var updater = update.URL;

            var localVersion = new Version(LocalVersion);
            var globalVersion = new Version(update.Version);

            var check = localVersion.CompareTo(globalVersion);

            if (check < 0) { // if outdated, set update prompt to true
                Debug.Log($"Version {globalVersion.ToString()} available");
                var result = LayoutManager.OpenPanel(updaterPanelPrefab, true);
                if (result.success) { // This really shouldn't ever fail but just in case
                    (result.panel as UpdatePromptPanel).UpdaterLink = updater;
                }
            }
        }
    }

    public bool CheckConnection()
    {
        try
        {
            WebClient client = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;

            using (client.OpenRead("https://raw.githubusercontent.com/Autodesk/synthesis/master/VersionManager.json"))
            {
                return true;
            }
        }
        catch
        {
            return false;
        }
    }

    public bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;
        // If there are errors in the certificate chain, look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                    }
                }
            }
        }
        return isOk;
    }
}