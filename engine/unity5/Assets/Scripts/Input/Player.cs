using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using System.Linq;

//=========================================================================================
//                                      Player Class
// Description: manages the each player's controls
//=========================================================================================

namespace Synthesis.Input
{
    public class Player
    {
        public const int PLAYER_COUNT = 6;
        
        public const Profile.Mode DEFAULT_PROFILE_MODE = Profile.Mode.ArcadeKeyboard;

        private int index; // TODO: necessary to track this here?

        //Checks if tank drive is enabled
        private Profile.Mode activeProfileMode;

        private Profile[] profiles;

        public Player(int playerIndex)
        {
            index = playerIndex;

            profiles = new Profile[(int)System.Enum.GetValues(typeof(Profile.Mode)).Cast<Profile.Mode>().Max() + 1]; // Set array to number of enum values

            for (int i = 0; i < profiles.Length; i++) {
                profiles[i] = new Profile();
                ResetProfile((Profile.Mode)i);
            }

            activeProfileMode = DEFAULT_PROFILE_MODE;
        }

        public Profile GetProfile(Profile.Mode profileMode)
        {
            return profiles[(int)profileMode];
        }

        private string MakePrefPrefix()
        {
            return "Controls.Player" + index + "." + activeProfileMode;
        }

        public void SaveActiveProfile()
        {
            PlayerPrefs.SetString(MakePrefPrefix(), profiles[(int)activeProfileMode].ToString());
            PlayerPrefs.Save();
        }

        public bool CheckIfSaved()
        {
            if (PlayerPrefs.GetString(MakePrefPrefix()) != profiles[(int)activeProfileMode].ToString())
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
                profiles[(int)activeProfileMode].FromString(input);
                profiles[(int)activeProfileMode].UpdateFieldControls(index);
            }
            else
            {
                ResetProfile(activeProfileMode);
            }

            if (!CheckIfSaved())
                SaveActiveProfile();
        }

        public Profile.Buttons GetButtons()
        {
            return profiles[(int)activeProfileMode].buttons;
        }

        public Profile.Axes GetAxes()
        {
            return profiles[(int)activeProfileMode].axes;
        }

        /// <summary>
        /// Gets a list of all active player profile controls.
        /// </summary>
        public ReadOnlyCollection<KeyMapping> GetActiveList()
        {
            List<KeyMapping> list = new List<KeyMapping>();
            foreach (var field in profiles[(int)activeProfileMode].buttons.GetType().GetFields())
            {
                if ((field.FieldType == typeof(KeyMapping)))
                {
                    if ((KeyMapping)field.GetValue(profiles[(int)activeProfileMode].buttons) != null)
                        list.Add((KeyMapping)field.GetValue(profiles[(int)activeProfileMode].buttons));
                }
                else if ((field.FieldType == typeof(KeyMapping[])))
                {
                    foreach (var mapping in (KeyMapping[])field.GetValue(profiles[(int)activeProfileMode].buttons))
                    {
                        if (mapping != null)
                        {
                            list.Add(mapping);
                        }
                    }
                }
                else if ((field.FieldType == typeof(List<KeyMapping>)))
                {
                    foreach (var mapping in (List<KeyMapping>)field.GetValue(profiles[(int)activeProfileMode].buttons))
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

        public void SetActiveProfileMode(Profile.Mode profileMode)
        {
            activeProfileMode = profileMode;
        }

        public Profile.Mode GetActiveProfileMode()
        {
            return activeProfileMode;
        }

        public void ResetProfile(Profile.Mode profileMode)
        {
            profiles[(int)profileMode] = Profile.CreateDefault(index, profileMode);
        }
    }
}
