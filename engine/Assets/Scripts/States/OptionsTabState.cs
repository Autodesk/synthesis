using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Input;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.States
{
    public class OptionsTabState : State
    {
        private GameObject graphics;
        private GameObject splashScreen;

        private static bool inputPanelOn = false;

        /// <summary>
        /// Establishes references to required <see cref="GameObject"/>s.
        /// </summary>
        public override void Start()
        {
            graphics = Auxiliary.FindGameObject("Graphics");
            splashScreen = Auxiliary.FindGameObject("LoadSplash");

            OnGraphicsButtonClicked();
        }

        public override void Update()
        {
            
        }

        /// <summary>
        /// Activates the input tab when the input button is pressed.
        /// </summary>
        public void OnInputButtonClicked()
        {
            graphics.SetActive(false);
            inputPanelOn = true;
        }

        public void OnOkButtonClicked()
        {
            graphics.SetActive(false);
            inputPanelOn = true;
        }

        /// <summary>
        /// Activates the graphics tab when the graphics button is pressed.
        /// </summary>
        public void OnGraphicsButtonClicked()
        {
            graphics.SetActive(true);
            //input.SetActive(false);
            //settingsMode.SetActive(true);
            inputPanelOn = false;
        }

        /// <summary>
        /// Changes the quality settings label when the quality settings button is pressed.
        /// </summary>
        public void OnQualitySettingsClicked()
        {
            QualitySettings.SetQualityLevel((QualitySettings.GetQualityLevel() + 1) % QualitySettings.names.Length);
            GameObject.Find("QualitySettingsText").GetComponent<Text>().text = QualitySettings.names[QualitySettings.GetQualityLevel()];
        }

        /// <summary>
        /// Applies the graphics settings.
        /// </summary>
        public void OnApplySettingsButtonClicked()
        {
            PopupButton resPopup = GameObject.Find("ResolutionButton").GetComponent<PopupButton>();
            int xRes;
            int yRes;

            ParseResolution(resPopup.list[PlayerPrefs.GetInt("resolution")].text, out xRes, out yRes);

            Screen.SetResolution(xRes, yRes, PlayerPrefs.GetInt("fullscreen") != 0);

            OnBackButtonClicked();
        }

        /// <summary>
        /// Exits the state of changing the graphics settings.
        /// </summary>
        public void OnBackButtonClicked()
        {
            StateMachine.ChangeState(new HomeTabState());
        }

        /// <summary>
        /// Parses the given string representation of a screen resolution and assigns
        /// values to the provided out parameters.
        /// </summary>
        /// <param name="resolution"></param>
        /// <param name="xRes"></param>
        /// <param name="yRes"></param>
        private void ParseResolution(string resolution, out int xRes, out int yRes)
        {
            string[] components = resolution.Split('x');
            xRes = int.Parse(components[0]);
            yRes = int.Parse(components[1]);
        }
    }
}