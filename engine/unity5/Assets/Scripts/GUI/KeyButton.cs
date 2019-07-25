using Synthesis.Input;
using Synthesis.Input.Enums;
using Synthesis.Input.Inputs;
using UnityEngine;
using UnityEngine.UI;

//=========================================================================================
//                                      KeyButton.cs
// Description: OnGUI() script with functions for CreateButton.cs
// Main Content: Various functions to assist in generating control buttons and player lists.
// Adapted from: https://github.com/Gris87/InputControl
//=========================================================================================

namespace Synthesis.GUI
{
    public class KeyButton : MonoBehaviour
    {
        public static KeyButton selectedButton = null;
        public static bool ignoreMouseMovement = true;
        public static bool useKeyModifiers = true;

        public KeyMapping keyMapping;
        public int keyIndex;

        private Text mKeyText;

        public void Awake()
        {
            mKeyText = GetComponentInChildren<Text>();
        }

        // Use this for initialization
        public void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        // Update is called once per frame
        public void OnGUI()
        {
            //Implement style preferances; (some assets/styles are configured in Unity: OptionsTab > Canvas > SettingsMode > SettingsPanel
            mKeyText.font = Resources.Load("Fonts/Russo_One") as Font;
            mKeyText.color = Color.white;
            mKeyText.fontSize = 13;

            //Checks if the CurrentInput uses the ignoreMouseMovement or useKeyModifiers
            //Currently DISABLED (hidden in the Unity menu) due to inconsistent toggle to key updates 08/2017
            if (selectedButton == this)
            {
                CustomInput CurrentInput = Input.InputControl.CurrentInput(ignoreMouseMovement, useKeyModifiers);

                if (CurrentInput != null)
                {
                    if (CurrentInput.modifiers == KeyModifier.NoModifier && CurrentInput is KeyboardInput
                        && ((KeyboardInput)CurrentInput).key == KeyCode.Backspace) //Allows users to use the BACKSPACE to set "None" to their controls.
                    {
                        SetInput(new KeyboardInput());
                    }
                    else if (CurrentInput.modifiers == KeyModifier.NoModifier && CurrentInput is KeyboardInput
                        && ((KeyboardInput)CurrentInput).key == KeyCode.Backspace)
                    {
                        SetInput(new KeyboardInput());
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
            switch (keyIndex)
            {
                case 0:
                    mKeyText.text = keyMapping.primaryInput.ToString();
                    break;
                case 1:
                    mKeyText.text = keyMapping.secondaryInput.ToString();
                    break;
                case 2:
                    mKeyText.text = keyMapping.tertiaryInput.ToString();
                    break;
                default:
                    throw new System.Exception();
            }
        }
        //}

        /// <summary>
        /// Updates the text when the user clicks the control buttons. 
        /// Source: https://github.com/Gris87/InputControl
        /// </summary>
        public void OnClick()
        {
            selectedButton = this;

            mKeyText.text = "Press Key";
        }

        /// <summary>
        /// Sets the primary or secondary input to the selected input from the user.
        /// Source: https://github.com/Gris87/InputControl
        /// </summary>
        /// <param name="input">Input from any device or axis (e.g. Joysticks, Mouse, Keyboard)</param>
        private void SetInput(CustomInput input)
        {
            switch (keyIndex)
            {
                case 0:
                    keyMapping.primaryInput = input;
                    break;
                case 1:
                    keyMapping.secondaryInput = input;
                    break;
                case 2:
                    keyMapping.tertiaryInput = input;
                    break;
                default:
                    throw new System.Exception();
            }

            UpdateText();

            selectedButton = null;
        }
    }
}