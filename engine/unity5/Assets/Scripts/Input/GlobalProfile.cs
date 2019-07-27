using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

//=========================================================================================
//                                   GlobalProfile Class
// Description: Manages the global control mappings for all player profiles
//=========================================================================================

namespace Synthesis.Input
{
    public class GlobalProfile
    {
        public class Buttons
        {
            // Camera controls
            [JsonProperty]
            public KeyMapping cameraVerticalPos;
            [JsonProperty]
            public KeyMapping cameraVerticalNeg;
            [JsonProperty]
            public KeyMapping cameraHorizontalPos;
            [JsonProperty]
            public KeyMapping cameraHorizontalNeg;

            //Other controls
            [JsonProperty]
            public KeyMapping resetField;
            [JsonProperty]
            public KeyMapping cameraToggle;
            [JsonProperty]
            public KeyMapping scoreboard;
            [JsonProperty]
            public KeyMapping replayMode;

            public Buttons()
            {
                // Leave fields null
            }

            public List<KeyMapping> ToList()
            {
                // Keep this up-to-date with all this class's fields
                // The order they appear here is the order they'll appear in the controls menu
                List<KeyMapping> list = new List<KeyMapping>();

                list.Add(cameraVerticalPos);
                list.Add(cameraVerticalNeg);
                list.Add(cameraHorizontalPos);
                list.Add(cameraHorizontalNeg);

                list.Add(resetField);
                list.Add(cameraToggle);
                list.Add(scoreboard);
                list.Add(replayMode);

                return list;
            }
        }

        public class Axes
        {
            public Axis cameraHorizontal;
            public Axis cameraVertical;

            public Axes()
            {
                // Leave fields null
            }
        }

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

        public GlobalProfile()
        {
            buttons = new Buttons();
            axes = new Axes();

            Reset();
        }

        public Buttons GetButtons()
        {
            return buttons;
        }

        public Axes GetAxes()
        {
            return axes;
        }

        private static JsonSerializerSettings JSON_SETTINGS = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        };

        private new string ToString()
        {
            return JsonConvert.SerializeObject(this, JSON_SETTINGS);
        }

        private void FromString(string input)
        {
            JsonConvert.PopulateObject(input, this, JSON_SETTINGS);
        }

        public List<KeyMapping> GetList()
        {
            return buttons.ToList();
        }

        private string MakePrefPrefix()
        {
            return "Controls.Global";
        }

        public void Save()
        {
            PlayerPrefs.SetString(MakePrefPrefix(), ToString());
            PlayerPrefs.Save();
        }

        public bool HasBeenSaved()
        {
            return PlayerPrefs.GetString(MakePrefPrefix()) == ToString();
        }

        public void Load()
        {
            string input = PlayerPrefs.GetString(MakePrefPrefix());
            if (input != "")
                FromString(input);
            else
                Reset();

            if (!HasBeenSaved())
                Save();
        }

        public void Reset()
        {
            #region Default Controls

            buttons.cameraVerticalPos = new KeyMapping("Camera Vertical Pos", KeyCode.W);
            buttons.cameraVerticalNeg = new KeyMapping("Camera Vertical Neg", KeyCode.S);
            buttons.cameraHorizontalPos = new KeyMapping("Camera Horizontal Pos", KeyCode.D);
            buttons.cameraHorizontalNeg = new KeyMapping("Camera Horizontal Neg", KeyCode.A);

            axes.cameraVertical = new Axis("Camera Vertical", buttons.cameraVerticalNeg, buttons.cameraVerticalPos);
            axes.cameraHorizontal = new Axis("Camera Horizontal", buttons.cameraHorizontalNeg, buttons.cameraHorizontalPos);

            buttons.resetField = new KeyMapping("Reset Field", KeyCode.F);
            buttons.replayMode = new KeyMapping("Replay Mode", KeyCode.Tab);
            buttons.cameraToggle = new KeyMapping("Camera Toggle", KeyCode.C);
            buttons.scoreboard = new KeyMapping("Scoreboard", KeyCode.Q);

            #endregion
        }
    }
}