using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Synthesis.Input.Inputs;
using Synthesis.Input.Enums;

//=========================================================================================
//                                      InputControl.cs
/// Description: <see cref = "InputControl" /> provides interface to the Input system. It's 
/// based on <see cref="Input"/> class and allows the ability to change key mappings in runtime.
/// Adapted from: https://github.com/Gris87/InputControl
//=========================================================================================

namespace Synthesis.Input
{
    public static class InputControl
    {
        public static bool freeze = false;

        public const float NO_SMOOTH = 1000f;

        // Set of main keys
        private static List<KeyMapping> mKeysList = new List<KeyMapping>();
        private static Dictionary<string, KeyMapping> mKeysMap = new Dictionary<string, KeyMapping>();

        // Set of main axes
        private static List<Axis> mAxesList = new List<Axis>();
        private static Dictionary<string, Axis> mAxesMap = new Dictionary<string, Axis>();

        // Smooth for GetAxis
        private static Dictionary<string, float> mSmoothAxesValues = new Dictionary<string, float>();
        private static float mSmoothCoefficient = NO_SMOOTH; // Smooth looks the same as in Input.GetAxis() with this value

        // Joystick options
        private static float mJoystickThreshold = 0.2f;

        // Mouse options
        private static float mMouseSensitivity = 1f;
        private static bool mInvertMouseY = false;

        // Common options
        private static InputDevice mInputDevice = InputDevice.Any;

        #region Properties

        #region smoothCoefficient
        /// <summary>
        /// Gets or sets the axis smooth coefficient. Smooth coefficient is used in GetAxis method to make the movement a little smoothed as well as in Input.GetAxis
        /// </summary>
        /// <value>Axis smooth coefficient.</value>
        public static float SmoothCoefficient
        {
            get
            {
                return mSmoothCoefficient;
            }

            set
            {
                if (value < 0.0001f)
                {
                    mSmoothCoefficient = 0.0001f;
                }
                else
                if (value > NO_SMOOTH)
                {
                    mSmoothCoefficient = NO_SMOOTH;
                }
                else
                {
                    mSmoothCoefficient = value;
                }
            }
        }
        #endregion

        #region joystickThreshold
        /// <summary>
        /// Gets or sets the joystick threshold.
        /// </summary>
        /// <value>Joystick threshold.</value>
        public static float JoystickThreshold
        {
            get
            {
                return mJoystickThreshold;
            }

            set
            {
                if (value < 0f)
                {
                    mJoystickThreshold = 0f;
                }
                else
                if (value > 1f)
                {
                    mJoystickThreshold = 1f;
                }
                else
                {
                    mJoystickThreshold = value;
                }
            }
        }
        #endregion

        #region mouseSensitivity
        /// <summary>
        /// Gets or sets the mouse sensitivity.
        /// </summary>
        /// <value>Mouse sensitivity.</value>
        public static float MouseSensitivity
        {
            get
            {
                return mMouseSensitivity;
            }

            set
            {
                if (value < 0f)
                {
                    mMouseSensitivity = 0f;
                }
                else
                {
                    mMouseSensitivity = value;
                }
            }
        }
        #endregion

        #region invertMouseY
        /// <summary>
        /// Gets or sets a value indicating that mouse Y is inverted.
        /// </summary>
        /// <value><c>true</c> if mouse Y is inverted; otherwise, <c>false</c>.</value>
        public static bool InvertMouseY
        {
            get
            {
                return mInvertMouseY;
            }

            set
            {
                mInvertMouseY = value;
            }
        }
        #endregion

        #endregion

        // ---------------------------------------------------------------

        #region Synthesis

        #region Synthesis Setup Keys

        /// <summary>
        /// Creates new <see cref="KeyMapping"/> with specified name, active player, primary CustomInput, 
        /// and drive type.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="controlIndex">Integer index to specify which player is active.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="isTankDrive">Boolean to check if TankDrive is active.</param>
        public static KeyMapping SetKey(string name, int controlIndex, CustomInput primary, bool isTankDrive)
        {
            return Controls.Players[controlIndex].SetKey(name, ArgToInput(primary), null, isTankDrive);
        }

        public static KeyMapping SetKey(string name, int controlIndex, KeyCode primary, bool isTankDrive)
        {
            return Controls.Players[controlIndex].SetKey(name, ArgToInput(primary), null, isTankDrive);
        }

        /// <summary>
        /// Creates new <see cref="KeyMapping"/> with specified name, active player, primary KeyCode, 
        /// secondary CustomInput, and drive type.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="controlIndex">Integer index to specify which player is active.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="isTankDrive">Boolean to check if TankDrive is active.</param>
        public static KeyMapping SetKey(string name, int controlIndex, KeyCode primary, CustomInput secondary, bool isTankDrive)
        {
            return Controls.Players[controlIndex].SetKey(name, ArgToInput(primary), ArgToInput(secondary), isTankDrive);
        }

        /// <summary>
        /// Gets the list of ALL the keys.
        /// </summary>
        /// <returns>List of keys.</returns>
        public static ReadOnlyCollection<KeyMapping> GetKeysList()
        {
            mKeysList.Clear();
            foreach (Player player in Controls.Players)
            {
                foreach (KeyMapping key in player.GetActiveList())
                {
                    mKeysList.Add(key);
                }
            }
            return mKeysList.AsReadOnly();
        }

        #endregion

        #region Synthesis Setup Axes

