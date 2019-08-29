using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BulletUnity;
using Synthesis.FSM;
using System.IO;
using Synthesis.GUI;
using Synthesis.States;
using Synthesis.Utils;
using Synthesis.Input;

namespace Synthesis.MixAndMatch
{
    public class MaMSimUI : LinkedMonoBehaviour<MainState>
    {
        GameObject canvas;

        GameObject mixAndMatchPanel;
        GameObject multiplayerPanel;

        private SimUI simUI;

        private void Start()
        {
            FindElements();
        }

        private void OnGUI()
        {
            UserMessageManager.Render();
        }

        /// <summary>
        /// Finds all the necessary UI elements that need to be updated/modified
        /// </summary>
        private void FindElements()
        {
            canvas = GameObject.Find("Canvas");

            mixAndMatchPanel = Auxiliary.FindObject(canvas, "MixAndMatchPanel");
            multiplayerPanel = Auxiliary.FindObject(canvas, "MultiplayerPanel");

            simUI = StateMachine.SceneGlobal.gameObject.GetComponent<SimUI>();
        }

        public void ToggleMaMPanel()
        {
            if (mixAndMatchPanel.activeSelf)
            {
                mixAndMatchPanel.SetActive(false);
                InputControl.EnableSimControls();
            }
            else
            {
                simUI.EndOtherProcesses();
                mixAndMatchPanel.SetActive(true);
                InputControl.DisableSimControls();
            }
        }

        public void ToggleMaMInMultiplayer()
        {
            if (mixAndMatchPanel.activeSelf)
            {
                mixAndMatchPanel.SetActive(false);
                multiplayerPanel.SetActive(true);
            }
            else
            {
                simUI.EndOtherProcesses();
                mixAndMatchPanel.SetActive(true);
                multiplayerPanel.SetActive(true);
            }
        }
    }
}
