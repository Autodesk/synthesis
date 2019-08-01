using Newtonsoft.Json;
using Synthesis.Input.Enums;
using Synthesis.Input.Inputs;

namespace Synthesis.Input
{
    /// <summary>
    /// <see cref="KeyMapping"/> is a named handler for 3 <see cref="CustomInput"/>.
    /// Source: https://github.com/Gris87/InputControl
    /// </summary>
    public class KeyMapping
    {
        [JsonProperty]
        private string mName;

        [JsonProperty]
        private CustomInput mPrimaryInput;

        [JsonProperty]
        private CustomInput mSecondaryInput;

        [JsonProperty]
        private CustomInput mTertiaryInput;

        public const int NUM_INPUTS = 3;


        #region Properties

        #region name
        /// <summary>
        /// Gets the <see cref="KeyMapping"/> name.
        /// </summary>
        /// <value>KeyMapping name.</value>
        [JsonIgnore]
        public string name
        {
            get
            {
                return mName;
            }
        }
        #endregion

        public ref CustomInput GetInput(int i)
        {
            switch (i)
            {
                case 0:
                    return ref mPrimaryInput;
                case 1:
                    return ref mSecondaryInput;
                case 2:
                    return ref mTertiaryInput;
                default:
                    throw new System.Exception();
            }
        }

        #region primaryInput
        /// <summary>
        /// Gets or sets the primary input. Please note that if you set null value it will create KeyboardInput with KeyCode.None
        /// </summary>
        /// <value>Primary input.</value>
        [JsonIgnore]
        public CustomInput primaryInput
        {
            get
            {
                return mPrimaryInput;
            }

            set
            {
                if (value == null)
                {
                    mPrimaryInput = new KeyboardInput();
                }
                else
                {
                    mPrimaryInput = value;
                }
            }
        }
        #endregion

        #region secondaryInput
        /// <summary>
        /// Gets or sets the secondary input. Please note that if you set null value it will create KeyboardInput with KeyCode.None
        /// </summary>
        /// <value>Secondary input.</value>
        [JsonIgnore]
        public CustomInput secondaryInput
        {
            get
            {
                return mSecondaryInput;
            }

            set
            {
                if (value == null)
                {
                    mSecondaryInput = new KeyboardInput();
                }
                else
                {
                    mSecondaryInput = value;
                }
            }
        }
        #endregion

        #region thirdInput
        /// <summary>
        /// Gets or sets the third input. Please note that if you set null value it will create KeyboardInput with KeyCode.None
        /// </summary>
        /// <value>Third input.</value>
        [JsonIgnore]
        public CustomInput tertiaryInput
        {
            get
            {
                return mTertiaryInput;
            }

            set
            {
                if (value == null)
                {
                    mTertiaryInput = new KeyboardInput();
                }
                else
                {
                    mTertiaryInput = value;
                }
            }
        }
        #endregion

        #endregion

        #region argToInput Helper Functions for setKey() and SetAxis()

        /// <summary>
        /// Convert argument to <see cref="CustomInput"/>.
        /// </summary>
        /// <returns>Converted CustomInput.</returns>
        /// <param name="arg">Some kind of argument.</param>
        private static CustomInput ArgToInput(UnityEngine.KeyCode? arg, KeyModifier keyModifier = KeyModifier.NoModifier)
        {
            if (arg == null)
                return null;
            return new KeyboardInput(arg.Value, keyModifier);
        }
        #endregion

        public KeyMapping(string name, UnityEngine.KeyCode primaryInput, KeyModifier primaryKeyModifier = KeyModifier.NoModifier, 
            UnityEngine.KeyCode? secondaryInput = null, KeyModifier secondaryKeyModifier = KeyModifier.NoModifier, 
            UnityEngine.KeyCode? tertiaryInput = null, KeyModifier tertiaryKeyModifier = KeyModifier.NoModifier) :
            this(name, ArgToInput(primaryInput, primaryKeyModifier), ArgToInput(secondaryInput, secondaryKeyModifier), ArgToInput(tertiaryInput, tertiaryKeyModifier))
        { }

