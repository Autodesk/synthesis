using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using UnityEngine.UI;
using Synthesis.Input.Inputs;
using Synthesis.Field;
using System.Linq;
using Newtonsoft.Json;

//=========================================================================================
//                                      Player Class
// Description: Controls the individual player's controls by generating individual lists
///             for each player through <see cref="KeyMapping"/>KeyMapping Lists and Maps
//=========================================================================================

namespace Synthesis.Input
{
    public class Player
    {
        public const int PLAYER_COUNT = 6;
        public enum ControlProfile // Order must correspond to that in the dropdown list within Unity
        {
            // ArcadeJoystick,
            ArcadeKeyboard,
            // MecanumJoystick,
            // MecanumKeyboard,
            TankJoystick,
            // TankKeyboard
            // Custom1,
            // Custom2,
            // Custom3
        }

        public class UnhandledControlProfileException : System.Exception
        {
            public UnhandledControlProfileException(){}

            public UnhandledControlProfileException(string message): base(message){}

            public UnhandledControlProfileException(string message, System.Exception inner): base(message, inner){}
        }

        public const ControlProfile DEFAULT_CONTROL_PROFILE = ControlProfile.ArcadeKeyboard;

        /// <summary>
        /// <see cref="Buttons"/> is a set of user defined buttons.
        /// </summary>
        public class Buttons
        {
            //Basic robot controls
            [JsonProperty]
            public KeyMapping forward;
            [JsonProperty]
            public KeyMapping backward;
            [JsonProperty]
            public KeyMapping left;
            [JsonProperty]
            public KeyMapping right;

            //Tank drive controls
            [JsonProperty]
            public KeyMapping tankFrontLeft;
            [JsonProperty]
            public KeyMapping tankBackLeft;
            [JsonProperty]
            public KeyMapping tankFrontRight;
            [JsonProperty]
            public KeyMapping tankBackRight;

            //Remaining PWM Controls
            [JsonProperty]
            public KeyMapping[] pwmPos;
            [JsonProperty]
            public KeyMapping[] pwmNeg;

            //Other controls
            [JsonProperty]
            public KeyMapping resetRobot;
            [JsonProperty]
            public KeyMapping resetField;
            [JsonProperty]
            public KeyMapping cameraToggle;
            [JsonProperty]
            public KeyMapping scoreboard;
            [JsonProperty]
            public KeyMapping trajectory;
            [JsonProperty]
            public KeyMapping replayMode;
            [JsonProperty]
            public KeyMapping duplicateRobot;
            [JsonProperty]
            public KeyMapping switchActiveRobot;

            //driver practice controls
            [JsonProperty]
            public List<KeyMapping> pickup;
            [JsonProperty]
            public List<KeyMapping> release;
            [JsonProperty]
            public List<KeyMapping> spawnPieces;

            public Buttons()
            {
                pwmPos = new KeyMapping[DriveJoints.PWM_COUNT - DriveJoints.PWM_OFFSET];

                pwmNeg = new KeyMapping[DriveJoints.PWM_COUNT - DriveJoints.PWM_OFFSET];

                pickup = new List<KeyMapping>();
                release = new List<KeyMapping>();
                spawnPieces = new List<KeyMapping>();

                // Leave fields null
            }
        }

        /// <summary>
        /// <see cref="Axes"/> is a set of user defined axes.
        /// </summary>
        public class Axes
        {
            //Arcade Axes
            public Axis vertical;
            public Axis horizontal;

            //Tank Axes
            public Axis tankLeftAxes;
            public Axis tankRightAxes;

            //PWM Axes

            public Axis[] pwmAxes;

            public Axes()
            {
                pwmAxes = new Axis[DriveJoints.PWM_COUNT - DriveJoints.PWM_OFFSET];
                // Leave fields null
            }
        }

        public class Profile
        {
            /// <summary>
            /// Set of buttons.
            /// </summary>
            [JsonProperty]
            public Buttons buttons;

            /// <summary>
            /// Set of axes.
            /// </summary>
            [JsonProperty]
            public Axes axes;

            public Profile()
            {
                buttons = new Buttons();
                axes = new Axes();
            }

