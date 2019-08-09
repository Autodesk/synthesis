//﻿using Synthesis.FSM;
//using Synthesis.Network;
//using Synthesis.States;
//using Synthesis.Utils;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;

//namespace Synthesis.GUI
//{
//    public class NetworkMultiplayerUI : MonoBehaviour
//    {
//        /// <summary>
//        /// The global <see cref="NetworkMultiplayerUI"/> instance.
//        /// </summary>
//        public static NetworkMultiplayerUI Instance { get; private set; }

//        /// <summary>
//        /// Sets or gets the visibility of the UI.
//        /// </summary>
//        public bool Visible
//        {
//            get
//            {
//                return canvas.enabled;
//            }
//            set
//            {
//                canvas.enabled = value;
//                tabCanvas.SetActive(!value);

//                if (value)
//                {
//                    FindObjectOfType<UnityEngine.Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("Default"));
//                    while (tabStateMachine.PopState() != null) ;
//                }
//                else
//                {
//                    FindObjectOfType<UnityEngine.Camera>().cullingMask |= (1 << LayerMask.NameToLayer("Default"));
//                    tabStateMachine.PushState(new MainToolbarState());
//                }
//            }
//        }

//        /// <summary>
//        /// The <see cref="StateMachine"/> controlling the user interface of the network multiplayer scene.
//        /// </summary>
//        public StateMachine UIStateMachine { get; private set; }

//        private Canvas canvas;
//        private GameObject tabCanvas;
//        private GameObject exitPanel;
//        private StateMachine tabStateMachine;

//        /// <summary>
//        /// Initializes the global instance reference.
//        /// </summary>
//        private void Awake()
//        {
//            Instance = this;
//        }

//        /// <summary>
//        /// Links the <see cref="NetworkMultiplayerUI"/>'s panels to the <see cref="StateMachine"/> and
//        /// registers all button callbacks.
//        /// </summary>
//        private void Start()
//        {
//            UIStateMachine = GetComponent<StateMachine>();
//            canvas = GetComponent<Canvas>();
//            tabCanvas = Auxiliary.FindGameObject("Canvas");
//            exitPanel = Auxiliary.FindGameObject("ExitPanel");
//            tabStateMachine = tabCanvas.GetComponent<StateMachine>();
//            UICallbackManager.RegisterButtonCallbacks(tabStateMachine, tabCanvas);
//            UICallbackManager.RegisterDropdownCallbacks(tabStateMachine, tabCanvas);

//            LinkPanels();
//            UICallbackManager.RegisterButtonCallbacks(UIStateMachine, gameObject);
//            UICallbackManager.RegisterToggleCallbacks(UIStateMachine, gameObject);

//            if (PlayerPrefs.GetInt("ShowDisclaimer", 1) == 1)
//                UIStateMachine.PushState(new DisclaimerState(true));
//            else
//                UIStateMachine.PushState(new HostJoinState());
//        }

//        /// <summary>
//        /// Runs every frame to update the GUI elements.
//        /// </summary>
//        void OnGUI()
//        {
//            UserMessageManager.scale = canvas.scaleFactor;
//            UserMessageManager.Render();
//        }

//        /// <summary>
//        /// Pops the active UI <see cref="State"/>.
//        /// </summary>
//        public void OnBackButtonClicked()
//        {
//            UIStateMachine.PopState();

//            if (UIStateMachine.CurrentState == null)
//            {
//                AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.Multiplater,
//                    AnalyticsLedger.TimingVarible.Customizing,
//                    AnalyticsLedger.TimingLabel.MultiplayerLobbyMenu);
//                AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.MultiplayerMenu,
//                    AnalyticsLedger.EventAction.BackedOut,
//                    "",
//                    AnalyticsLedger.getMilliseconds().ToString());

//                Auxiliary.FindGameObject("ExitingPanel").SetActive(true);
//                SceneManager.LoadScene("MainMenu");
//            }
//        }

//        /// <summary>
//        /// Opens the exit panel.
//        /// </summary>
//        public void OpenExitPanel()
//        {
//            exitPanel.SetActive(true);
//        }

//        /// <summary>
//        /// Closes the exit panel.
//        /// </summary>
//        public void CloseExitPanel()
//        {
//            exitPanel.SetActive(false);
//        }

//        /// <summary>
//        /// Loads the main menu scene.
//        /// </summary>
//        public void ReturnToMainMenu()
//        {
//            if (MultiplayerNetwork.Instance.Host)
//                MultiplayerNetwork.Instance.StopHost();
//            else
//                MultiplayerNetwork.Instance.StopClient();

//            SceneManager.LoadScene("MainMenu");
//        }

//        /// <summary>
//        /// Links individual panels with their respective <see cref="State"/>s.
//        /// </summary>
//        private void LinkPanels()
//        {
//            LinkPanel<DisclaimerState>("DisclaimerPanel");
//            LinkPanel<HostJoinState>("HostJoinPanel");
//            LinkPanel<EnterTagState>("EnterTagPanel");
//            LinkPanel<EnterInfoState>("EnterInfoPanel");
//            LinkPanel<LobbyState>("LobbyPanel");
//            LinkPanel<LoadFieldState>("SimLoadField");
//            LinkPanel<LoadRobotState>("SimLoadRobot");
//            LinkPanel<FetchingMetadataState>("FetchingMetadataPanel");
//            LinkPanel<AnalyzingResourcesState>("AnalyzingResourcesPanel");
//            LinkPanel<GatheringResourcesState>("GatheringResourcesPanel");
//            LinkPanel<DistributingResourcesState>("DistributingResourcesPanel");
//            LinkPanel<GeneratingSceneState>("GeneratingScenePanel");
//        }

//        /// <summary>
//        /// Links a panel to the provided <see cref="State"/> type from the panel's name.
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="panelName"></param>
//        private void LinkPanel<T>(string panelName) where T : State
//        {
//            GameObject panel = Auxiliary.FindGameObject(panelName);

//            if (panel != null)
//                UIStateMachine.Link<T>(panel);
//        }
//    }
//}