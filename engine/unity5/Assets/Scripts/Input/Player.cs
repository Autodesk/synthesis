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

            public List<KeyMapping> GetKeyMappingList()
            {
                List<KeyMapping> list = new List<KeyMapping>();
                foreach (var field in buttons.GetType().GetFields())
                {
                    if ((field.FieldType == typeof(KeyMapping)))
                    {
                        if ((KeyMapping)field.GetValue(buttons) != null)
                            list.Add((KeyMapping)field.GetValue(buttons));
                    }
                    else if ((field.FieldType == typeof(KeyMapping[])))
                    {
                        foreach(var mapping in (KeyMapping[])field.GetValue(buttons))
                        {
                            if(mapping != null)
                            {
                                list.Add(mapping);
                            }
                        }
                    }
                    else if ((field.FieldType == typeof(List<KeyMapping>)))
                    {
                        foreach (var mapping in (List<KeyMapping>)field.GetValue(buttons))
                        {
                            if (mapping != null)
                            {
                                list.Add(mapping);
                            }
                        }
                    }
                    else
                    {
                        throw new System.Exception("Unhandled field type " + field.FieldType.Name);
                    }
                }
                return list;
            }
        }

        public Player(int i)
        {
            index = i;

            arcadeDriveProfile = new Profile();
            tankDriveProfile = new Profile();

            ResetArcade();
            ResetTank();

            SetControlProfile(DEFAULT_CONTROL_PROFILE);
        }

        private int index; // TODO: necessary to track this here?

        //Checks if tank drive is enabled
        private ControlProfile activeControlProfile;

        //The list and controls called on the current player
        private Profile activeProfile;

        //Set of arcade drive keys
        private Profile arcadeDriveProfile;

        //Set of tank drive keys
        private Profile tankDriveProfile;

        public Profile GetProfile(ControlProfile controlProfile) // TODO move Controls initialization into this file
        {
            switch (controlProfile)
            {
                case ControlProfile.TankJoystick:
                    return tankDriveProfile;
                case ControlProfile.ArcadeKeyboard:
                    return arcadeDriveProfile;
                default:
                    throw new System.Exception("Unsupported control profile"); // TODO make custom exception
            }
        }

        private string MakePrefPrefix()
        {
            return "Controls.Player" + index + "." + activeControlProfile;
        }

        public void SaveActiveProfile()
        {
            switch (activeControlProfile)
            {
                case ControlProfile.TankJoystick:
                    PlayerPrefs.SetString(MakePrefPrefix(), tankDriveProfile.ToString());
                    break;
                case ControlProfile.ArcadeKeyboard:
                    PlayerPrefs.SetString(MakePrefPrefix(), arcadeDriveProfile.ToString());
                    break;
                default:
                    throw new System.Exception("Unsupported control profile"); // TODO make custom exception
            }
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
                switch (activeControlProfile)
                {
                    case ControlProfile.TankJoystick:
                        tankDriveProfile.FromString(input);
                        break;
                    case ControlProfile.ArcadeKeyboard:
                        arcadeDriveProfile.FromString(input);
                        break;
                    default:
                        throw new System.Exception("Unsupported control profile"); // TODO make custom exception
                }
                Controls.UpdateFieldControls(index);
                if(!CheckIfSaved())
                    SaveActiveProfile();
                SetControlProfile(activeControlProfile);
            }
            else
            {
                switch (activeControlProfile)
                {
                    case ControlProfile.TankJoystick:
                        ResetTank();
                        break;
                    case ControlProfile.ArcadeKeyboard:
                        ResetArcade();
                        break;
                    default:
                        throw new System.Exception("Unsupported control profile"); // TODO make custom exception
                }
                Controls.UpdateFieldControls(index);
                if (!CheckIfSaved())
                    SaveActiveProfile();
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
            /*
            for(int i = 0; i < activeList.Count; i++)
            {
                if ((activeList[i].name.Contains(" Pick Up ") || activeList[i].name.Contains(" Release ") || activeList[i].name.Contains(" Spawn ")) &&
                    FieldDataHandler.gamepieces.Where(g => activeList[i].name.Contains(g.name)).ToArray().Count() == 0)
                {
                    switch (activeControlProfile) // Remove gamepiece controls when field changes // TODO only do when field changes
                    {
                        case ControlProfile.ArcadeKeyboard:
                            arcadeDriveList.Remove(activeList[i]);
                            resetArcadeDriveList.Remove(activeList[i]);
                            break;
                        case ControlProfile.TankJoystick:
                            tankDriveList.Remove(activeList[i]);
                            resetTankDriveList.Remove(activeList[i]);
                            break;
                        default:
                            throw new System.Exception("Unsupported control profile");
                    }
                    activeList.RemoveAt(i);
                }
            }
            */
            return activeProfile.GetKeyMappingList().AsReadOnly();
        }

        public void SetControlProfile(ControlProfile p)
        {
            activeControlProfile = p;
            switch (activeControlProfile)
            {
                case ControlProfile.ArcadeKeyboard:
                    activeProfile = arcadeDriveProfile;
                    break;
                case ControlProfile.TankJoystick:
                    activeProfile = tankDriveProfile;
                    break;
                default:
                    throw new System.Exception("Unsupported control profile");
            }
        }

        public ControlProfile GetActiveControlProfile()
        {
            return activeControlProfile;
        }

        /// <summary>
        /// Resets the activeList to tank drive defaults.
        /// </summary>
        public void ResetTank()
        {
            tankDriveProfile = Controls.CreateDefault(index, ControlProfile.TankJoystick);
            SetControlProfile(ControlProfile.TankJoystick);
        }

        /// <summary>
        /// Resets the activeList to arcade drive defaults.
        /// </summary>
        public void ResetArcade()
        {
            arcadeDriveProfile = Controls.CreateDefault(index, ControlProfile.ArcadeKeyboard);
            SetControlProfile(ControlProfile.ArcadeKeyboard);
        }
    }
}
