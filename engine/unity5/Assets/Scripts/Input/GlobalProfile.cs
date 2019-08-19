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
            public KeyMapping cameraForward;
            [JsonProperty]
            public KeyMapping cameraBackward;
            [JsonProperty]
            public KeyMapping cameraRight;
            [JsonProperty]
            public KeyMapping cameraLeft;
            [JsonProperty]
            public KeyMapping cameraUp;
            [JsonProperty]
            public KeyMapping cameraDown;

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

                list.Add(cameraForward);
                list.Add(cameraBackward);
                list.Add(cameraLeft);
                list.Add(cameraRight);
                list.Add(cameraUp);
                list.Add(cameraDown);

                list.Add(resetField);
                list.Add(cameraToggle);
                list.Add(scoreboard);
                list.Add(replayMode);

                return list;
            }
        }

        public class Axes
        {
            public Axis cameraForward;
            public Axis cameraLateral;
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

        private static JsonSerializerSettings JSON_SETTINGS = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };

        public new string ToString()
        {
            return JsonConvert.SerializeObject(this, JSON_SETTINGS);
        }

        public void FromString(string input)
        {
            Debug.Log("Before " + buttons.cameraToggle.primaryInput.ToString());
            JsonConvert.PopulateObject(input, this, JSON_SETTINGS);
            Debug.Log("After " + buttons.cameraToggle.primaryInput.ToString());
        }

        public List<KeyMapping> GetList()
        {
            return buttons.ToList();
        }

        public void Reset()
        {
            #region Default Controls

            buttons.cameraForward = new KeyMapping("Camera Forward", KeyCode.W);
            buttons.cameraBackward = new KeyMapping("Camera Backward", KeyCode.S);
            buttons.cameraRight = new KeyMapping("Camera Right", KeyCode.D);
            buttons.cameraLeft = new KeyMapping("Camera Left", KeyCode.A);
            buttons.cameraUp = new KeyMapping("Camera Up", KeyCode.Space);
            buttons.cameraDown = new KeyMapping("Camera Down", KeyCode.LeftShift);

            axes.cameraForward= new Axis("Camera Forward Axis", buttons.cameraBackward, buttons.cameraForward);
            axes.cameraLateral = new Axis("Camera Lateral Axis", buttons.cameraLeft, buttons.cameraRight);
            axes.cameraVertical = new Axis("Camera Vertical Axis", buttons.cameraDown, buttons.cameraUp);

            buttons.resetField = new KeyMapping("Reset Field", KeyCode.F);
            buttons.replayMode = new KeyMapping("Replay Mode", KeyCode.Tab);
            buttons.cameraToggle = new KeyMapping("Camera Toggle", KeyCode.C);
            buttons.scoreboard = new KeyMapping("Scoreboard", KeyCode.Q);

            #endregion
        }
    }
}