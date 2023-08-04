using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using SynthesisAPI.Utilities;
using UnityEngine;
using static MatchResultsTracker;
using Logger = SynthesisAPI.Utilities.Logger;

namespace Modes.MatchMode {
    public class MatchMode : IMode {
        public static float MatchTime = 15f;

        public static MatchResultsTracker MatchResultsTracker;

        /// Integers to represent which robots the user selected in the MatchModeModal
        public static int[] SelectedRobots = new int[6];

        /// Whether or not the robot should snap to a grid in positioning mode
        public static bool[] RoundSpawnLocation = new bool[6];

        /// The un-rounded spawn position of all robots
        public static (Vector3 position, Quaternion rotation)[] RawSpawnLocations = new(
            Vector3 position, Quaternion rotation)[6];

        /// Rounds the robots spawn position if the user chooses to
        public static (Vector3 position, Quaternion rotation) GetSpawnLocation(int robot) {
            if (RoundSpawnLocation[robot])
                return SpawnLocationPanel.RoundSpawnLocation(RawSpawnLocations[robot]);

            return RawSpawnLocations[robot];
        }

        public static List<RobotSimObject> Robots = new List<RobotSimObject>();

        public const string PREVIOUS_SPAWN_LOCATION = "Previous Spawn Location";
        public const string PREVIOUS_SPAWN_ROTATION = "Previous Spawn Rotation";

        private MatchStateMachine _stateMachine;

        public void Start() {
            DynamicUIManager.CreateModal<MatchModeModal>();
            EventBus.NewTypeListener<OnScoreUpdateEvent>(HandleScoreEvent);

            Array.Fill(SelectedRobots, -1);
            Array.Fill(RawSpawnLocations, (Vector3.zero, Quaternion.identity));

            _stateMachine = MatchStateMachine.Instance;
            _stateMachine.SetState(MatchStateMachine.StateName.MatchConfig);

            SetupMatchResultTracking();
            MainHUD.SetUpMatch();
        }

        private void HandleScoreEvent(IEvent e) {
            if (e.GetType() != typeof(OnScoreUpdateEvent))
                return;
            OnScoreUpdateEvent scoreUpdateEvent = e as OnScoreUpdateEvent;
            if (scoreUpdateEvent == null)
                return;

            ScoringZone zone = scoreUpdateEvent.Zone;
            int points       = zone.Points * (scoreUpdateEvent.IncreaseScore ? 1 : -1);

            switch (zone.Alliance) {
                case Alliance.Blue:
                    Scoring.blueScore += points;
                    break;
                case Alliance.Red:
                    Scoring.redScore += points;
                    break;
            }
        }

        /// Creates a MatchResultsTracker and event listeners to update it
        public void SetupMatchResultTracking() {
            MatchResultsTracker = new MatchResultsTracker();

            EventBus.NewTypeListener<OnScoreUpdateEvent>(e => {
                ScoringZone zone = ((OnScoreUpdateEvent) e).Zone;
                switch (zone.Alliance) {
                    case Alliance.Blue:
                        ((BluePoints) MatchResultsTracker.MatchResultEntries[typeof(BluePoints)]).Points += zone.Points;
                        break;
                    case Alliance.Red:
                        ((RedPoints) MatchResultsTracker.MatchResultEntries[typeof(RedPoints)]).Points += zone.Points;
                        break;
                }
            });
        }

        public void Update() {
            if (_stateMachine != null) {
                _stateMachine.Update();

                if (MatchTime <= 0 && _stateMachine.CurrentState.StateName is >= MatchStateMachine.StateName.Auto and <=
                                          MatchStateMachine.StateName.Endgame)
                    _stateMachine.AdvanceState();
            }
        }

        public void End() {
            Scoring.redScore  = 0;
            Scoring.blueScore = 0;
            Robots.Clear();
            EventBus.RemoveTypeListener<OnScoreUpdateEvent>(HandleScoreEvent);
        }

        public void OpenMenu() {}

        public void CloseMenu() {}

        /// Spawns in all of the selected robots and disables physics for spawn location selection
        public static void SpawnAllRobots() {
            var robotsFolder = ParsePath("$appdata/Autodesk/Synthesis/Mira", '/');
            if (!Directory.Exists(robotsFolder))
                Directory.CreateDirectory(robotsFolder);
            var robotFiles =
                Directory.GetFiles(robotsFolder).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();

            int i = 0;
            SelectedRobots.ForEach(x => {
                if (x != -1) {
                    Vector3 position              = new Vector3(2 * i - 6, -2.5f, 0);
                    RawSpawnLocations[i].position = position;

                    RobotSimObject.SpawnRobot(null, position, Quaternion.identity, false, robotFiles[x]);
                    Robots.Add(RobotSimObject.GetCurrentlyPossessedRobot());
                } else
                    Robots.Add(null);
                i++;
            });
        }

        /// Resets the currently selected robots and field
        public static void ResetMatchConfiguration() {
            Robots = new List<RobotSimObject>();
            Array.Fill(SelectedRobots, -1);
        }

        public static string ParsePath(string p, char c) {
            string[] a = p.Split(c);
            string b   = "";
            for (int i = 0; i < a.Length; i++) {
                switch (a[i]) {
                    case "$appdata":
                        b += Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                        break;
                    default:
                        b += a[i];
                        break;
                }
                if (i != a.Length - 1)
                    b += Path.AltDirectorySeparatorChar;
            }
            return b;
        }
    }
}
