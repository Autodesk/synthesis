using UnityEngine;
using UnityEngine.UI;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Synthesis.GUI;
using Synthesis.Utils;

//=========================================================================================
//                                      CreateButton.cs
// Description: Generates buttons and button text for the each player in the control panel.
// Main Content:
//     UpdateActiveButtons(): Creates and updates the active player's buttons/controls.
//     UpdatePlayerOne() - UpdatePlayerSix(): Creates and updates the specified player's controls.
//     Toggles - Utilized by players to toggle their player preferances.
//     Functions - Various functions supporting these features.
// Adapted from: https://github.com/Gris87/InputControl
//=========================================================================================

namespace Synthesis.Input
{
    public class CreateButton : MonoBehaviour
    {
        //Toggle Switches
        public GameObject tankDriveSwitch;
        public GameObject unitConversionSwitch;

        public GameObject keyNamePrefab;
        public GameObject keyButtonsPrefab;

        private Transform namesTransform; //The string name of the control (the first column of the control panel; non-button)
        private Transform keysTransform; //The buttons of the controls (column 2 and column 3 of the control panel)

        public void Awake()
        {
            tankDriveSwitch = Auxiliary.FindObject("TankDriveSwitch");
            unitConversionSwitch = Auxiliary.FindObject("UnitConversionSwitch");

            namesTransform = transform.Find("Names");
            keysTransform = transform.Find("Keys");
        }

        // Use this for initialization
        public void Start()
        {
            DestroyList();
            //Can change the default measurement HERE and also change the default value in the slider game object in main menu
            PlayerPrefs.SetString("Measure", "Metric");

            //Loads controls (if changed in another scene) and updates their button text.
            GameObject.Find("Content").GetComponent<CreateButton>().UpdateButtons();

            GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateAllText();
        }

        //==============================================================================================
        //                                       Update Functions
        // The following functions are almost identical EXCEPT for the ReadOnlyCollection line.
        // Each function will retrieve control information for the specified player list and create control 
        // input buttons for that player. Each player is specified by a playerIndex and the specific player's
        // list can be called with this index 0 (player one) - index 5 (player six).
        //==============================================================================================

        public void UpdateButtons() // TODO rename to CreateButtons and limit number of calls? Replace with UpdateAllText?
        {
            DestroyList();
            float maxNameWidth = 0;
            float contentHeight = 4;

            //Retrieves and updates the active player's keys
            ReadOnlyCollection<KeyMapping> keys = Controls.Players[SettingsMode.activePlayerIndex].GetActiveList();

            foreach (KeyMapping key in keys)
            {
                //========================================================================================
                //                                   Key Text vs Key Buttons
                //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
                //Key Buttons: The buttons in the second and third column of the Input Manager menu
                //========================================================================================

                //Source: https://github.com/Gris87/InputControl
                #region Key text
                GameObject keyNameText = Instantiate(keyNamePrefab) as GameObject;
                keyNameText.name = key.name;

                RectTransform keyNameTextRectTransform = keyNameText.GetComponent<RectTransform>();

                keyNameTextRectTransform.transform.SetParent(namesTransform);
                keyNameTextRectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
                keyNameTextRectTransform.localScale = new Vector3(1, 1, 1);

                Text keyText = keyNameText.GetComponentInChildren<Text>();
                keyText.text = key.name;

                float keyNameWidth = keyText.preferredWidth + 8;

                if (keyNameWidth > maxNameWidth)
                {
                    maxNameWidth = keyNameWidth;
                }
                #endregion

                #region Key buttons
                GameObject keyButtons = Instantiate(keyButtonsPrefab) as GameObject;
                keyButtons.name = key.name;

                RectTransform keyButtonsRectTransform = keyButtons.GetComponent<RectTransform>();

                keyButtonsRectTransform.transform.SetParent(keysTransform);
                keyButtonsRectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
                keyButtonsRectTransform.localScale = new Vector3(1, 1, 1);

                for (int i = 0; i < keyButtons.transform.childCount; ++i) // 3 - primary, secondary, tertiary
                {
                    KeyButton buttonScript = keyButtons.transform.GetChild(i).GetComponent<KeyButton>();

                    buttonScript.keyMapping = key;
                    buttonScript.keyIndex = i;

                    buttonScript.UpdateText();
                }
                #endregion
                //=============================================

                contentHeight += 28;
            }

            RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
            RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
            RectTransform rectTransform = GetComponent<RectTransform>();

            namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
            keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
            rectTransform.sizeDelta = new Vector2(0, contentHeight);

            GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateButtonStyle();
        }

