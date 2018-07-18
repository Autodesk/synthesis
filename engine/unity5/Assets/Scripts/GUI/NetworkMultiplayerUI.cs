using Synthesis.FSM;
using Synthesis.States;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Synthesis.GUI
{
    public class NetworkMultiplayerUI : MonoBehaviour
    {
        private const float MessageScaleFactor = 10f;

        private StateMachine uiStateMachine;
        private Canvas canvas;

        /// <summary>
        /// Links the <see cref="NetworkMultiplayerUI"/>'s panels to the <see cref="StateMachine"/> and
        /// registers all button callbacks.
        /// </summary>
        private void Start()
        {
            uiStateMachine = GetComponent<StateMachine>();
            canvas = GetComponent<Canvas>();

            LinkPanels();
            RegisterButtonCallbacks();

            uiStateMachine.PushState(new HostJoinState());
        }

        /// <summary>
        /// Runs every frame to update the GUI elements.
        /// </summary>
        void OnGUI()
        {
            UserMessageManager.scale = canvas.scaleFactor * MessageScaleFactor;
            UserMessageManager.Render();
        }

        /// <summary>
        /// Pops the active UI <see cref="State"/>.
        /// </summary>
        public void OnBackButtonPressed()
        {
            uiStateMachine.PopState();

            if (uiStateMachine.CurrentState == null)
            {
                Auxiliary.FindGameObject("ExitingPanel").SetActive(true);
                SceneManager.LoadScene("MainMenu");
            }
        }

        /// <summary>
        /// Links individual panels with their respective <see cref="State"/>s.
        /// </summary>
        private void LinkPanels()
        {
            LinkPanel<HostJoinState>("HostJoinPanel");
            LinkPanel<EnterInfoState>("EnterInfoPanel");
            LinkPanel<ConnectingState>("ConnectingPanel");
            LinkPanel<LobbyState>("LobbyPanel");
        }

        /// <summary>
        /// Links a panel to the provided <see cref="State"/> type from the panel's name.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="panelName"></param>
        private void LinkPanel<T>(string panelName, bool strict = true) where T : State
        {
            GameObject tab = Auxiliary.FindGameObject(panelName);

            if (tab != null)
                uiStateMachine.Link<T>(tab, true, strict);
        }

        /// <summary>
        /// Finds each Button component in the main menu that doesn't already have a
        /// listener and registers it with a callback.
        /// </summary>
        private void RegisterButtonCallbacks()
        {
            foreach (Button b in GetComponentsInChildren<Button>(true))
                if (b.onClick.GetPersistentEventCount() == 0)
                    b.onClick.AddListener(() => InvokeCallback("On" + b.name + "Pressed"));
        }

        /// <summary>
        /// Invokes a method in the active <see cref="State"/> by the given method name.
        /// </summary>
        /// <param name="methodName"></param>
        private void InvokeCallback(string methodName)
        {
            State currentState = uiStateMachine.CurrentState;
            MethodInfo info = currentState.GetType().GetMethod(methodName);

            if (info == null)
                Debug.LogWarning("Method " + methodName + " does not have a listener in " + currentState.GetType().ToString());
            else
                info.Invoke(currentState, null);
        }
    }
}