        /// <summary>
        /// Creates new <see cref="Input.Axis"/> with specified negative <see cref="KeyMapping"/> and positive <see cref="KeyMapping"/>.
        /// </summary>
        /// <returns>Created Axis.</returns>
        /// <param name="name">Axis name.</param>
        /// <param name="controlIndex">Integer index to specify which player is active.</param>
        /// <param name="negative">Name of negative KeyMapping.</param>
        /// <param name="positive">Name of positive KeyMapping.</param>
        /// <param name="isTankDrive">Boolean to check if TankDrive is active.</param>
        public static Axis SetAxis(string name, int controlIndex, KeyMapping negative, KeyMapping positive, bool isTankDrive)
        {
            return Controls.Players[controlIndex].SetAxis(name, negative, positive, isTankDrive);
        }

        #endregion

        #endregion

        // ----------------------------------------------------------------
        // The following functions have not been implemented in Synthesis.
        // Source: https://github.com/Gris87/InputControl

        #region Setup/Remove Key Variations (With Different Arguments)

        #region SetKey with different arguments

        #region Level 1
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary)
        {
            return SetKey(name, ArgToInput(primary));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary)
        {
            return SetKey(name, ArgToInput(primary));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary)
        {
            return SetKey(name, ArgToInput(primary));
        }
        #endregion

        // ============================================================================================================

        #region Level 2