        /// <summary>
        /// Destroys control lists.
        /// Reccommended: Call before generating/creating a new player control list.
        /// </summary>
        public void DestroyList()
        {
            namesTransform = transform.Find("Names");
            keysTransform = transform.Find("Keys");

            //Loops through each string name for controls and destroys the object(s)
            foreach (Transform child in namesTransform)
            {
                Destroy(child.gameObject);
            }

            //Loops through each control input button and destroys the object(s)
            foreach (Transform child in keysTransform)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Resets controls to tank drive defaults for the active player and updates 
        /// corresponding control labels/buttons.
        /// </summary>
        public void ResetTankDrive()
        {
            Controls.Players[SettingsMode.activePlayerIndex].ResetTank();
            UpdateButtons();
        }

        /// <summary>
        /// Resets controls to arcade drive defaults for the active player and updates 
        /// corresponding control labels/buttons.
        /// </summary>
        public void ResetArcadeDrive()
        {
            Controls.Players[SettingsMode.activePlayerIndex].ResetArcade();
            UpdateButtons();
        }

        /// <summary>
        /// Toggles the tankDriveSwitch/slider between arcade/tank drive for each player.
        /// </summary>
        public void TankSlider()
        {
            int i = (int)tankDriveSwitch.GetComponent<Slider>().value;

            switch (i)
            {
                case 0:  //tank drive slider is OFF
                    Controls.Players[SettingsMode.activePlayerIndex].SetArcadeDrive();
                    Controls.TankDriveEnabled = false;
                    Controls.Load();
                    if (States.MainState.timesLoaded > 1)
                    {
                        Controls.UpdateFieldControls(false);
                    }
                    break;
                case 1:  //tank drive slider is ON
                    Controls.Players[SettingsMode.activePlayerIndex].SetTankDrive();
                    Controls.TankDriveEnabled = true;
                    if (States.MainState.timesLoaded > 1)
                    {
                        Controls.UpdateFieldControls(true);
                    }
                    Controls.Load();
                    break;
                default: //defaults to arcade drive
                    Controls.Players[SettingsMode.activePlayerIndex].SetArcadeDrive();
                    Controls.TankDriveEnabled = false;
                    break;
            }
            Controls.Load();
            UpdateButtons();
        }

        /// <summary>
        /// Sets the player preference for measurement units
        /// </summary>
        public void UnitConversionSlider()
        {
            int i = (int)unitConversionSwitch.GetComponent<Slider>().value;

            switch (i)
            {
                case 0:  //unit conversion slider is OFF
                    PlayerPrefs.SetString("Measure", "Imperial");
                    break;
                case 1:  //unit conversion slider is ON
                    PlayerPrefs.SetString("Measure", "Metric");
                    break;
            }
        }
        /// <summary>
        /// Updates the toggles/sliders when changing scenes.
        /// </summary>
        public void OnEnable()
        {
            //Tank drive slider
            tankDriveSwitch = Auxiliary.FindObject("TankDriveSwitch");
            tankDriveSwitch.GetComponent<Slider>().value = Controls.Players[SettingsMode.activePlayerIndex].isTankDrive ? 1 : 0;

            //Measurement slider
            unitConversionSwitch = Auxiliary.FindObject("UnitConversionSwitch");
            unitConversionSwitch.GetComponent<Slider>().value = PlayerPrefs.GetString("Measure").Equals("Metric") ? 1 : 0;
        }

        /// <summary>
        /// Updates the tank slider. Called on the active player to check for each player's individual preferances.
        /// </summary>
        public void UpdateTankSlider()
        {
            //tankDriveSwitch = AuxFunctions.FindObject("TankDriveSwitch");
            tankDriveSwitch.GetComponent<Slider>().value = Controls.Players[SettingsMode.activePlayerIndex].isTankDrive ? 1 : 0;
        }
    }
}