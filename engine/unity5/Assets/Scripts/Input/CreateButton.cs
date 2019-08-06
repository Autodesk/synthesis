using UnityEngine;
using UnityEngine.UI;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Synthesis.GUI;
using Synthesis.Utils;
using System;

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
        public GameObject keyNamePrefab;
        public GameObject keyButtonsPrefab;
        public GameObject simpleKeyButtonsPrefab;
        private GameObject controlLabelSwitch;

        private Transform namesTransform; //The string name of the control (the first column of the control panel; non-button)
        private Transform keysTransform; //The buttons of the controls (column 2 and column 3 of the control panel)

        private List<KeyButton> buttons = new List<KeyButton>();

        public void Awake()
        {
            namesTransform = transform.Find("Names");
            keysTransform = transform.Find("Keys");
            controlLabelSwitch = Auxiliary.FindObject("ControlLabelSwitch");
        }

        // Use this for initialization
        public void Start()
        {
            CreateButtons();

            GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdatePlayerButtonStyle();
        }

        public void SelectButtons(Inputs.CustomInput selection)
        {
            foreach (var button in buttons)
            {
                for (int i = 0; i < KeyMapping.NUM_INPUTS; i++)
                {
                    if (button.keyIndex == i && button.keyMapping.GetInput(i).Equals(selection))
                        button.Select();
                }
            }
        }

        private ReadOnlyCollection<KeyMapping> CreateKeyMappingList()
        {
            var merged = Controls.Players[SettingsMode.activePlayerIndex].GetActiveList();
            merged.AddRange(Controls.Global.GetList()); // TODO: move to separate global controls configuration tab of menu
            return merged.AsReadOnly();
        }

        public void AddButtonRow(string label, KeyMapping key, ref float contentHeight, ref float maxNameWidth)
        {
            AddButtonRow(label, key, ref contentHeight, ref maxNameWidth, null, null);
        }

        public void AddButtonRow(string label, KeyMapping key, ref float contentHeight, ref float maxNameWidth, Action<int, Inputs.CustomInput> inputHandler, Action<int, Text> textHandler)
        {
            //========================================================================================
            //                                   Key Text vs Key Buttons
            //Key Text: The labels/text in the first column of the InputManager menu (see Options tab)
            //Key Buttons: The buttons in the second and third column of the Input Manager menu
            //========================================================================================

            //Source: https://github.com/Gris87/InputControl

            #region Key text

            GameObject keyNameText = Instantiate(keyNamePrefab) as GameObject;
            keyNameText.name = label;

            RectTransform keyNameTextRectTransform = keyNameText.GetComponent<RectTransform>();

            keyNameTextRectTransform.transform.SetParent(namesTransform);
            keyNameTextRectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
            keyNameTextRectTransform.localScale = new Vector3(1, 1, 1);

            Text keyText = keyNameText.GetComponentInChildren<Text>();
            keyText.text = label;

            float keyNameWidth = keyText.preferredWidth + 8;

            if (keyNameWidth > maxNameWidth)
            {
                maxNameWidth = keyNameWidth;
            }

            #endregion

            #region Key buttons
            GameObject keyButtons = inputHandler != null ? (Instantiate(simpleKeyButtonsPrefab) as GameObject) : (Instantiate(keyButtonsPrefab) as GameObject);
            keyButtons.name = label;

            RectTransform keyButtonsRectTransform = keyButtons.GetComponent<RectTransform>();

            keyButtonsRectTransform.transform.SetParent(keysTransform);
            keyButtonsRectTransform.anchoredPosition3D = new Vector3(0, 0, 0);
            keyButtonsRectTransform.localScale = new Vector3(1, 1, 1);


            for (int i = 0; i < keyButtons.transform.childCount; ++i) // 3 - primary, secondary, tertiary
            {
                KeyButton buttonScript = keyButtons.transform.GetChild(i).GetComponent<KeyButton>();

                if (inputHandler != null)
                    buttonScript.Init(label, key, i, inputHandler, textHandler);
                else
                    buttonScript.Init(label, key, i);

                buttons.Add(buttonScript);
            }
            #endregion
            //=============================================

            contentHeight += 28;
        }

        //==============================================================================================
        //                                       Update Functions
        // The following functions are almost identical EXCEPT for the ReadOnlyCollection line.
        // Each function will retrieve control information for the specified player list and create control 
        // input buttons for that player. Each player is specified by a playerIndex and the specific player's
        // list can be called with this index 0 (player one) - index 5 (player six).
        //==============================================================================================

        public void CreateButtons() // TODO rename to CreateButtons and limit number of calls? Replace with UpdateAllText?
        {
            DestroyButtons();
            float maxNameWidth = 0;
            float contentHeight = 4;

            //Retrieves and updates the active player's keys
            ReadOnlyCollection<KeyMapping> keys = CreateKeyMappingList();

            bool use_drive_base_labels = (int)controlLabelSwitch.GetComponent<Slider>().value == 0;

            if (use_drive_base_labels)
            {
                // Custom button controls for handling tank, arcade, and mecanum drive bases more simply in the UI
                switch (SettingsMode.activeProfileMode)
                {
                    case Profile.Mode.Arcade:
                        AddButtonRow("Forward", Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM], ref contentHeight, ref maxNameWidth,
                            (int keyIndex, Inputs.CustomInput input) =>
                            {
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM].primaryInput = input;
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_RIGHT_PWM].primaryInput = input;
                            },
                            (int keyIndex, Text mKeyText) =>
                            {
                                mKeyText.text = Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM].primaryInput.ToString();
                            });
                        AddButtonRow("Backward", Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM], ref contentHeight, ref maxNameWidth,
                            (int keyIndex, Inputs.CustomInput input) =>
                            {
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_LEFT_PWM].primaryInput = input;
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_RIGHT_PWM].primaryInput = input;
                            },
                            (int keyIndex, Text mKeyText) =>
                            {
                                mKeyText.text = Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_LEFT_PWM].primaryInput.ToString();
                            });
                        AddButtonRow("Left", Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM], ref contentHeight, ref maxNameWidth,
                            (int keyIndex, Inputs.CustomInput input) =>
                            {
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_LEFT_PWM].secondaryInput = input;
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_RIGHT_PWM].secondaryInput = input;
                            },
                            (int keyIndex, Text mKeyText) =>
                            {
                                mKeyText.text = Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_LEFT_PWM].secondaryInput.ToString();
                            });
                        AddButtonRow("Right", Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM], ref contentHeight, ref maxNameWidth,
                            (int keyIndex, Inputs.CustomInput input) =>
                            {
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM].secondaryInput = input;
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_RIGHT_PWM].secondaryInput = input;
                            },
                            (int keyIndex, Text mKeyText) =>
                            {
                                mKeyText.text = Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM].secondaryInput.ToString();
                            });
                        break;
                    case Profile.Mode.Tank:
                        AddButtonRow("Left Forward", Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM], ref contentHeight, ref maxNameWidth,
                            (int keyIndex, Inputs.CustomInput input) =>
                            {
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM].primaryInput = input;
                            },
                            (int keyIndex, Text mKeyText) =>
                            {
                                mKeyText.text = Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM].primaryInput.ToString();
                            });
                        AddButtonRow("Left Backward", Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_LEFT_PWM], ref contentHeight, ref maxNameWidth,
                            (int keyIndex, Inputs.CustomInput input) =>
                            {
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_LEFT_PWM].primaryInput = input;
                            },
                            (int keyIndex, Text mKeyText) =>
                            {
                                mKeyText.text = Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_LEFT_PWM].primaryInput.ToString();
                            });
                        AddButtonRow("Right Forward", Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_RIGHT_PWM], ref contentHeight, ref maxNameWidth,
                            (int keyIndex, Inputs.CustomInput input) =>
                            {
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_RIGHT_PWM].primaryInput = input;
                            },
                            (int keyIndex, Text mKeyText) =>
                            {
                                mKeyText.text = Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_RIGHT_PWM].primaryInput.ToString();
                            });
                        AddButtonRow("Right Backward", Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_RIGHT_PWM], ref contentHeight, ref maxNameWidth,
                            (int keyIndex, Inputs.CustomInput input) =>
                            {
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_RIGHT_PWM].primaryInput = input;
                            },
                            (int keyIndex, Text mKeyText) =>
                            {
                                mKeyText.text = Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_RIGHT_PWM].primaryInput.ToString();
                            });
                        break;
                    case Profile.Mode.Mecanum:
                        AddButtonRow("Forward", Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM], ref contentHeight, ref maxNameWidth,
                            (int keyIndex, Inputs.CustomInput input) =>
                            {
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM].primaryInput = input;
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.BACK_LEFT_PWM].primaryInput = input;

                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_RIGHT_PWM].primaryInput = input;
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.BACK_RIGHT_PWM].primaryInput = input;
                            },
                            (int keyIndex, Text mKeyText) =>
                            {
                                mKeyText.text = Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM].primaryInput.ToString();
                            });
                        AddButtonRow("Backward", Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_LEFT_PWM], ref contentHeight, ref maxNameWidth,
                            (int keyIndex, Inputs.CustomInput input) =>
                            {
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_LEFT_PWM].primaryInput = input;
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.BACK_LEFT_PWM].primaryInput = input;

                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_RIGHT_PWM].primaryInput = input;
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.BACK_RIGHT_PWM].primaryInput = input;
                            },
                            (int keyIndex, Text mKeyText) =>
                            {
                                mKeyText.text = Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_LEFT_PWM].primaryInput.ToString();
                            });
                        AddButtonRow("Left", Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_LEFT_PWM], ref contentHeight, ref maxNameWidth,
                            (int keyIndex, Inputs.CustomInput input) =>
                            {
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_LEFT_PWM].secondaryInput = input;
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.BACK_LEFT_PWM].secondaryInput = input;

                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_RIGHT_PWM].secondaryInput = input;
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.BACK_RIGHT_PWM].secondaryInput = input;
                            },
                            (int keyIndex, Text mKeyText) =>
                            {
                                mKeyText.text = Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_LEFT_PWM].secondaryInput.ToString();
                            });
                        AddButtonRow("Right", Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM], ref contentHeight, ref maxNameWidth,
                            (int keyIndex, Inputs.CustomInput input) =>
                            {
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM].secondaryInput = input;
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.BACK_LEFT_PWM].secondaryInput = input;

                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_RIGHT_PWM].secondaryInput = input;
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.BACK_RIGHT_PWM].secondaryInput = input;
                            },
                            (int keyIndex, Text mKeyText) =>
                            {
                                mKeyText.text = Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM].secondaryInput.ToString();
                            });
                        AddButtonRow("Strafe Left", Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM], ref contentHeight, ref maxNameWidth,
                            (int keyIndex, Inputs.CustomInput input) =>
                            {
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_LEFT_PWM].tertiaryInput = input;
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.BACK_LEFT_PWM].tertiaryInput = input;

                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_RIGHT_PWM].tertiaryInput = input;
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.BACK_RIGHT_PWM].tertiaryInput = input;
                            },
                            (int keyIndex, Text mKeyText) =>
                            {
                                mKeyText.text = Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_LEFT_PWM].tertiaryInput.ToString();
                            });
                        AddButtonRow("Strafe Right", Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM], ref contentHeight, ref maxNameWidth,
                            (int keyIndex, Inputs.CustomInput input) =>
                            {
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM].tertiaryInput = input;
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.BACK_LEFT_PWM].tertiaryInput = input;

                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_RIGHT_PWM].tertiaryInput = input;
                                Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.BACK_RIGHT_PWM].tertiaryInput = input;
                            },
                            (int keyIndex, Text mKeyText) =>
                            {
                                mKeyText.text = Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM].tertiaryInput.ToString();
                            });
                        break;
                    default:
                        throw new Profile.UnhandledControlProfileException();
                }
            }
            foreach (KeyMapping key in keys)
            {
                bool default_front = ReferenceEquals(key, Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_LEFT_PWM]) ||
                    ReferenceEquals(key, Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_LEFT_PWM]) ||
                    ReferenceEquals(key, Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.FRONT_RIGHT_PWM]) ||
                    ReferenceEquals(key, Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.FRONT_RIGHT_PWM]);
                bool default_back = ReferenceEquals(key, Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.BACK_LEFT_PWM]) ||
                    ReferenceEquals(key, Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.BACK_LEFT_PWM]) ||
                    ReferenceEquals(key, Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmPos[Profile.BACK_RIGHT_PWM]) ||
                    ReferenceEquals(key, Controls.Players[SettingsMode.activePlayerIndex].GetButtons().pwmNeg[Profile.BACK_RIGHT_PWM]);
                bool covered_in_simple = default_front || (SettingsMode.activeProfileMode == Profile.Mode.Mecanum && default_back);
                if (!use_drive_base_labels || !covered_in_simple)
                {
                    AddButtonRow(key.name, key, ref contentHeight, ref maxNameWidth);
                }
            }

            RectTransform namesRectTransform = namesTransform.GetComponent<RectTransform>();
            RectTransform keysRectTransform = keysTransform.GetComponent<RectTransform>();
            RectTransform rectTransform = GetComponent<RectTransform>();

            namesRectTransform.offsetMax = new Vector2(maxNameWidth, 0);
            keysRectTransform.offsetMin = new Vector2(maxNameWidth, 0);
            rectTransform.sizeDelta = new Vector2(0, contentHeight);
        }

        /// <summary>
        /// Destroys control lists.
        /// Reccommended: Call before generating/creating a new player control list.
        /// </summary>
        private void DestroyButtons()
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

            buttons.Clear();
        }
    }
}