            public new string ToString()
            {
                return JsonConvert.SerializeObject(buttons, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });
            }

            public void FromString(string input, bool init = false)
            {
                if (init)
                {
                    buttons = JsonConvert.DeserializeObject<Buttons>(input, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });
                }
                else
                {
                    JsonConvert.PopulateObject(input, buttons, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    });
                }
            }
        }

        private int index; // TODO: necessary to track this here?

        //Checks if tank drive is enabled
        private ControlProfile activeControlProfile;

        //The list and controls called on the current player
        private Profile activeProfile;

        private Profile[] profiles;

        public Player(int playerIndex)
        {
            index = playerIndex;

            profiles = new Profile[(int)System.Enum.GetValues(typeof(ControlProfile)).Cast<ControlProfile>().Max() + 1]; // Set array to number of enum values

            for (int i = 0; i < profiles.Length; i++) {
                profiles[i] = new Profile();
                ResetProfile((ControlProfile)i);
            }

            SetControlProfile(DEFAULT_CONTROL_PROFILE);
        }

        public Profile GetProfile(ControlProfile controlProfile) // TODO move Controls initialization into this file
        {
            return profiles[(int)controlProfile];
        }

        private string MakePrefPrefix()
        {
            return "Controls.Player" + index + "." + activeControlProfile;
        }

        public void SaveActiveProfile()
        {
            PlayerPrefs.SetString(MakePrefPrefix(), profiles[(int)activeControlProfile].ToString());
            PlayerPrefs.Save();
        }

        public bool CheckIfSaved()
        {
            if (PlayerPrefs.GetString(MakePrefPrefix()) != activeProfile.ToString())
            {
                return false;
            }
            return true;
        }

        public void LoadActiveProfile()
        {
            string input = PlayerPrefs.GetString(MakePrefPrefix());
            if (input != "")
            {
                profiles[(int)activeControlProfile].FromString(input);

                Controls.UpdateFieldControls(index);

                if (!CheckIfSaved())
                    SaveActiveProfile();
                SetControlProfile(activeControlProfile);
            }
            else
            {
                ResetProfile(activeControlProfile);

                Controls.UpdateFieldControls(index); // TODO: move into create default?

                if (!CheckIfSaved())
                    SaveActiveProfile();
            }
        }

        public Buttons GetButtons()
        {
            return activeProfile.buttons;
        }

        public Axes GetAxes()
        {
            return activeProfile.axes;
        }

        /// <summary>
        /// Gets the active player list.
        /// </summary>
        /// <returns>The active player list.</returns>
        public ReadOnlyCollection<KeyMapping> GetActiveList()
        {
            List<KeyMapping> list = new List<KeyMapping>();
            foreach (var field in activeProfile.buttons.GetType().GetFields())
            {
                if ((field.FieldType == typeof(KeyMapping)))
                {
                    if ((KeyMapping)field.GetValue(activeProfile.buttons) != null)
                        list.Add((KeyMapping)field.GetValue(activeProfile.buttons));
                }
                else if ((field.FieldType == typeof(KeyMapping[])))
                {
                    foreach (var mapping in (KeyMapping[])field.GetValue(activeProfile.buttons))
                    {
                        if (mapping != null)
                        {
                            list.Add(mapping);
                        }
                    }
                }
                else if ((field.FieldType == typeof(List<KeyMapping>)))
                {
                    foreach (var mapping in (List<KeyMapping>)field.GetValue(activeProfile.buttons))
                    {
                        if (mapping != null)
                        {
                            list.Add(mapping);
                        }
                    }
                }
                else
                {
                    throw new System.Exception("Unhandled Buttons field type " + field.FieldType.Name);
                }
            }
            return list.AsReadOnly();
        }

        public void SetControlProfile(ControlProfile p)
        {
            activeControlProfile = p;
            activeProfile = profiles[(int)activeControlProfile];
        }

        public ControlProfile GetActiveControlProfile()
        {
            return activeControlProfile;
        }

        public void ResetProfile(ControlProfile controlProfile)
        {
            profiles[(int)controlProfile] = Controls.CreateDefault(index, controlProfile);
            SetControlProfile(controlProfile);
        }
    }
}
