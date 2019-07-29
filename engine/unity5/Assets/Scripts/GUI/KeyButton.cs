using Synthesis.Input;
using Synthesis.Input.Enums;
using Synthesis.Input.Inputs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//=========================================================================================
//                                      KeyButton.cs
// Description: OnGUI() script with functions for CreateButton.cs
// Main Content: Various functions to assist in generating control buttons and player lists.
// Adapted from: https://github.com/Gris87/InputControl
//=========================================================================================

namespace Synthesis.GUI
{
    public class KeyButton : MonoBehaviour, IPointerClickHandler
    {
        private static List<KeyButton> selectedButtons = new List<KeyButton>();
        public static bool ignoreMouseMovement = true;
        public static bool useKeyModifiers = true;

        private static Stopwatch delayTimer = new Stopwatch();
        private const long DELAY_TIME = 100; // ms

        public KeyMapping keyMapping;
        public int keyIndex;
        private Text mKeyText;

        private Action<int, CustomInput> updateInputHandler;
        private Action<int, Text> updateTextHandler;

        private static Color DEFAULT_COLOR = Color.white;
        private static Color SELECTED_COLOR = new Color(247 / 255f, 162 / 255f, 24 / 255f, 1f);

        public void Awake()
        {
            mKeyText = GetComponentInChildren<Text>();
            delayTimer.Restart();
        }

        // Use this for initialization
        public void Start()
        {
            //Implement style preferances; (some assets/styles are configured in Unity: OptionsTab > Canvas > SettingsMode > SettingsPanel
            mKeyText.font = Resources.Load("Fonts/Russo_One") as Font;
            mKeyText.color = DEFAULT_COLOR;
            mKeyText.fontSize = 13;
        }

        public void Init(string label, KeyMapping key, int index)
        {
            Init(label, key, index,
                (int dummy, CustomInput input) =>
                {
                    keyMapping.GetInput(keyIndex) = input;
                },
                (int dummy, Text dummy2) =>
                {
                    mKeyText.text = keyMapping.GetInput(keyIndex).ToString();
                });
        }

        public void Init(string label, KeyMapping key, int index, Action<int, CustomInput> inputHandler, Action<int, Text> textHandler)
        {
            name = label;
            keyMapping = key;
            keyIndex = index;

            updateInputHandler = inputHandler;
            updateTextHandler = textHandler;

            updateTextHandler(keyIndex, mKeyText);
        }

        private static CustomInput CreateDefaultMapping()
        {
            return new KeyboardInput();
        }

        // Update is called once per frame
        public void OnGUI()
        {
            if (selectedButtons.Contains(this))
            {
                CustomInput CurrentInput = InputControl.CurrentInput(ignoreMouseMovement, useKeyModifiers);

                if (CurrentInput != null)
                {
                    if (CurrentInput.modifiers == KeyModifier.NoModifier && CurrentInput is KeyboardInput
                        && ((KeyboardInput)CurrentInput).key == KeyCode.Backspace) //Allows users to use the BACKSPACE to set "None" to their controls.
                    {
                        SetInput(CreateDefaultMapping());
                    }
                    else
                    {
                        SetInput(CurrentInput);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the primary and secondary control buttons' text label.
        /// Source: https://github.com/Gris87/InputControl
        /// </summary>
        public void UpdateText()
        {
            mKeyText.text = keyMapping.GetInput(keyIndex).ToString();
        }

        public void Select()
        {
            if (delayTimer.ElapsedMilliseconds > DELAY_TIME && !selectedButtons.Contains(this)) // Delay to allow users to assign mouse clicks more easily without re-selecting
            {
                selectedButtons.Add(this);
                mKeyText.text = "Press Key";
                mKeyText.color = SELECTED_COLOR;
                GetComponent<Button>().interactable = false;
            }
        }

        public void Deselect()
        {
            if (selectedButtons.Remove(this))
            {
                GetComponent<Button>().interactable = true;
                mKeyText.color = DEFAULT_COLOR;
                updateTextHandler(keyIndex, mKeyText);
                delayTimer.Restart();
            }
        }

        /// <summary>
        /// Updates the text when the user clicks the control buttons. 
        /// Source: https://github.com/Gris87/InputControl
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left: // Select this one only
                    Select();
                    break;
                case PointerEventData.InputButton.Middle:
                    break;
                case PointerEventData.InputButton.Right: // Select all with the same current key mapping. Allows batch reassignment.
                    if(!keyMapping.GetInput(keyIndex).Equals(CreateDefaultMapping()))
                        GameObject.Find("Content").GetComponent<CreateButton>().SelectButtons(keyMapping.GetInput(keyIndex));
                    else // Instead of batch selecting mapping None, only select this mapping
                        Select();
                    break;
                default:
                    throw new Exception();
            }
        }

        /// <summary>
        /// Sets the primary or secondary input to the selected input from the user.
        /// Source: https://github.com/Gris87/InputControl
        /// </summary>
        /// <param name="input">Input from any device or axis (e.g. Joysticks, Mouse, Keyboard)</param>
        private void SetInput(CustomInput input)
        {
            updateInputHandler(keyIndex, input);

            Deselect();
        }
    }
}