using Assets.Scripts;
using Newtonsoft.Json;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Net.Security;
using System.Diagnostics;

public class AutoUpdater : MonoBehaviour
{

    public static string updater;
    public const string LocalVersion = "5.0.0.0";

    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("VersionNumber").GetComponent<Text>().text = "Version " + LocalVersion;

        if (CheckConnection())
        {
            WebClient client = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
            var json = new WebClient().DownloadString("https://raw.githubusercontent.com/Autodesk/synthesis/master/VersionManager.json");
            VersionManager update = JsonConvert.DeserializeObject<VersionManager>(json);
            updater = update.URL;

            var localVersion = new Version(LocalVersion);
            var globalVersion = new Version(update.Version);

            var check = localVersion.CompareTo(globalVersion);

            if (check < 0) // if outdated, set update prompt to true
            {
                // Auxiliary.FindGameObject
                GameObject.Find("UpdatePrompt").SetActive(true);
            }
            else
            {
                GameObject.Find("UpdatePrompt").SetActive(false);
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

    // Prompt Cancel Button Pressed
    public void CloseUpdatePrompt()
    {
        GameObject.Find("UpdatePrompt").SetActive(false);
    }

    // Prompt Upate Button Pressed
    public void UpdateYes()
    {
        Process.Start(updater);
        if (Application.isEditor)
        {
            GameObject.Find("UpdatePrompt").SetActive(false);
        }
        else
        {
            Application.Quit();
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
