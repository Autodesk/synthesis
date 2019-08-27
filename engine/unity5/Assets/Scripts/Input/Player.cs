using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using System.Linq;
using Synthesis.GUI;
using System;

//=========================================================================================
//                                      Player Class
// Description: manages the each player's controls
//=========================================================================================

namespace Synthesis.Input
{
    public class Player
    {
        public const int PLAYER_COUNT = 6;
        
        public const Profile.Mode DEFAULT_PROFILE_MODE = Profile.Mode.Arcade;

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
            }

            activeProfileMode = DEFAULT_PROFILE_MODE;
            ResetActiveProfile();
        }

        public Profile GetActiveProfile()
        {
            return profiles[(int)activeProfileMode];
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

        public bool HasBeenSaved()
        {
            return PlayerPrefs.GetString(MakePrefPrefix()) == profiles[(int)activeProfileMode].ToString();
        }

        public void LoadActiveProfile()
        {
            string input = PlayerPrefs.GetString(MakePrefPrefix());
            if (input != "")
            {
                try
                {
                    profiles[(int)activeProfileMode].FromString(input);
                }
                catch (Exception)
                {
                    UserMessageManager.Dispatch("Error loading controls. Resetting to defaults.", 5);
                    profiles[(int)activeProfileMode].Reset(index, activeProfileMode);
                }
                profiles[(int)activeProfileMode].UpdateFieldControls(index);
            }
            else
            {
                ResetActiveProfile();
            }

            if (!HasBeenSaved())
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
        public List<KeyMapping> GetActiveList()
        {
            return profiles[(int)activeProfileMode].buttons.ToList();
        }

        public void SetActiveProfileMode(Profile.Mode profileMode)
        {
            activeProfileMode = profileMode;
        }

        public Profile.Mode GetActiveProfileMode()
        {
            return activeProfileMode;
        }

        public void ResetActiveProfile()
        {
            profiles[(int)activeProfileMode].Reset(index, activeProfileMode);
        }
    }
}
