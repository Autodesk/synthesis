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

        Player.ControlProfile activeControlProfile;

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
            profileDropdown.value = (int)Controls.Players[SettingsMode.activePlayerIndex].controlProfile;

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
            UserMessageManager.Dispatch("Player preferences saved.", 5);
        }

        public void OnSaveClickMainMenu()
        {
            Controls.Save();
            UserMessageManager.Dispatch("Player preferences saved.", 5);
            StateMachine.SceneGlobal.ChangeState(new HomeTabState());
        }

        /// <summary>
        /// Loads ALL player controls when clicked.
        /// </summary>
        public void OnLoadClick()
        {
            Controls.Load();
        }

        /// <summary>
        /// Resets ALL player controls to the profile defaults when clicked.
        /// </summary>
        public void OnReset()
        {
            switch (Controls.Players[activePlayerIndex].controlProfile)
            {
                case Player.ControlProfile.TankKeyboard:
                    GameObject.Find("Content").GetComponent<CreateButton>().ResetTankDrive();
                    break;
                case Player.ControlProfile.ArcadeKeyboard:
                    GameObject.Find("Content").GetComponent<CreateButton>().ResetArcadeDrive();
                    break;
                default:
                    throw new System.Exception("Unsupported control profile");
            }
            Controls.Save();
        }

        /// <summary>
        /// Allows the player to switch their control profile.
        /// </summary>
        public void OnProfileSelect(int value)
        {
            if (activeControlProfile != (Player.ControlProfile)value)
            {
                activeControlProfile = (Player.ControlProfile)value;
                switch (activeControlProfile)
                {
                    case Player.ControlProfile.ArcadeKeyboard:
                        Controls.Players[SettingsMode.activePlayerIndex].SetArcadeDrive();
                        if (MainState.timesLoaded > 1)
                        {
                            Controls.UpdateFieldControls(activeControlProfile);
                        }
                        break;
                    case Player.ControlProfile.TankKeyboard:
                        Controls.Players[SettingsMode.activePlayerIndex].SetTankDrive();
                        if (MainState.timesLoaded > 1)
                        {
                            Controls.UpdateFieldControls(activeControlProfile);
                        }
                        break;
                    default: //defaults to arcade drive
                        Controls.Players[SettingsMode.activePlayerIndex].SetArcadeDrive();
                        break;
                }
                Controls.Load();

                GameObject.Find("Content").GetComponent<CreateButton>().CreateButtons();
            }
        }

        //=========================================================================================
        //                                     Update Player Buttons
        // Creates and updates a specific player and their control's list. Checks for possible
        // toggle updates and if controls were saved.
        //=========================================================================================

        public void OnPlayerSelect(int index)
        {
            activePlayerIndex = index;
            //Creates and generates player one's keys and control buttons
            GameObject.Find("Content").GetComponent<CreateButton>().CreateButtons();

            //Checks if the profile selection needs to be updated (according to the player)
            UpdateProfileSelection();

            //If the user did not press the save button, revert back to the last loaded and saved controls (no auto-save.)
            GetLastSavedControls();
            UpdatePlayerButtonStyle();
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
            if (profileDropdown.value != (int)Controls.Players[SettingsMode.activePlayerIndex].controlProfile)
            {
                profileDropdown.value = (int)Controls.Players[SettingsMode.activePlayerIndex].controlProfile;
                profileDropdown.RefreshShownValue();
            }
        }

        /// <summary>
        /// Gets the last loaded controls if the player did not press the "Save" button.
        /// Helps prevent auto-saving (in case a user accidentally changes their controls.)
        /// </summary>
        public void GetLastSavedControls()
        {
            Controls.Load();
            GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateButtons();
        }

        /// <summary>
        /// Updates all the key/control buttons.
        /// </summary>
        public void UpdateButtons()
        {
            KeyButton[] keyButtons = GetComponentsInChildren<KeyButton>();

            foreach (KeyButton keyButton in keyButtons)
            {
                keyButton.UpdateText();
            }
        }
    }
}