        /// <summary>
        /// Create a new instance of <see cref="KeyMapping"/> with 3 specified <see cref="CustomInput"/>.
        /// </summary>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primaryCustomInput">Primary input.</param>
        /// <param name="secondaryCustomInput">Secondary input.</param>
        /// <param name="thirdCustomInput">Third input.</param>
        [JsonConstructor]
        public KeyMapping(string name, CustomInput primaryCustomInput = null, CustomInput secondaryCustomInput = null, CustomInput thirdCustomInput = null)
        {
            set(name, primaryCustomInput, secondaryCustomInput, thirdCustomInput);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyMapping"/> class based on another instance.
        /// </summary>
        /// <param name="another">Another KeyMapping instance.</param>
        public KeyMapping(KeyMapping another)
        {
            set(another);
        }

        /// <summary>
        /// Set the same <see cref="CustomInput"/> as in another instance.
        /// </summary>
        /// <param name="another">Another KeyMapping instance.</param>
        public void set(KeyMapping another)
        {
            set(another.mName, another.mPrimaryInput, another.mSecondaryInput, another.mTertiaryInput);
        }

        public void set(string name, CustomInput primaryCustomInput = null, CustomInput secondaryCustomInput = null, CustomInput thirdCustomInput = null)
        {
            mName = name;
            set(primaryCustomInput, secondaryCustomInput, thirdCustomInput);
        }

        public void set(UnityEngine.KeyCode primaryInput, UnityEngine.KeyCode? secondaryInput = null, UnityEngine.KeyCode? tertiaryInput = null)
        {
            set(ArgToInput(primaryInput), ArgToInput(secondaryInput), ArgToInput(tertiaryInput));
        }

        public void set(CustomInput primaryCustomInput = null, CustomInput secondaryCustomInput = null, CustomInput thirdCustomInput = null)
        {
            primaryInput = primaryCustomInput;
            secondaryInput = secondaryCustomInput;
            tertiaryInput = thirdCustomInput;
        }

        private float combineValues(float a, float b, float c)
        {
            // Combine values, but make sure it's no higher than the highest or lower than the lowest parameter.

            var value = a + b + c;
            var upperBound = System.Math.Max(System.Math.Max(a, b), c);
            var lowerBound = System.Math.Min(System.Math.Min(a, b), c);

            return (value < lowerBound) ? lowerBound : ((value > upperBound) ? upperBound : value); // Clamp value
        }

        /// <summary>
        /// Returns input value while the user holds down the key.
        /// </summary>
        /// <returns>Input value if button or axis is still active.</returns>
        /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
        /// <param name="axis">Specific actions for axis (Empty by default).</param>
        /// <param name="device">Preferred input device.</param>
        public float getValue(bool exactKeyModifiers = false, string axis = "", InputDevice device = InputDevice.Any)
        {
            return combineValues(
                mPrimaryInput.getInput(exactKeyModifiers, axis, device),
                mSecondaryInput.getInput(exactKeyModifiers, axis, device),
                mTertiaryInput.getInput(exactKeyModifiers, axis, device));
        }

        /// <summary>
        /// Returns input value during the frame the user starts pressing down the key.
        /// </summary>
        /// <returns>Input value if button or axis become active during this frame.</returns>
        /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
        /// <param name="axis">Specific actions for axis (Empty by default).</param>
        /// <param name="device">Preferred input device.</param>
        public float getValueDown(bool exactKeyModifiers = false, string axis = "", InputDevice device = InputDevice.Any)
        {
            return combineValues(
               mPrimaryInput.getInputDown(exactKeyModifiers, axis, device),
               mSecondaryInput.getInputDown(exactKeyModifiers, axis, device),
               mTertiaryInput.getInputDown(exactKeyModifiers, axis, device));
        }

        /// <summary>
        /// Returns input value during the frame the user releases the key.
        /// </summary>
        /// <returns>Input value if button or axis stopped being active during this frame.</returns>
        /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
        /// <param name="axis">Specific actions for axis (Empty by default).</param>
        /// <param name="device">Preferred input device.</param>
        public float getValueUp(bool exactKeyModifiers = false, string axis = "", InputDevice device = InputDevice.Any)
        {
            return combineValues(
               mPrimaryInput.getInputUp(exactKeyModifiers, axis, device),
               mSecondaryInput.getInputUp(exactKeyModifiers, axis, device),
               mTertiaryInput.getInputUp(exactKeyModifiers, axis, device));
        }

        /// <summary>
        /// Returns true while the user holds down the key.
        /// </summary>
        /// <returns>True if button or axis is still active.</returns>
        /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
        /// <param name="device">Preferred input device.</param>
        public bool isPressed(bool exactKeyModifiers = false, InputDevice device = InputDevice.Any)
        {
            return getValue(exactKeyModifiers, "", device) != 0;
        }

        /// <summary>
        /// Returns true during the frame the user starts pressing down the key.
        /// </summary>
        /// <returns>True if button or axis become active during this frame.</returns>
        /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
        /// <param name="device">Preferred input device.</param>
        public bool isPressedDown(bool exactKeyModifiers = false, InputDevice device = InputDevice.Any)
        {
            return getValueDown(exactKeyModifiers, "", device) != 0;
        }

        /// <summary>
        /// Returns true during the frame the user releases the key.
        /// </summary>
        /// <returns>True if button or axis stopped being active during this frame.</returns>
        /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
        /// <param name="device">Preferred input device.</param>
        public bool isPressedUp(bool exactKeyModifiers = false, InputDevice device = InputDevice.Any)
        {
            return getValueUp(exactKeyModifiers, "", device) != 0;
        }
    }
}
