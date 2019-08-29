using Newtonsoft.Json;
using Synthesis.GUI;
using System;
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
            [JsonProperty]
            public KeyMapping cameraRotateLeft;
            [JsonProperty]
            public KeyMapping cameraRotateRight;
            [JsonProperty]
            public KeyMapping cameraTiltUp;
            [JsonProperty]
            public KeyMapping cameraTiltDown;
            [JsonProperty]
            public KeyMapping cameraRollLeft;
            [JsonProperty]
            public KeyMapping cameraRollRight;
            // TODO add camera yaw
            //Other controls
            [JsonProperty]
            public KeyMapping resetField;
            [JsonProperty]
            public KeyMapping cameraToggle;
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
                list.Add(cameraRotateLeft);
                list.Add(cameraRotateRight);
                list.Add(cameraTiltUp);
                list.Add(cameraTiltDown);
                list.Add(cameraRollLeft);
                list.Add(cameraRollRight);

                list.Add(resetField);
                list.Add(cameraToggle);
                list.Add(replayMode);

                return list;
            }
        }

        public class Axes
        {
            [JsonProperty]
            public Axis cameraForward;
            [JsonProperty]
            public Axis cameraLateral;
            [JsonProperty]
            public Axis cameraVertical;
            [JsonProperty]
            public Axis cameraRotation;
            [JsonProperty]
            public Axis cameraTilt;
            [JsonProperty]
            public Axis cameraRoll;

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

        public static JsonSerializerSettings JSON_SETTINGS = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            MissingMemberHandling = MissingMemberHandling.Error
        };

        public new string ToString()
        {
            return JsonConvert.SerializeObject(this, JSON_SETTINGS);
        }

        public void FromString(string input)
        {
            try
            {
                JsonConvert.PopulateObject(input, this, JSON_SETTINGS);
            }
            catch (Exception)
            {
                throw;
            }
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
            buttons.cameraRotateLeft = new KeyMapping("Camera Rotate Left", KeyCode.Q);
            buttons.cameraRotateRight = new KeyMapping("Camera Rotate Right", KeyCode.E);
            buttons.cameraTiltUp = new KeyMapping("Camera Tilt Up", KeyCode.X);
            buttons.cameraTiltDown = new KeyMapping("Camera Tilt Down", KeyCode.Z);
            buttons.cameraRollLeft = new KeyMapping("Camera Roll Left", KeyCode.LeftBracket);
            buttons.cameraRollRight = new KeyMapping("Camera Roll Right", KeyCode.RightBracket);

            axes.cameraForward= new Axis("Camera Forward Axis", buttons.cameraBackward, buttons.cameraForward);
            axes.cameraLateral = new Axis("Camera Lateral Axis", buttons.cameraLeft, buttons.cameraRight);
            axes.cameraVertical = new Axis("Camera Vertical Axis", buttons.cameraDown, buttons.cameraUp);
            axes.cameraRotation = new Axis("Camera Rotation Axis", buttons.cameraRotateLeft, buttons.cameraRotateRight);
            axes.cameraTilt = new Axis("Camera Tilt Axis", buttons.cameraTiltDown, buttons.cameraTiltUp);
            axes.cameraRoll = new Axis("Camera Roll Axis", buttons.cameraRollLeft, buttons.cameraRollRight);

            buttons.resetField = new KeyMapping("Reset Field", KeyCode.F);
            buttons.replayMode = new KeyMapping("Replay Mode", KeyCode.Tab);
            buttons.cameraToggle = new KeyMapping("Camera Toggle", KeyCode.C);

            #endregion
        }
    }
}