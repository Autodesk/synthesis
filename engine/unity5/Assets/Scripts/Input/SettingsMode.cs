using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.States;
using Synthesis.Utils;
using UnityEngine;
using UnityEngine.UI;

//=========================================================================================
//                                      SettingsMode.cs
// Description: The script that enables all the control panel functions.
// Main Content: Primarily functions attached to control panel buttons.
//=========================================================================================

namespace Synthesis.Input
{
    public class SettingsMode : MonoBehaviour
    {
        //Toggle Switches
        public Dropdown profileDropdown;
        public GameObject unitConversionSwitch;

        public Sprite DefaultButtonImage;
        public Sprite ActiveButtonImage;

        public State CurrentState { get; set; }

        // Variable to keep track which player controls are being edited in the control panel
        public static int activePlayerIndex;

        private Profile.Mode activeProfileMode;

        public void Awake()
        {
            profileDropdown = Auxiliary.FindObject("ProfileDropdown").GetComponentInChildren<Dropdown>();

            unitConversionSwitch = Auxiliary.FindObject("UnitConversionSwitch");
            //Can change the default measurement HERE and also change the default value in the slider game object in main menu
            PlayerPrefs.SetString("Measure", "Metric");

            activePlayerIndex = 0;
        }

        /// <summary>
        /// Updates the toggles/sliders when changing scenes.
        /// </summary>
        public void OnEnable()
        {
            profileDropdown = Auxiliary.FindObject("ProfileDropdown").GetComponentInChildren<Dropdown>();
            profileDropdown.value = (int)Controls.Players[activePlayerIndex].GetActiveProfileMode();

            //Measurement slider
            unitConversionSwitch = Auxiliary.FindObject("UnitConversionSwitch");
            unitConversionSwitch.GetComponent<Slider>().value = PlayerPrefs.GetString("Measure").Equals("Metric") ? 0 : 1;
        }

        /// <summary>
        /// Ignore mouse movement toggle.
        /// Current State: DISABLED 08/2017 - need to fix bugs for consistent toggle updates
        /// </summary>
        /// <param name="on"></param>
        public void OnIgnoreMouseMovementChanged(bool on)
        {
            KeyButton.ignoreMouseMovement = on;
        }

        /// <summary>
        /// Use key modifiers toggle.
        /// Current State: DISABLED 08/2017 - need to fix bugs for consistent toggle updates
        /// </summary>
        /// <param name="on"></param>
        public void OnUseKeyModifiersChanged(bool on)
        {
            KeyButton.useKeyModifiers = on;
        }

        /// <summary>
        /// Saves ALL player controls when clicked.
        /// </summary>
        public void OnSaveClick()
        {
            Controls.Save();
        }

        /// <summary>
        /// Resets ALL player controls to the profile defaults when clicked.
        /// </summary>
        public void OnReset()
        {
            Controls.Players[activePlayerIndex].ResetProfile(activeProfileMode);
            Controls.Save();
            GameObject.Find("Content").GetComponent<CreateButton>().CreateButtons();
        }

        /// <summary>
        /// Allows the player to switch their control profile.
        /// </summary>
        public void OnProfileSelect(int value)
        {
            if (activeProfileMode != (Profile.Mode)value)
            {
                GameObject.Find("Simulator").GetComponent<SimUI>().CheckUnsavedControls(() =>
                {
                    activeProfileMode = (Profile.Mode)value;

                    Controls.Players[activePlayerIndex].SetActiveProfileMode(activeProfileMode);
                    Controls.Players[activePlayerIndex].LoadActiveProfile();

                    GameObject.Find("Content").GetComponent<CreateButton>().CreateButtons();
                });
            }
        }

        //=========================================================================================
        //                                     Update Player Buttons
        // Creates and updates a specific player and their control's list. Checks for possible
        // toggle updates and if controls were saved.
        //=========================================================================================

        public void OnPlayerSelect(int index)
        {
            if (index != activePlayerIndex)
            {
                GameObject.Find("Simulator").GetComponent<SimUI>().CheckUnsavedControls(() =>
                {
                    activePlayerIndex = index;
                    UpdateProfileSelection();
                    UpdatePlayerButtonStyle();
                    GameObject.Find("Content").GetComponent<CreateButton>().CreateButtons();
                });
            }
        }

        /// <summary>
        /// Updates the active player button to the active button style. This makes the button
        /// appear highlighted (and stay highlighted) when the player clicks on a specific button.
        /// </summary>
        public void UpdatePlayerButtonStyle()
        {
            GameObject.Find("PlayerOne Button").GetComponent<Image>().sprite = (activePlayerIndex == 0) ? ActiveButtonImage : DefaultButtonImage;
            GameObject.Find("PlayerTwo Button").GetComponent<Image>().sprite = (activePlayerIndex == 1) ? ActiveButtonImage : DefaultButtonImage;
            GameObject.Find("PlayerThree Button").GetComponent<Image>().sprite = (activePlayerIndex == 2) ? ActiveButtonImage : DefaultButtonImage;
            GameObject.Find("PlayerFour Button").GetComponent<Image>().sprite = (activePlayerIndex == 3) ? ActiveButtonImage : DefaultButtonImage;
            GameObject.Find("PlayerFive Button").GetComponent<Image>().sprite = (activePlayerIndex == 4) ? ActiveButtonImage : DefaultButtonImage;
            GameObject.Find("PlayerSix Button").GetComponent<Image>().sprite = (activePlayerIndex == 5) ? ActiveButtonImage : DefaultButtonImage;
        }

        /// <summary>
        /// Updates the control profile selection. Called on the active player to check for each player's individual preferances.
        /// </summary>
        public void UpdateProfileSelection()
        {
            if (profileDropdown.value != (int)Controls.Players[activePlayerIndex].GetActiveProfileMode())
            {
                profileDropdown.value = (int)Controls.Players[activePlayerIndex].GetActiveProfileMode();
                profileDropdown.RefreshShownValue();
            }
        }
    }
}