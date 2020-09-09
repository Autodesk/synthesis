using System;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;
using SynthesisAPI.Utilities;

namespace SynthesisCore.Utilities
{
    public static class Updater
    {
        public static bool IsUpdateAvailable { get; private set; }
        private static string _updateUrl;
        private static string _updateVersion;
        private const string CurrentVersion = "5.0.0";

        public static void CheckForUpdate()
        {
            if (HasValidConnection())
            {
                ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
                var json = new WebClient().DownloadString("https://raw.githubusercontent.com/Autodesk/synthesis/master/VersionManager.json");
                SynthesisVersion synthesisUpdate = Toolkit.DeserializeJson<SynthesisVersion>(json);
                if (synthesisUpdate != null)
                {
                    _updateUrl = synthesisUpdate.URL;
                    _updateVersion = synthesisUpdate.Version;

                    var localVersion = new Version(CurrentVersion);
                    var globalVersion = new Version(synthesisUpdate.Version);

                    var versionComparison = localVersion.CompareTo(globalVersion);

                    if (versionComparison < 0)
                    {
                        //Logger.Log("[Updater] Update available");
                        IsUpdateAvailable = true;
                    }
                }
            }
        }

        private static bool HasValidConnection()
        {
            try
            {
                WebClient client = new WebClient();
                ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

                using (client.OpenRead("https://raw.githubusercontent.com/Autodesk/synthesis/master/VersionManager.json"))
                {
                    //Logger.Log("[Updater] Connection to version checker established");
                    return true;
                }
            }
            catch
            {
                Logger.Log("[Updater] Connection to version checker failed", LogLevel.Warning);
                return false;
            }
        }

        private static bool RemoteCertificateValidationCallback(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isValidCertificateChain = true;
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
                            isValidCertificateChain = false;
                        }
                    }
                }
            }
            return isValidCertificateChain;
        }

        public static SynthesisVersion GetUpdateVersion()
        {
            return new SynthesisVersion {Version = _updateVersion, URL = _updateUrl};
        }

        public static SynthesisVersion GetCurrentVersion()
        {
            return new SynthesisVersion { Version = CurrentVersion, URL = "" };
        }

        public class SynthesisVersion
        {
            public string Version { get; set; }
            public string URL { get; set; }
        }

    }
}