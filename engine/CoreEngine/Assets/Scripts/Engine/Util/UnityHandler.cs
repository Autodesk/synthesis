using System;
using UnityEngine;
using Synthesis.Simulator;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net;
using System.Diagnostics;
using UnityEngine.UI;
using Assets.Scripts.Engine.Util;
using Newtonsoft.Json;

namespace Synthesis.Util
{
    public class UnityHandler : MonoBehaviour
    {
        public MeshRenderer PlaneMeshRenderer;

        private static UnityHandler instance = null;
        private string updater;

        public delegate void CallBack();
        public static event CallBack OnUpdate = () => { };
        public static event CallBack OnFixedUpdate = () => { };
        public static event CallBack OnLateUpdate = () => { };

        void Start()
        {
            _ = SimulatorHandler.Instance;
            DontDestroyOnLoad(gameObject);

            string CurrentVersion = "4.3.3";
            //GameObject.Find("VersionNumber").GetComponent<Text>().text = "Version " + CurrentVersion;

            if (CheckConnection())
            {
                WebClient client = new WebClient();
                ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
                var json = new WebClient().DownloadString("https://raw.githubusercontent.com/Autodesk/synthesis/master/VersionManager.json");
                VersionManager update = JsonConvert.DeserializeObject<VersionManager>(json);
                updater = update.URL;

                var localVersion = new Version(CurrentVersion);
                var globalVersion = new Version(update.Version);

                var check = localVersion.CompareTo(globalVersion);

                if (check < 0)
                {
                    //Auxiliary.FindGameObject("UpdatePrompt").SetActive(true);
                }
            }
        }

        void Update()
        {
            OnUpdate();
        }

        void FixedUpdate()
        {
            OnFixedUpdate();
        }

        void LateUpdate()
        {
            OnLateUpdate();
        }

        public bool CheckConnection()
        {
            try
            {
                WebClient client = new WebClient();
                ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

                using (client.OpenRead("https://raw.githubusercontent.com/Autodesk/synthesis/master/VersionManager.json"))
                {
                    UnityEngine.Debug.Log("Connection To Update Server Established");
                    return true;
                }
            }
            catch
            {
                UnityEngine.Debug.Log("Connection To Update Server Failed");
                return false;
            }
        }

        public bool RemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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

        public static UnityHandler Instance 
        { 
            get {
                if (instance == null) instance = FindObjectOfType<UnityHandler>();
                return instance;
            } 
        }

        public void GetUpdate(bool yes)
        {
            if (yes)
            {
                Process.Start("http://bxd.autodesk.com");
                Process.Start(updater);
            }
            //else Auxiliary.FindObject("UpdatePrompt").SetActive(false);
        }
    }
}