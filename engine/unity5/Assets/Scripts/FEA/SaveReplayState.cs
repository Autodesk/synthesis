using Assets.Scripts.FSM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;

namespace Assets.Scripts.FEA
{
    public class SaveReplayState : SimState
    {
        string fieldPath;

        List<Tracker> trackers;
        List<List<ContactDescriptor>> contacts;

        GameObject canvas;
        Button saveButton;
        Button cancelButton;
        Text replayNameText;

        /// <summary>
        /// Initializes a new SaveReplayState instance.
        /// </summary>
        /// <param name="trackers"></param>
        /// <param name="contacts"></param>
        public SaveReplayState(string fieldPath, List<Tracker> trackers, List<List<ContactDescriptor>> contacts)
        {
            this.fieldPath = fieldPath;
            this.trackers = trackers;
            this.contacts = contacts;
        }

        /// <summary>
        /// Establishes references to GUI components and disables camera movement.
        /// </summary>
        public override void Start()
        {
            //canvas = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("SaveReplayCanvas")).First();
            //canvas.SetActive(true);

            saveButton = GameObject.Find("SaveButton").GetComponent<Button>();
            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(Save);

            cancelButton = GameObject.Find("CancelButton").GetComponent<Button>();
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(Cancel);

            replayNameText = GameObject.Find("ReplayNameText").GetComponent<Text>();

            DynamicCamera.MovingEnabled = false;
        }

        /// <summary>
        /// Renders the UserMessageManager.
        /// </summary>
        public override void OnGUI()
        {
            UserMessageManager.Render();
        }

        /// <summary>
        /// Disables the canvas and re-enables camera movement.
        /// </summary>
        public override void End()
        {
            //canvas.SetActive(false);
            DynamicCamera.MovingEnabled = true;
        }

        /// <summary>
        /// Saves the replay from the provided file name.
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrEmpty(replayNameText.text) || replayNameText.text.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                UserMessageManager.Dispatch("Please enter a valid replay name!", 5f);
                return;
            }

            ReplayExporter.Write(replayNameText.text, fieldPath, trackers, contacts);
            StateMachine.Instance.PopState();

            if (SimUI.changeAnalytics)
            {
                Analytics.CustomEvent("Saved Replay", new Dictionary<string, object> //for analytics tracking
                {
                });
            }
        }

        /// <summary>
        /// Cancels saving and exits the state.
        /// </summary>
        public void Cancel()
        {
            StateMachine.Instance.PopState();
        }
    }
}
