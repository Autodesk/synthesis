using System;
using System.Collections.Generic;
using Analytics;
using Modes.MatchMode;
using Synthesis.UI.Dynamic;
using UnityEngine;
using Synthesis.Physics;
using SynthesisAPI.EventBus;
using UI.Dynamic.Modals;
using Random = UnityEngine.Random;

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

    /// Sets the current state. Automatically calls any event functions in the state
    /// <param name="stateName">The new state to switch to</param>
    public void SetState(StateName stateName) {
        Debug.Log($"State set to {stateName}");
        var newState = _matchStates[stateName];
        if (newState == null) {
            Debug.LogError($"No state found for {stateName}");
            return;
        }

        if (newState == _currentState) {
            Debug.LogError($"New state is the same as the current state ({stateName})");
            return;
        }

        _currentState.End();
        _currentState = newState;
        _currentState.Start();
    }

    /// Manages the state of the match (ex: match config, teleop, match results)
    public MatchStateMachine() {
        _matchStates.Add(StateName.None, new None());
        _matchStates.Add(StateName.MatchConfig, new MatchConfig());
        _matchStates.Add(StateName.RobotPositioning, new RobotPositioning());
        _matchStates.Add(StateName.Auto, new Auto());
        _matchStates.Add(StateName.Teleop, new Teleop());
        _matchStates.Add(StateName.MatchResults, new MatchResults());

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
        private StateName _stateName;

        public MatchState(StateName stateName) {
            this._stateName = stateName;
        }

        public virtual void Start() {
            SynthesisAPI.EventBus.EventBus.Push(new OnStateStarted(this, _stateName));
        }

        public abstract void Update();

        public virtual void End() {
            SynthesisAPI.EventBus.EventBus.Push(new OnStateEnded(this, _stateName));
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
            ((MatchModeModal) DynamicUIManager.ActiveModal).OnAccepted += () =>
                MatchStateMachine.Instance.SetState(StateName.RobotPositioning);
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
                camMode.SetTransform(location,
                    Quaternion.LookRotation(-location.normalized, Vector3.Cross(-location.normalized, Vector3.right)));
            }
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

    /// The autonomous state at the beginning of a match
    public class Auto : MatchState {
        public override void Start() {
            base.Start();
            // TODO: start auto timer on scoreboard
            
            AnalyticsManager.LogCustomEvent(AnalyticsEvent.MatchStarted, ("NumRobots", RobotSimObject.SpawnedRobots.Count));
        }

        public override void Update() {
            // TEMP END CONDITION FOR STATE MACHINE TESTING
            if (Input.GetKeyDown(KeyCode.RightArrow))
                MatchStateMachine.Instance.SetState(StateName.Teleop);
        }

        public override void End() {
            base.End();
        }

        public Auto() : base(StateName.Auto) {}
    }

    /// The teleop state of a match
    public class Teleop : MatchState {
        public override void Start() {
            base.Start();
            // TODO: start teleop timer on scoreboard
        }

        public override void Update() {
            // TEMP END CONDITION FOR STATE MACHINE TESTING
            if (Input.GetKeyDown(KeyCode.RightArrow))
                MatchStateMachine.Instance.SetState(StateName.MatchResults);
        }

        public override void End() {}

        public Teleop() : base(StateName.Teleop) {}
    }

    /// A state when a modal is displayed after a match showing info about the match
    public class MatchResults : MatchState {
        public override void Start() {
            base.Start();

            DynamicUIManager.CreateModal<MatchResultsModal>();
            
            AnalyticsManager.LogCustomEvent(AnalyticsEvent.MatchEnded, 
                ("BluePoints", MatchMode.MatchResultsTracker.MatchResultEntries[
                        typeof(MatchResultsTracker.BluePoints)].ToString()),
                ("RedPoints", MatchMode.MatchResultsTracker.MatchResultEntries[
                        typeof(MatchResultsTracker.RedPoints)].ToString()));
        }

        public override void Update() {}

        public override void End() {}

        public MatchResults() : base(StateName.MatchResults) {}
    }

#endregion

    /// Represents a specific MatchState
    public enum StateName {
        None,
        MatchConfig,
        RobotPositioning,
        Auto,
        Teleop,
        MatchResults
    }
}