using System.Collections.Generic;
using UnityEngine;

//=========================================================================================
//                                      Global Player Class
// Description: manages the global player controls
//=========================================================================================

namespace Synthesis.Input
{
    public class GlobalPlayer
    {
        private GlobalProfile profile;

        public GlobalPlayer()
        {
            profile = new GlobalProfile();

            Reset();
        }

        public GlobalProfile GetProfile()
        {
            return profile;
        }

        private string MakePrefPrefix()
        {
            return "Controls.Global";
        }

        public void Save()
        {
            PlayerPrefs.SetString(MakePrefPrefix(), profile.ToString());
            PlayerPrefs.Save();
        }

        public bool HasBeenSaved()
        {
            return PlayerPrefs.GetString(MakePrefPrefix()) == profile.ToString();
        }

        public void Load()
        {
            string input = PlayerPrefs.GetString(MakePrefPrefix());
            if (input != "")
            {
                profile.FromString(input);
            }
            else
            {
                profile.Reset();
            }
        }

        public GlobalProfile.Buttons GetButtons()
        {
            return profile.buttons;
        }

        public GlobalProfile.Axes GetAxes()
        {
            return profile.axes;
        }

        /// <summary>
        /// Gets a list of all active player profile controls.
        /// </summary>
        public List<KeyMapping> GetList()
        {
            return profile.buttons.ToList();
        }

        public void Reset()
        {
            profile.Reset();
        }
    }
}
