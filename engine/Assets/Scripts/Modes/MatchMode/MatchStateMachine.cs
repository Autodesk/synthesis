using System.Collections.Generic;
using Synthesis.Physics;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using UI.Dynamic.Modals;
using UnityEngine;

namespace Modes.MatchMode {
    public class MatchStateMachine {
        private static MatchStateMachine _instance;

        public static MatchStateMachine Instance {
            get {
                if (_instance == null)
                    _instance = new MatchStateMachine();

                return _instance;
            }
        }

#region State Management

        private readonly Dictionary<StateName, MatchState> _matchStates = new Dictionary<StateName, MatchState>();
        private MatchState _currentState;
        public MatchState CurrentState => _currentState;

        /// Sets the current state. Automatically calls any event functions in the state
        /// <param name="stateName">The new state to switch to</param>
        public void SetState(StateName stateName) {
            Debug.Log($"State set to {stateName}");
            var newState = _matchStates[stateName];
            if (newState == null) {
                Debug.LogError($"No state found for {stateName}");
                return;
            }

            _currentState.End();
            _currentState = newState;
            _currentState.Start();
        }

        public void AdvanceState() {
            StateName currentStateName = _currentState.StateName;
            var nextStateName          = currentStateName + 1;
            SetState(nextStateName);
        }

        /// Manages the state of the match (ex: match config, teleop, match results)
        public MatchStateMachine() {
            _matchStates.Add(StateName.None, new None());
            _matchStates.Add(StateName.MatchConfig, new MatchConfig());
            _matchStates.Add(StateName.RobotPositioning, new RobotPositioning());
            _matchStates.Add(StateName.FieldConfig, new FieldConfig());
            _matchStates.Add(StateName.Auto, new Auto());
            _matchStates.Add(StateName.Transition, new Transition());
            _matchStates.Add(StateName.Teleop, new Teleop());
            _matchStates.Add(StateName.Endgame, new Endgame());
            _matchStates.Add(StateName.MatchResults, new MatchResults());
            _matchStates.Add(StateName.Restart, new Restart());
            _matchStates.Add(StateName.Reconfigure, new Reconfigure());

            _currentState = _matchStates[StateName.None];
        }

        public void Update() {
            _currentState.Update();
        }

#endregion

#region Match States

        /// Called whenever a new match state is started
        public class OnStateStarted : IEvent {
            public MatchState state;
            public StateName stateName;

            public OnStateStarted(MatchState state, StateName stateName) {
                this.state     = state;
                this.stateName = stateName;
            }

            // state passes to next in SpawnLocationPanel accept button
        }

        /// Called whenever a match state is ended
        public class OnStateEnded : IEvent {
            public MatchState state;
            public StateName stateName;

            public OnStateEnded(MatchState state, StateName stateName) {
                this.state     = state;
                this.stateName = stateName;
            }
        }

        /// A specific state during match mode
        public abstract class MatchState {
            public StateName StateName;

            public MatchState(StateName stateName) {
                this.StateName = stateName;
            }

            public virtual void Start() {
                EventBus.Push(new OnStateStarted(this, StateName));
            }

            public abstract void Update();

            public virtual void End() {
                EventBus.Push(new OnStateEnded(this, StateName));
            }
        }

        /// An empty state
        public class None : MatchState {
            public override void Start() {
                base.Start();
            }

            public override void Update() {}

            public override void End() {
                base.End();
            }

            public None() : base(StateName.None) {}
        }

        /// When the user is choosing which robots to spawn in and other match settings
        public class MatchConfig : MatchState {
            public override void Start() {
                base.Start();
                DynamicUIManager.CreateModal<MatchModeModal>();
                DynamicUIManager.ActiveModal.OnAccepted += () => Instance.SetState(StateName.RobotPositioning);
            }

            public override void Update() {}

            public override void End() {
                base.End();
            }

            public MatchConfig() : base(StateName.MatchConfig) {}
        }

        /// When the user is choosing where the robot will spawn
        public class RobotPositioning : MatchState {
            public override void Start() {
                base.Start();

                PhysicsManager.IsFrozen = true;
                MatchMode.SpawnAllRobots();

                if (Camera.main != null) {
                    FreeCameraMode camMode = CameraController.CameraModes["Freecam"] as FreeCameraMode;
                    Camera.main.GetComponent<CameraController>().CameraMode = camMode;
                    var location                                            = new Vector3(0, 6, -8);
                    camMode.SetTransform(location, Quaternion.LookRotation(-location.normalized,
                                                       Vector3.Cross(-location.normalized, Vector3.right)));
                }

                // state passes to next in SpawnLocationPanel accept button
            }

            public override void Update() {}

            public override void End() {
                base.End();

                PhysicsManager.IsFrozen = false;

                if (Camera.main != null) {
                    Camera.main.GetComponent<CameraController>().CameraMode = CameraController.CameraModes["Orbit"];
                }
            }

