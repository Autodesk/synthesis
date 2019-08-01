using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using Synthesis.FSM;
using System.Reflection;
using Synthesis.States;
using Synthesis.Utils;
using Assets.Scripts.GUI;
using UnityEngine.SceneManagement;
using Synthesis.MixAndMatch;
using System.Net;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Diagnostics;
using Assets.Scripts;

namespace Synthesis.GUI
{
    /// <summary>
    /// This is the class that handles nearly everything within the main menu scene such as ui objects, transitions, and loading fields/robots.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        private GameObject splashScreen;
        private Canvas canvas;
        public string updater;

        GameObject releaseNumber;

        /// <summary>
        /// Runs every frame to update the GUI elements.
        /// </summary>
        void OnGUI()
        {
            //Renders the message manager which displays error messages
            UserMessageManager.Render();
            UserMessageManager.scale = canvas.scaleFactor;
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

        private void Awake()
        {

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

        /// <summary>
        /// Switches to the home tab and its respective UI elements.
        /// </summary>
        public void SwitchTabHome()
        {
            StateMachine.SceneGlobal.ChangeState(new HomeTabState());
        }

        /// <summary>
        /// Switches to the error screen and its respective UI elements.
        /// </summary>
        public void SwitchErrorScreen()
        {
            StateMachine.SceneGlobal.PushState(new ErrorScreenState());
        }

        /// <summary>
        /// Switches to the sim tab and its respective UI elements.
        /// </summary>
        public void SwitchTabSim()
        {
            StateMachine.SceneGlobal.ChangeState(new SimTabState());
        }

        //Exits the program
        public void Exit()
        {
            if (!Application.isEditor)
                System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        /// <summary>
        /// Called at initialization of the MainMenu scene. Initializes all variables and loads pre-existing settings if they exist.
        /// </summary>
        void Start()
        {
            FindAllGameObjects();
            splashScreen.SetActive(true); //Turns on the loading screen while initializing
            LinkTabs();
            UICallbackManager.RegisterButtonCallbacks(StateMachine.SceneGlobal, gameObject);
            UICallbackManager.RegisterDropdownCallbacks(StateMachine.SceneGlobal, gameObject);

            //Creates the replay directory
            FileInfo file = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Replays" + Path.DirectorySeparatorChar);
            file.Directory.Create();

            //Assigns the currently store registry values or default file path to the proper variables if they exist.
            string robotDirectory = PlayerPrefs.GetString("RobotDirectory", (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Robots"));
            string fieldDirectory = PlayerPrefs.GetString("FieldDirectory", (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Fields"));

            //If the directory doesn't exist, create it.
            if (!Directory.Exists(robotDirectory))
            {
                file = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Robots" + Path.DirectorySeparatorChar);
                file.Directory.Create();
                robotDirectory = file.Directory.FullName;
            }
            if (!Directory.Exists(fieldDirectory))
            {
                file = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Fields" + Path.DirectorySeparatorChar);
                file.Directory.Create();
                fieldDirectory = file.Directory.FullName;
            }
            //Saves the current directory information to the registry
            PlayerPrefs.SetString("RobotDirectory", robotDirectory);
            PlayerPrefs.SetString("FieldDirectory", fieldDirectory);

            canvas = GetComponent<Canvas>();

            if (DirectSimulatorLaunch())
                return;

            splashScreen.SetActive(false);

            //This makes it so that if the user exits from the simulator, 
            //they are put into the panel where they can select a robot/field
            //In all other cases, users are welcomed with the main menu screen.
            if (!string.IsNullOrEmpty(AppModel.ErrorMessage))
                SwitchErrorScreen();
            else
                StateMachine.SceneGlobal.PushState(new HomeTabState());

            if (GameObject.Find("NetworkManager") != null)
            {
                StateMachine.SceneGlobal.ChangeState(new SimTabState());
                //UnityEngine.Debug.Log("Load from multiplayer");
            }
        }

        /// <summary>
        /// Links individual tab components with their respective <see cref="State"/>s.
        /// </summary>
        private void LinkTabs()
        {
            LinkTab<HomeTabState>("HomeTab");
            LinkTab<SimTabState>("SimTab", false);
            LinkTab<OptionsTabState>("OptionsTab");
            LinkTab<ErrorScreenState>("ErrorScreen");
            LinkTab<SelectionState>("SelectionPanel");
            LinkTab<DefaultSimulatorState>("DefaultSimulator");
            LinkTab<MixAndMatchState>("MixAndMatchMode");
            LinkTab<LoadRobotState>("SimLoadRobot");
            LinkTab<LoadFieldState>("SimLoadField");
        }

        private void LinkTab<T>(string tabName, bool strict = true) where T : State
        {
            GameObject tab = Auxiliary.FindGameObject(tabName);

            if (tab != null)
                StateMachine.SceneGlobal.Link<T>(tab, true, strict);
        }

        /// <summary>
        /// Finds all the UI game objects and assigns them to a variable
        /// </summary>
        private void FindAllGameObjects()
        {
            splashScreen = Auxiliary.FindObject(gameObject, "LoadSplash");
            Auxiliary.FindObject(gameObject, "QualitySettingsText").GetComponent<Text>().text = QualitySettings.names[QualitySettings.GetQualityLevel()];
        }

        /// <summary>
        /// Launches directly into the simulator if command line arguments are provided.
        /// </summary>
        /// <returns></returns>
        private bool DirectSimulatorLaunch()
        {
            if (!AppModel.InitialLaunch)
                return false;

            AppModel.InitialLaunch = false;

            //Loads robot and field directories from command line arguments if valid.
            string[] args = Environment.GetCommandLineArgs();
            bool robotDefined = false;
            bool fieldDefined = false;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "-robot":
                        if (i < args.Length - 1)
                        {
                            string robotFile = args[++i];

                            DirectoryInfo dirInfo = new DirectoryInfo(robotFile);
                            PlayerPrefs.SetString("RobotDirectory", dirInfo.Parent.FullName);
                            PlayerPrefs.SetString("simSelectedRobot", robotFile);
                            PlayerPrefs.SetString("simSelectedRobotName", dirInfo.Name);
                            robotDefined = true;
                        }
                        break;
                    case "-field":
                        if (i < args.Length - 1)
                        {
                            string fieldFile = args[++i];

                            DirectoryInfo dirInfo = new DirectoryInfo(fieldFile);
                            PlayerPrefs.SetString("FieldDirectory", dirInfo.Parent.FullName);
                            PlayerPrefs.SetString("simSelectedField", fieldFile);
                            PlayerPrefs.SetString("simSelectedFieldName", dirInfo.Name);
                            fieldDefined = true;
                        }
                        break;
                }
            }

            //If command line arguments have been passed, start the simulator.
            if (robotDefined && fieldDefined)
            {

                AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.MainSimulator,
                    AnalyticsLedger.TimingVarible.Starting,
                    AnalyticsLedger.TimingLabel.MainSimMenu);

                PlayerPrefs.SetString("simSelectedReplay", string.Empty);
                SceneManager.LoadScene("Scene");

                RobotTypeManager.SetProperties(false);
                return true;
            }

            return false;
        }

        public void GetUpdate(bool yes)
        {
            if (yes)
            {
                Process.Start("http://synthesis.autodesk.com");
                Process.Start(updater);
                Application.Quit();
            }
            else Auxiliary.FindObject("UpdatePrompt").SetActive(false);
        }
    }
}