        #region Level 2-0
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, KeyCode secondary)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, MouseAxis secondary)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, MouseButton secondary)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary));
        }
        #endregion

        // ------------------------------------------------------------------------------------------------------------

        #region Level 2-1
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, CustomInput secondary)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, KeyCode secondary)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, MouseAxis secondary)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, MouseButton secondary)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary));
        }
        #endregion

        // ------------------------------------------------------------------------------------------------------------

        #region Level 2-2
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, CustomInput secondary)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, KeyCode secondary)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, MouseAxis secondary)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, MouseButton secondary)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary));
        }
        #endregion

        // ------------------------------------------------------------------------------------------------------------

        #region Level 2-3
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, CustomInput secondary)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, KeyCode secondary)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, MouseAxis secondary)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, MouseButton secondary)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary));
        }
        #endregion

        #endregion

        // ============================================================================================================

        #region Level 3

        #region Level 3-0

        #region Level 3-0-0
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, CustomInput secondary, KeyCode third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, CustomInput secondary, MouseAxis third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, CustomInput secondary, MouseButton third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }
        #endregion

        // ------------------------------------------------------------------------------------------------------------

        #region Level 3-0-1
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, KeyCode secondary, CustomInput third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, KeyCode secondary, KeyCode third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, KeyCode secondary, MouseAxis third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, KeyCode secondary, MouseButton third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }
        #endregion

        // ------------------------------------------------------------------------------------------------------------

        #region Level 3-0-2
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, MouseAxis secondary, CustomInput third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, MouseAxis secondary, KeyCode third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, MouseAxis secondary, MouseAxis third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, MouseAxis secondary, MouseButton third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }
        #endregion

        // ------------------------------------------------------------------------------------------------------------

        #region Level 3-0-3
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, MouseButton secondary, CustomInput third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, MouseButton secondary, KeyCode third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, MouseButton secondary, MouseAxis third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary, MouseButton secondary, MouseButton third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }
        #endregion

        #endregion

        // ************************************************************************************************************

        #region Level 3-1

        #region Level 3-1-0
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, CustomInput secondary, CustomInput third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, CustomInput secondary, KeyCode third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, CustomInput secondary, MouseAxis third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, CustomInput secondary, MouseButton third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }
        #endregion

        // ------------------------------------------------------------------------------------------------------------

        #region Level 3-1-1
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, KeyCode secondary, CustomInput third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, KeyCode secondary, KeyCode third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, KeyCode secondary, MouseAxis third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, KeyCode secondary, MouseButton third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }
        #endregion

        // ------------------------------------------------------------------------------------------------------------

        #region Level 3-1-2
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, MouseAxis secondary, CustomInput third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, MouseAxis secondary, KeyCode third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, MouseAxis secondary, MouseAxis third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, MouseAxis secondary, MouseButton third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }
        #endregion

        // ------------------------------------------------------------------------------------------------------------

        #region Level 3-1-3
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, MouseButton secondary, CustomInput third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, MouseButton secondary, KeyCode third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, MouseButton secondary, MouseAxis third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, KeyCode primary, MouseButton secondary, MouseButton third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }
        #endregion

        #endregion

        // ************************************************************************************************************

        #region Level 3-2

        #region Level 3-2-0
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, CustomInput secondary, CustomInput third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, CustomInput secondary, KeyCode third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, CustomInput secondary, MouseAxis third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, CustomInput secondary, MouseButton third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }
        #endregion

        // ------------------------------------------------------------------------------------------------------------

        #region Level 3-2-1
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, KeyCode secondary, CustomInput third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, KeyCode secondary, KeyCode third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, KeyCode secondary, MouseAxis third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, KeyCode secondary, MouseButton third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }
        #endregion

        // ------------------------------------------------------------------------------------------------------------

        #region Level 3-2-2
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, MouseAxis secondary, CustomInput third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, MouseAxis secondary, KeyCode third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, MouseAxis secondary, MouseAxis third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, MouseAxis secondary, MouseButton third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }
        #endregion

        // ------------------------------------------------------------------------------------------------------------

        #region Level 3-2-3
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, MouseButton secondary, CustomInput third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, MouseButton secondary, KeyCode third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, MouseButton secondary, MouseAxis third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseAxis primary, MouseButton secondary, MouseButton third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }
        #endregion

        #endregion

        // ************************************************************************************************************

        #region Level 3-3

        #region Level 3-3-0
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, CustomInput secondary, CustomInput third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, CustomInput secondary, KeyCode third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, CustomInput secondary, MouseAxis third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, CustomInput secondary, MouseButton third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }
        #endregion

        // ------------------------------------------------------------------------------------------------------------

        #region Level 3-3-1
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, KeyCode secondary, CustomInput third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, KeyCode secondary, KeyCode third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, KeyCode secondary, MouseAxis third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, KeyCode secondary, MouseButton third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }
        #endregion

        // ------------------------------------------------------------------------------------------------------------

        #region Level 3-3-2
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, MouseAxis secondary, CustomInput third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, MouseAxis secondary, KeyCode third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, MouseAxis secondary, MouseAxis third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, MouseAxis secondary, MouseButton third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }
        #endregion

        // ------------------------------------------------------------------------------------------------------------

        #region Level 3-3-3
        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, MouseButton secondary, CustomInput third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, MouseButton secondary, KeyCode third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, MouseButton secondary, MouseAxis third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, MouseButton primary, MouseButton secondary, MouseButton third)
        {
            return SetKey(name, ArgToInput(primary), ArgToInput(secondary), ArgToInput(third));
        }
        #endregion

        #endregion

        #endregion

        // ============================================================================================================

        #region Argument to Input fuctions
        /// <summary>
        /// Convert argument to <see cref="CustomInput"/>.
        /// </summary>
        /// <returns>Converted CustomInput.</returns>
        /// <param name="arg">Some kind of argument.</param>
        private static CustomInput ArgToInput(CustomInput arg)
        {
            return arg;
        }

        /// <summary>
        /// Convert argument to <see cref="CustomInput"/>.
        /// </summary>
        /// <returns>Converted CustomInput.</returns>
        /// <param name="arg">Some kind of argument.</param>
        private static CustomInput ArgToInput(KeyCode arg)
        {
            return new KeyboardInput(arg);
        }

        /// <summary>
        /// Convert argument to <see cref="CustomInput"/>.
        /// </summary>
        /// <returns>Converted CustomInput.</returns>
        /// <param name="arg">Some kind of argument.</param>
        private static CustomInput ArgToInput(MouseAxis arg)
        {
            return new MouseInput(arg);
        }

        /// <summary>
        /// Convert argument to <see cref="CustomInput"/>.
        /// </summary>
        /// <returns>Converted CustomInput.</returns>
        /// <param name="arg">Some kind of argument.</param>
        private static CustomInput ArgToInput(MouseButton arg)
        {
            return new MouseInput(arg);
        }
        #endregion

        #endregion

        /// <summary>
        /// Create new <see cref="KeyMapping"/> with specified name and inputs.
        /// </summary>
        /// <returns>Created KeyMapping.</returns>
        /// <param name="name">KeyMapping name.</param>
        /// <param name="primary">Primary input.</param>
        /// <param name="secondary">Secondary input.</param>
        /// <param name="third">Third input.</param>
        public static KeyMapping SetKey(string name, CustomInput primary = null, CustomInput secondary = null, CustomInput third = null)
        {
            KeyMapping outKey = null;

            if (mKeysMap.TryGetValue(name, out outKey))
            {
                outKey.primaryInput = primary;
                outKey.secondaryInput = secondary;
                outKey.thirdInput = third;
            }
            else
            {
                outKey = new KeyMapping(name, primary, secondary, third);

                mKeysList.Add(outKey);
                mKeysMap.Add(name, outKey);
            }

            return outKey;
        }

        /// <summary>
        /// Removes <see cref="KeyMapping"/> by name.
        /// </summary>
        /// <param name="name">KeyMapping name.</param>
        public static void RemoveKey(string name)
        {
            KeyMapping outKey = null;

            if (mKeysMap.TryGetValue(name, out outKey))
            {
                RemoveKey(outKey);
            }
        }

        /// <summary>
        /// Removes specified <see cref="KeyMapping"/>.
        /// </summary>
        /// <param name="key">KeyMapping instance.</param>
        public static void RemoveKey(KeyMapping key)
        {
            mKeysList.Remove(key);
            mKeysMap.Remove(key.name);
        }

        /// <summary>
        /// Gets <see cref="KeyMapping"/> by name.
        /// </summary>
        /// <param name="name">KeyMapping name.</param>
        public static KeyMapping Key(string name)
        {
            KeyMapping outKey = null;

            if (mKeysMap.TryGetValue(name, out outKey))
            {
                return outKey;
            }

            return null;
        }

        /// <summary>
        /// Gets the list of keys. FOR USE IF NOT USING SYNTHESIS KEYS
        /// </summary>
        /// <returns>List of keys.</returns>
        //public static ReadOnlyCollection<KeyMapping> getKeysList()
        //{
        //    return mKeysList.AsReadOnly();
        //}

        #endregion

        #region Setup/Remove Axes Variations (With Different Arguments)
        /// <summary>
        /// Create new <see cref="Input.Axis"/> with specified negative <see cref="KeyMapping"/> and positive <see cref="KeyMapping"/>.
        /// </summary>
        /// <returns>Created Axis.</returns>
        /// <param name="name">Axis name.</param>
        /// <param name="negative">Name of negative KeyMapping.</param>
        /// <param name="positive">Name of positive KeyMapping.</param>
        public static Axis SetAxis(string name, string negative, string positive)
        {
            KeyMapping negativeKey = null;
            KeyMapping positiveKey = null;

            if (!mKeysMap.TryGetValue(negative, out negativeKey))
            {
                Debug.LogError("Negative key \"" + negative + "\" not found for axis " + name);

                return null;
            }

            if (!mKeysMap.TryGetValue(positive, out positiveKey))
            {
                Debug.LogError("Positive key \"" + positive + "\" not found for axis " + name);

                return null;
            }

            return SetAxis(name, negativeKey, positiveKey);
        }

        /// <summary>
        /// Create new <see cref="Input.Axis"/> with specified negative <see cref="KeyMapping"/> and positive <see cref="KeyMapping"/>.
        /// </summary>
        /// <returns>Created Axis.</returns>
        /// <param name="name">Axis name.</param>
        /// <param name="negative">Negative KeyMapping.</param>
        /// <param name="positive">Positive KeyMapping.</param>
        public static Axis SetAxis(string name, KeyMapping negative, KeyMapping positive)
        {
            Axis outAxis = null;

            if (mAxesMap.TryGetValue(name, out outAxis))
            {
                outAxis.set(negative, positive);
            }
            else
            {
                outAxis = new Axis(name, negative, positive);

                mAxesList.Add(outAxis);
                mAxesMap.Add(name, outAxis);
            }

            return outAxis;
        }

        /// <summary>
        /// Removes <see cref="Input.Axis"/> by name.
        /// </summary>
        /// <param name="name">Axis name.</param>
        public static void RemoveAxis(string name)
        {
            Axis outAxis = null;

            if (mAxesMap.TryGetValue(name, out outAxis))
            {
                RemoveAxis(outAxis);
            }
        }

        /// <summary>
        /// Removes specified <see cref="Input.Axis"/>.
        /// </summary>
        /// <param name="axis">Axis instance.</param>
        public static void RemoveAxis(Axis axis)
        {
            mAxesList.Remove(axis);
            mAxesMap.Remove(axis.Name);
        }

        /// <summary>
        /// Gets <see cref="Input.Axis"/> by name.
        /// </summary>
        /// <param name="name">Axis name.</param>
        public static Axis Axis(string name)
        {
            Axis outAxis = null;

            if (mAxesMap.TryGetValue(name, out outAxis))
            {
                return outAxis;
            }

            return null;
        }

        /// <summary>
        /// Gets the list of axes.
        /// </summary>
        /// <returns>List of axes.</returns>
        [Obsolete("Please use getAxesList instead of this. Obsoletion date: 2014-12-28. It will be removed after 1 year")]
        public static List<Axis> GetAxes()
        {
            return mAxesList;
        }

        /// <summary>
        /// Gets the list of axes.
        /// </summary>
        /// <returns>List of axes.</returns>
        public static ReadOnlyCollection<Axis> GetAxesList()
        {
            return mAxesList.AsReadOnly();
        }
        #endregion

        // ----------------------------------------------------------------

        #region Get Inputs (GetButton, GetButtonDown, GetAxis, GetAxisRaw, Acceleration, Sensors, and More)
        /// <summary>
        /// Gets the last measured linear acceleration of a device in three-dimensional space.
        /// </summary>
        /// <value>Last measured linear acceleration of a device in three-dimensional space.</value>
        public static Vector3 Acceleration
        {
            get
            {
                return UnityEngine.Input.acceleration;
            }
        }

        /// <summary>
        /// Gets the number of acceleration measurements which occurred during last frame.
        /// </summary>
        /// <value>Number of acceleration measurements which occurred during last frame.</value>
        public static int AccelerationEventCount
        {
            get
            {
                return UnityEngine.Input.accelerationEventCount;
            }
        }

        /// <summary>
        /// Gets the list of acceleration measurements which occurred during the last frame. (Read Only) (Allocates temporary variables).
        /// </summary>
        /// <value>List of acceleration measurements which occurred during the last frame. (Read Only) (Allocates temporary variables).</value>
        public static AccelerationEvent[] AccelerationEvents
        {
            get
            {
                return UnityEngine.Input.accelerationEvents;
            }
        }

        /// <summary>
        /// Gets a value indicating that any key or mouse button currently held down.
        /// </summary>
        /// <value><c>true</c> if key or mouse button currently held down; otherwise, <c>false</c>.</value>
        public static bool AnyKey
        {
            get
            {
                return UnityEngine.Input.anyKey;
            }
        }

        /// <summary>
        /// Gets a value indicating that if it is a first frame the user hits any key or mouse button.
        /// </summary>
        /// <value><c>true</c> if user press any key during this frame; otherwise, <c>false</c>.</value>
        public static bool AnyKeyDown
        {
            get
            {
                return UnityEngine.Input.anyKeyDown;
            }
        }

        /// <summary>
        /// Property for accessing compass (handheld devices only).
        /// </summary>
        /// <value>Handheld device compass.</value>
        public static Compass Compass
        {
            get
            {
                return UnityEngine.Input.compass;
            }
        }

        /// <summary>
        /// This property controls if input sensors should be compensated for screen orientation.
        /// </summary>
        /// <value><c>true</c> if input sensors should be compensated for screen orientation; otherwise, <c>false</c>.</value>
        public static bool CompensateSensors
        {
            get
            {
                return UnityEngine.Input.compensateSensors;
            }
        }

        /// <summary>
        /// The current text input position used by IMEs to open windows.
        /// </summary>
        /// <value>Text input position.</value>
        public static Vector2 CompositionCursorPos
        {
            get
            {
                return UnityEngine.Input.compositionCursorPos;
            }
        }

        /// <summary>
        /// The current IME composition string being typed by the user.
        /// </summary>
        /// <value>Current IME composition string.</value>
        public static string CompositionString
        {
            get
            {
                return UnityEngine.Input.compositionString;
            }
        }

        /// <summary>
        /// Returns input that currently active by used.
        /// </summary>
        /// <returns>Currently active input.</returns>
        /// <param name="ignoreMouseMovement">If set to <c>true</c> ignore mouse movement.</param>
        /// <param name="useModifiers">If set to <c>true</c> handle key modifiers.</param>
        public static CustomInput CurrentInput(bool ignoreMouseMovement = true, bool useModifiers = false)
        {
            KeyModifier modifiers = KeyModifier.NoModifier;

            if (useModifiers)
            {
                if (UnityEngine.Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.RightControl))
                {
                    modifiers |= KeyModifier.Ctrl;
                }

                if (UnityEngine.Input.GetKey(KeyCode.LeftAlt) || UnityEngine.Input.GetKey(KeyCode.RightAlt))
                {
                    modifiers |= KeyModifier.Alt;
                }

                if (UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift))
                {
                    modifiers |= KeyModifier.Shift;
                }
            }

            #region Joystick
            for (int i = (int)Joystick.Joystick1; i < (int)Joystick.None; ++i)
            {
                String target = "Joystick " + i.ToString() + " ";

                #region Axes
                for (int j = 1; j <= (int)JoystickAxis.None / 2; ++j)
                {
                    float joyAxis = UnityEngine.Input.GetAxis(target + "Axis " + j.ToString());

                    if (joyAxis < -mJoystickThreshold)
                    {
                        return new JoystickInput((JoystickAxis)((j - 1) * 2 + 1), (Joystick)i, modifiers);
                    }

                    if (joyAxis > mJoystickThreshold)
                    {
                        return new JoystickInput((JoystickAxis)((j - 1) * 2), (Joystick)i, modifiers);
                    }
                }
                #endregion

                #region Buttons
                for (int j = 0; j < (int)JoystickButton.None; ++j)
                {
                    if (UnityEngine.Input.GetButton(target + "Button " + (j + 1).ToString()))
                    {
                        return new JoystickInput((JoystickButton)j, (Joystick)i, modifiers);
                    }
                }
                #endregion
            }
            #endregion

            #region Mouse

            #region Axes

            #region ScrollWheel
            float mouseAxis = UnityEngine.Input.GetAxis("Mouse ScrollWheel");

            if (mouseAxis < 0)
            {
                return new MouseInput(MouseAxis.WheelDown, modifiers);
            }

            if (mouseAxis > 0)
            {
                return new MouseInput(MouseAxis.WheelUp, modifiers);
            }
            #endregion

            if (!ignoreMouseMovement)
            {
                #region X
                mouseAxis = UnityEngine.Input.GetAxis("Mouse X");

                if (mouseAxis < 0)
                {
                    return new MouseInput(MouseAxis.MouseLeft, modifiers);
                }

                if (mouseAxis > 0)
                {
                    return new MouseInput(MouseAxis.MouseRight, modifiers);
                }
                #endregion

                #region Y
                mouseAxis = UnityEngine.Input.GetAxis("Mouse Y");

                if (mouseAxis < 0)
                {
                    return new MouseInput(MouseAxis.MouseDown, modifiers);
                }

                if (mouseAxis > 0)
                {
                    return new MouseInput(MouseAxis.MouseUp, modifiers);
                }
                #endregion
            }
            #endregion

            #region Buttons
            for (int i = 0; i < (int)MouseButton.None; ++i)
            {
                KeyCode key = (KeyCode)((int)KeyCode.Mouse0 + i);

                if (UnityEngine.Input.GetKey(key))
                {
                    return new MouseInput((MouseButton)i, modifiers);
                }
            }
            #endregion

            #endregion

            #region Keyboard
            foreach (var value in Enum.GetValues(typeof(KeyCode)))
            {
                KeyCode key = (KeyCode)value;

                if ((!useModifiers
                     ||
                     (
                      key != KeyCode.LeftControl
                      &&
                      key != KeyCode.RightControl
                      &&
                      key != KeyCode.LeftAlt
                      &&
                      key != KeyCode.RightAlt
                      &&
                      key != KeyCode.LeftShift
                      &&
                      key != KeyCode.RightShift
                      )
                    )
                    &&
                    UnityEngine.Input.GetKey(key)
                   )
                {
                    return new KeyboardInput(key, modifiers);
                }
            }
            #endregion

            return null;
        }

        /// <summary>
        /// Gets the device physical orientation as reported by OS.
        /// </summary>
        /// <value>Device orientation.</value>
        public static DeviceOrientation DeviceOrientation
        {
            get
            {
                return UnityEngine.Input.deviceOrientation;
            }
        }

        /// <summary>
        /// Returns specific acceleration measurement which occurred during last frame. (Does not allocate temporary variables).
        /// </summary>
        /// <returns>Specific acceleration measurement which occurred during last frame.</returns>
        /// <param name="index">Index of acceleration event.</param>
        public static AccelerationEvent GetAccelerationEvent(int index)
        {
            return UnityEngine.Input.GetAccelerationEvent(index);
        }

        /// <summary>
        /// Returns the value of the virtual axis identified by axisName.
        /// </summary>
        /// <returns>Value of the virtual axis.</returns>
        /// <param name="axisName">Axis name.</param>
        /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
        public static float GetAxis(string axisName, bool exactKeyModifiers = false)
        {
            if (freeze) return 0;
            float previousValue;

            if (!mSmoothAxesValues.TryGetValue(axisName, out previousValue))
            {
                previousValue = 0f;
            }

            float totalCoefficient = mSmoothCoefficient * Time.deltaTime;

            if (totalCoefficient > 1)
            {
                totalCoefficient = 1;
            }

            float newValue = GetAxisRaw(axisName, exactKeyModifiers);
            float res = previousValue + (newValue - previousValue) * totalCoefficient;

            mSmoothAxesValues[axisName] = res;

            return res;
        }

        /// <summary>
        /// Returns the value of the virtual axis.
        /// </summary>
        /// <returns>Value of the virtual axis.</returns>
        /// <param name="axis">Axis instance.</param>
        /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
        public static float GetAxis(Axis axis, bool exactKeyModifiers = false)
        {
            if (freeze) return 0;
            float previousValue;

            if (!mSmoothAxesValues.TryGetValue(axis.Name, out previousValue))
            {
                previousValue = 0f;
            }

            float totalCoefficient = mSmoothCoefficient * Time.deltaTime;

            if (totalCoefficient > 1)
            {
                totalCoefficient = 1;
            }

            float newValue = GetAxisRaw(axis, exactKeyModifiers);
            float res = previousValue + (newValue - previousValue) * totalCoefficient;

            mSmoothAxesValues[axis.Name] = res;

            return res;
        }

        /// <summary>
        /// Returns the value of the virtual axis identified by axisName with no smoothing filtering applied.
        /// </summary>
        /// <returns>Value of the virtual axis.</returns>
        /// <param name="axisName">Axis name.</param>
        /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
        public static float GetAxisRaw(string axisName, bool exactKeyModifiers = false)
        {
            if (freeze) return 0;
            float sensitivity = 1f;

            #region Standard axes
            if (axisName.Equals("Mouse X"))
            {
                sensitivity = mMouseSensitivity;
            }
            else
            if (axisName.Equals("Mouse Y"))
            {
                if (mInvertMouseY)
                {
                    sensitivity = -mMouseSensitivity;
                }
                else
                {
                    sensitivity = mMouseSensitivity;
                }
            }
            #endregion

            Axis outAxis = null;

            if (!mAxesMap.TryGetValue(axisName, out outAxis))
            {
                if (
                    !axisName.Equals("Mouse X")
                    &&
                    !axisName.Equals("Mouse Y")
                    &&
                    !axisName.Equals("Mouse ScrollWheel")
                   )
                {
                    Debug.LogError("Axis \"" + axisName + "\" not found. Using InputManager axis");
                }

                return UnityEngine.Input.GetAxisRaw(axisName) * sensitivity;
            }

            return outAxis.getValue(exactKeyModifiers, mInputDevice) * sensitivity;
        }

        /// <summary>
        /// Returns the value of the virtual axis with no smoothing filtering applied.
        /// </summary>
        /// <returns>Value of the virtual axis.</returns>
        /// <param name="axis">Axis instance.</param>
        /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
        public static float GetAxisRaw(Axis axis, bool exactKeyModifiers = false)
        {
            if (freeze) return 0;
            float sensitivity = 1f;

            #region Standard axes
            if (axis.Name.Equals("Mouse X"))
            {
                sensitivity = mMouseSensitivity;
            }
            else
            if (axis.Name.Equals("Mouse Y"))
            {
                if (mInvertMouseY)
                {
                    sensitivity = -mMouseSensitivity;
                }
                else
                {
                    sensitivity = mMouseSensitivity;
                }
            }
            #endregion

            return axis.getValue(exactKeyModifiers, mInputDevice) * sensitivity;
        }

        /// <summary>
        /// Returns true while the virtual button identified by buttonName is held down.
        /// </summary>
        /// <returns><c>true</c>, if button is held down, <c>false</c> otherwise.</returns>
        /// <param name="buttonName">Button name.</param>
        /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
        public static bool GetButton(string buttonName, bool exactKeyModifiers = false)
        {
            if (freeze) return false;
            KeyMapping outKey = null;

            if (!mKeysMap.TryGetValue(buttonName, out outKey))
            {
                Debug.LogError("Key \"" + buttonName + "\" not found");
                return false;
            }

            return outKey.isPressed(exactKeyModifiers, mInputDevice);
        }

        /// <summary>
        /// Returns true while the virtual button is held down.
        /// </summary>
        /// <returns><c>true</c>, if button is held down, <c>false</c> otherwise.</returns>
        /// <param name="button">KeyMapping instance.</param>
        /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
        public static bool GetButton(KeyMapping button, bool exactKeyModifiers = false)
        {
            if (freeze) return false;
            return button.isPressed(exactKeyModifiers, mInputDevice);
        }

        /// <summary>
        /// Returns true during the frame the user pressed down the virtual button identified by buttonName.
        /// </summary>
        /// <returns><c>true</c>, if user pressed down the button during the frame, <c>false</c> otherwise.</returns>
        /// <param name="buttonName">Button name.</param>
        /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
        public static bool GetButtonDown(string buttonName, bool exactKeyModifiers = false)
        {
            if (freeze) return false;

            KeyMapping outKey = null;

            if (!mKeysMap.TryGetValue(buttonName, out outKey))
            {
                Debug.LogError("Key \"" + buttonName + "\" not found");
                return false;
            }

            return outKey.isPressedDown(exactKeyModifiers, mInputDevice);
        }

        /// <summary>
        /// Returns true during the frame the user pressed down the virtual button.
        /// </summary>
        /// <returns><c>true</c>, if user pressed down the button during the frame, <c>false</c> otherwise.</returns>
        /// <param name="button">KeyMapping instance.</param>
        /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
        public static bool GetButtonDown(KeyMapping button, bool exactKeyModifiers = false)
        {
            if (freeze) return false;
            return button.isPressedDown(exactKeyModifiers, mInputDevice);
        }

        /// <summary>
        /// Returns true the first frame the user releases the virtual button identified by buttonName.
        /// </summary>
        /// <returns><c>true</c>, if user releases the button during the frame, <c>false</c> otherwise.</returns>
        /// <param name="buttonName">Button name.</param>
        /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
        public static bool GetButtonUp(string buttonName, bool exactKeyModifiers = false)
        {
            if (freeze) return false;
            KeyMapping outKey = null;

            if (!mKeysMap.TryGetValue(buttonName, out outKey))
            {
                Debug.LogError("Key \"" + buttonName + "\" not found");
                return false;
            }

            return outKey.isPressedUp(exactKeyModifiers, mInputDevice);
        }

        /// <summary>
        /// Returns true the first frame the user releases the virtual button.
        /// </summary>
        /// <returns><c>true</c>, if user releases the button during the frame, <c>false</c> otherwise.</returns>
        //// <param name="button">KeyMapping instance.</param>
        /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
        public static bool GetButtonUp(KeyMapping button, bool exactKeyModifiers = false)
        {
            if (freeze) return false;
            return button.isPressedUp(exactKeyModifiers, mInputDevice);
        }

        /// <summary>
        /// Gets the count of connected joysticks.
        /// </summary>
        /// <returns>Count of connected joysticks.</returns>
        public static int GetJoystickCount()
        {
            return UnityEngine.Input.GetJoystickNames().Length;
        }

        /// <summary>
        /// Returns an array of strings describing the connected joysticks.
        /// </summary>
        /// <returns>Names of connected joysticks.</returns>
        public static string[] GetJoystickNames()
        {
            return UnityEngine.Input.GetJoystickNames();
        }

        /// <summary>
        /// Returns true while the user holds down the key identified by name. Think auto fire.
        /// </summary>
        /// <returns><c>true</c>, if key is held down, <c>false</c> otherwise.</returns>
        /// <param name="name">Name of key.</param>
        public static bool GetKey(string name)
        {
            if (freeze) return false;
            return UnityEngine.Input.GetKey(name);
        }

        /// <summary>
        /// Returns true while the user holds down the key identified by the key <see cref="KeyCode"/> enum parameter.
        /// </summary>
        /// <returns><c>true</c>, if key is held down, <c>false</c> otherwise.</returns>
        /// <param name="key">Code of key.</param>
        public static bool GetKey(KeyCode key)
        {
            if (freeze) return false;
            return UnityEngine.Input.GetKey(key);
        }

        /// <summary>
        /// Returns true during the frame the user starts pressing down the key identified by name.
        /// </summary>
        /// <returns><c>true</c>, if user starts pressing down the key, <c>false</c> otherwise.</returns>
        /// <param name="name">Name of key.</param>
        public static bool GetKeyDown(string name)
        {
            if (freeze) return false;
            return UnityEngine.Input.GetKeyDown(name);
        }

        /// <summary>
        /// Returns true during the frame the user starts pressing down the key identified by the key <see cref="KeyCode"/> enum parameter.
        /// </summary>
        /// <returns><c>true</c>, if user starts pressing down the key, <c>false</c> otherwise.</returns>
        /// <param name="name">Code of key.</param>
        public static bool GetKeyDown(KeyCode key)
        {
            if (freeze) return false;
            return UnityEngine.Input.GetKeyDown(key);
        }

        /// <summary>
        /// Returns true during the frame the user releases the key identified by name.
        /// </summary>
        /// <returns><c>true</c>, if user releases the key, <c>false</c> otherwise.</returns>
        /// <param name="name">Name of key.</param>
        public static bool GetKeyUp(string name)
        {
            if (freeze) return false;
            return UnityEngine.Input.GetKeyUp(name);
        }

        /// <summary>
        /// Returns true during the frame the user releases the key identified by the key <see cref="KeyCode"/> enum parameter.
        /// </summary>
        /// <returns><c>true</c>, if user releases the key, <c>false</c> otherwise.</returns>
        /// <param name="name">Code of key.</param>
        public static bool GetKeyUp(KeyCode key)
        {
            if (freeze) return false;
            return UnityEngine.Input.GetKeyUp(key);
        }

        /// <summary>
        /// Returns whether the given mouse button is held down.
        /// </summary>
        /// <returns><c>true</c>, if mouse button is held down, <c>false</c> otherwise.</returns>
        /// <param name="button">Mouse button.</param>
        public static bool GetMouseButton(int button)
        {
            if (freeze) return false;
            if (button >= 0 && button < (int)MouseButton.None)
            {
                return GetMouseButton((MouseButton)button);
            }

            return false;
        }

        /// <summary>
        /// Returns whether the given <see cref="MouseButton"/> is held down.
        /// </summary>
        /// <returns><c>true</c>, if mouse button is held down, <c>false</c> otherwise.</returns>
        /// <param name="button">Mouse button.</param>
        public static bool GetMouseButton(MouseButton button)
        {
            if (freeze) return false;
            if (button != MouseButton.None)
            {
                return UnityEngine.Input.GetKey((KeyCode)((int)KeyCode.Mouse0 + (int)button));
            }

            return false;
        }

        /// <summary>
        /// Returns true during the frame the user pressed the given mouse button.
        /// </summary>
        /// <returns><c>true</c>, if user pressed mouse button, <c>false</c> otherwise.</returns>
        /// <param name="button">Mouse button.</param>
        public static bool GetMouseButtonDown(int button)
        {
            if (freeze) return false;
            if (button >= 0 && button < (int)MouseButton.None)
            {
                return GetMouseButtonDown((MouseButton)button);
            }

            return false;
        }

        /// <summary>
        /// Returns true during the frame the user pressed the given <see cref="MouseButton"/>.
        /// </summary>
        /// <returns><c>true</c>, if user pressed mouse button, <c>false</c> otherwise.</returns>
        /// <param name="button">Mouse button.</param>
        public static bool GetMouseButtonDown(MouseButton button)
        {
            if (freeze) return false;
            if (button != MouseButton.None)
            {
                return UnityEngine.Input.GetKeyDown((KeyCode)((int)KeyCode.Mouse0 + (int)button));
            }

            return false;
        }

        /// <summary>
        /// Returns true during the frame the user releases the given mouse button.
        /// </summary>
        /// <returns><c>true</c>, if user releases mouse button, <c>false</c> otherwise.</returns>
        /// <param name="button">Mouse button.</param>
        public static bool GetMouseButtonUp(int button)
        {
            if (freeze) return false;
            if (button >= 0 && button < (int)MouseButton.None)
            {
                return GetMouseButtonUp((MouseButton)button);
            }

            return false;
        }

        /// <summary>
        /// Returns true during the frame the user releases the given <see cref="MouseButton"/>.
        /// </summary>
        /// <returns><c>true</c>, if user releases mouse button, <c>false</c> otherwise.</returns>
        /// <param name="button">Mouse button.</param>
        public static bool GetMouseButtonUp(MouseButton button)
        {
            if (freeze) return false;
            if (button != MouseButton.None)
            {
                return UnityEngine.Input.GetKeyUp((KeyCode)((int)KeyCode.Mouse0 + (int)button));
            }

            return false;
        }

        /// <summary>
        /// Returns object representing status of a specific touch. (Does not allocate temporary variables).
        /// </summary>
        /// <returns>Touch instance.</returns>
        /// <param name="index">Touch index.</param>
        public static Touch GetTouch(int index)
        {
            return UnityEngine.Input.GetTouch(index);
        }
        #endregion

        #region Other Input Functions: Gyro, IMECompositionMode, IMEIsSelected, InputString, Location, Position, Touch, and More
        /// <summary>
        /// Returns default gyroscope.
        /// </summary>
        /// <value>Default gyroscope.</value>
        public static Gyroscope Gyro
        {
            get
            {
                return UnityEngine.Input.gyro;
            }
        }

        /// <summary>
        /// Controls enabling and disabling of IME input composition.
        /// </summary>
        /// <value>IME composition mode.</value>
        public static IMECompositionMode ImeCompositionMode
        {
            get
            {
                return UnityEngine.Input.imeCompositionMode;
            }

            set
            {
                UnityEngine.Input.imeCompositionMode = value;
            }
        }

        /// <summary>
        /// Gets a value indicating that the user have an IME keyboard input source selected.
        /// </summary>
        /// <value><c>true</c> if IME keyboard input source selected; otherwise, <c>false</c>.</value>
        public static bool ImeIsSelected
        {
            get
            {
                return UnityEngine.Input.imeIsSelected;
            }
        }

        /// <summary>
        /// Returns the keyboard input entered this frame. (Read Only)
        /// </summary>
        /// <value>Keyboard input.</value>
        public static string InputString
        {
            get
            {
                return UnityEngine.Input.inputString;
            }
        }

        /// <summary>
        /// Returns device location (handheld devices only).
        /// </summary>
        /// <value>Handheld device location.</value>
        public static LocationService Location
        {
            get
            {
                return UnityEngine.Input.location;
            }
        }

        /// <summary>
        /// Gets the current mouse position in pixel coordinates.
        /// </summary>
        /// <value>Current mouse position.</value>
        public static Vector3 MousePosition
        {
            get
            {
                return UnityEngine.Input.mousePosition;
            }
        }

        /// <summary>
        /// Gets a value indicating that mouse is present.
        /// </summary>
        /// <value><c>true</c> if mouse is present; otherwise, <c>false</c>.</value>
        public static bool MousePresent
        {
            get
            {
                return UnityEngine.Input.mousePresent;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating that the system handles multiple touches.
        /// </summary>
        /// <value><c>true</c> if system handles multiple touches; otherwise, <c>false</c>.</value>
        public static bool MultiTouchEnabled
        {
            get
            {
                return UnityEngine.Input.multiTouchEnabled;
            }

            set
            {
                UnityEngine.Input.multiTouchEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the preferred input device. (Any, Keyboard and mouse, joystick).
        /// </summary>
        /// <value>Preferred input device.</value>
        public static InputDevice PreferredInputDevice
        {
            get
            {
                return mInputDevice;
            }

            set
            {
                mInputDevice = value;
            }
        }

        /// <summary>
        /// Resets all input. After ResetInputAxes all axes return to 0 and all buttons return to 0 for one frame.
        /// </summary>
        public static void ResetInputAxes()
        {
            UnityEngine.Input.ResetInputAxes();
        }

        /// <summary>
        /// Gets or sets a value indicating that mouse actions are simulated as touches.
        /// </summary>
        /// <value><c>true</c> if mouse actions are simulated as touches; otherwise, <c>false</c>.</value>
        public static bool SimulateMouseWithTouches
        {
            get
            {
                return UnityEngine.Input.simulateMouseWithTouches;
            }

            set
            {
                UnityEngine.Input.simulateMouseWithTouches = value;
            }
        }

        /// <summary>
        /// Gets the number of touches. Guaranteed not to change throughout the frame.
        /// </summary>
        /// <value>Number of touches.</value>
        public static int TouchCount
        {
            get
            {
                return UnityEngine.Input.touchCount;
            }
        }

        /// <summary>
        /// Returns list of objects representing status of all touches during last frame. (Read Only) (Allocates temporary variables).
        /// </summary>
        /// <value>List of touches.</value>
        public static Touch[] Touches
        {
            get
            {
                return UnityEngine.Input.touches;
            }
        }

        /// <summary>
        /// Returns whether the device on which application is currently running supports touch input.
        /// </summary>
        /// <value><c>true</c> if touch supported; otherwise, <c>false</c>.</value>
        public static bool TouchSupported
        {
            get
            {
                return UnityEngine.Input.touchSupported;
            }
        }
        #endregion
    }
}