            public RobotPositioning() : base(StateName.RobotPositioning) {}
        }

        // might expand to include more than scoring zones if necessary
        public class FieldConfig : MatchState {
            public override void Start() {
                base.Start();
                DynamicUIManager.CreatePanel<ScoringZonesPanel>(true);
                var panel = DynamicUIManager.GetPanel<ScoringZonesPanel>();

                panel.OnAccepted += () => {
                    DynamicUIManager.CreateModal<ConfirmModal>("Start Match?");
                    DynamicUIManager.ActiveModal.OnAccepted += () => {
                        DynamicUIManager.CloseActiveModal();
                        DynamicUIManager.CreatePanel<ScoreboardPanel>(true, true);
                        Instance.SetState(StateName.Auto);
                    };
                    DynamicUIManager.ActiveModal.OnCancelled += () => {
                        DynamicUIManager.CloseActiveModal();
                        Instance.SetState(StateName.FieldConfig);
                    };
                };
            }

            public override void Update() {}

            public override void End() {
                base.End();
            }

            public FieldConfig() : base(StateName.FieldConfig) {}
        }

        /// <summary>
        /// The autonomous state at the beginning of a match
        /// </summary>
        public class Auto : MatchState {
            public override void Start() {
                base.Start();
                Scoring.targetTime = 15;
                DynamicUIManager.CreatePanel<ScoreboardPanel>(true, true);
            }

            public override void Update() {}

            public override void End() {
                base.End();
            }

            public Auto() : base(StateName.Auto) {}
        }

        /// <summary>
        ///  3 second transition state between Auto and Teleop
        /// </summary>
        public class Transition : MatchState {
            private float _timer;
            private string _possessed;

            public override void Start() {
                base.Start();
                _possessed                             = RobotSimObject.CurrentlyPossessedRobot;
                RobotSimObject.CurrentlyPossessedRobot = string.Empty;
                Scoring.targetTime                     = 135;
                _timer                                 = 3;
            }

            public override void Update() {
                _timer -= Time.deltaTime;
                if (_timer <= 0) {
                    Instance.AdvanceState();
                }
            }

            public override void End() {
                RobotSimObject.CurrentlyPossessedRobot = _possessed;
            }

            public Transition() : base(StateName.Transition) {}
        }

        /// <summary>
        /// The teleop state of a match
        /// </summary>
        public class Teleop : MatchState {
            public override void Start() {
                base.Start();
            }

            public override void Update() {
                if (Scoring.targetTime <= 30)
                    Instance.AdvanceState();
            }

            public override void End() {}

            public Teleop() : base(StateName.Teleop) {}
        }

        public class Endgame : MatchState {
            public override void Update() {}

            public Endgame() : base(StateName.Endgame) {}
        }

        /// A state when a modal is displayed after a match showing info about the match
        public class MatchResults : MatchState {
            public override void Start() {
                base.Start();

                DynamicUIManager.CreateModal<MatchResultsModal>();
            }

            public override void Update() {}

            public override void End() {}

            public MatchResults() : base(StateName.MatchResults) {}
        }

        /// Restarts the match with the same configuration
        public class Restart : MatchState {
            public override void Start() {
                base.Start();

                // Reset robots to their selected spawn position
                int i = 0;
                MatchMode.Robots.ForEach(x => {
                    if (x != null) {
                        (Vector3 position, Quaternion rotation) location = MatchMode.GetSpawnLocation(i);

                        Transform robot = x.RobotNode.transform;

                        robot.position = Vector3.zero;
                        robot.rotation = Quaternion.identity;

                        robot.rotation = location.rotation * Quaternion.Inverse(x.GroundedNode.transform.rotation);
                        robot.position = location.position - x.GroundedNode.transform.localToWorldMatrix.MultiplyPoint(
                                                                 x.GroundedBounds.center);
                    }
                    i++;
                });

                // TODO: reset the match results tracker
                // TODO: reset the scoreboard and timer
                // TODO: add a modal or panel to start the match so it doesn't instantly start
                Instance.SetState(StateName.Auto);
            }

            public override void Update() {}

            public override void End() {}

            public Restart() : base(StateName.Restart) {}
        }

        /// Resets the match and sends he user back to the MatchConfig modal
        public class Reconfigure : MatchState {
            public override void Start() {
                RobotSimObject.RemoveAllRobots();
                FieldSimObject.DeleteField();
                MatchMode.ResetMatchConfiguration();

                // TODO: reset the match results tracker
                // TODO: reset the scoreboard and timer
                Instance.SetState(StateName.MatchConfig);
            }

            public override void Update() {}

            public override void End() {}

            public Reconfigure() : base(StateName.Reconfigure) {}
        }

#endregion

        /// Represents a specific MatchState
        public enum StateName {
            None,
            MatchConfig,
            RobotPositioning,
            FieldConfig,
            Auto,
            Transition,
            Teleop,
            Endgame,
            MatchResults,
            Restart,
            Reconfigure
        }
    }
}
