using System;
using System.Collections.Generic;
using Synthesis.UI.Dynamic;
using UnityEngine;
using Synthesis.Physics;
using SynthesisAPI.EventBus;
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

    public MatchStateMachine() {
        _matchStates.Add(StateName.None, new None());
        _matchStates.Add(StateName.MatchConfig, new MatchConfig());
        _matchStates.Add(StateName.RobotPositioning, new RobotPositioning());
        _matchStates.Add(StateName.Auto, new Auto());
        _matchStates.Add(StateName.Teleop, new Teleop());
        _matchStates.Add(StateName.MatchResults, new MatachResults());

        _currentState = _matchStates[StateName.None];
    }

    public void Update() {
        _currentState.Update();
    }

    #endregion

    #region Match States

    public class OnStateStarted : IEvent {
        public MatchState state;
        public StateName stateName;

        public OnStateStarted(MatchState state, StateName stateName) {
            this.state = state;
            this.stateName = stateName;
        }
    }

    public class OnStateEnded : IEvent {
        public MatchState state;
        public StateName stateName;

        public OnStateEnded(MatchState state, StateName stateName) {
            this.state = state;
            this.stateName = stateName;
        }
    }

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

    public class None : MatchState {
        public override void Start() {
            base.Start();
        }

        public override void Update() { }

        public override void End() {
            base.End();
        }

        public None() : base(StateName.None) { }
    }

    public class MatchConfig : MatchState {
        public override void Start() {
            base.Start();
            DynamicUIManager.CreateModal<MatchModeModal>();
            ((MatchModeModal)DynamicUIManager.ActiveModal).OnAccepted +=
                () => MatchStateMachine.Instance.SetState(StateName.RobotPositioning);
        }

        public override void Update() { }

        public override void End() {
            base.End();
        }

        public MatchConfig() : base(StateName.MatchConfig) { }
    }

    public class RobotPositioning : MatchState {
        public override void Start() {
            base.Start();
            
            MatchMode.SpawnAllRobots();
            PhysicsManager.IsFrozen = true;

            if (Camera.main != null) {
                Camera.main.GetComponent<CameraController>().CameraMode = CameraController.CameraModes["Freecam"];
            }
        }

        public override void Update() { }

        public override void End() {
            base.End();
            
            PhysicsManager.IsFrozen = false;

            if (Camera.main != null) {
                Camera.main.GetComponent<CameraController>().CameraMode = CameraController.CameraModes["Orbit"];
            }
        }

        public RobotPositioning() : base(StateName.RobotPositioning) { }
    }

    public class Auto : MatchState {
        public override void Start() {
            base.Start();
            
            // TODO: start auto timer on scoreboard
        }

        public override void Update() {
            // TEMP END CONDITION FOR STATE MACHINE TESTING
            if (Input.GetKeyDown(KeyCode.RightArrow))
                MatchStateMachine.Instance.SetState(StateName.Teleop);
        }

        public override void End() {
            base.End();
        }

        public Auto() : base(StateName.Auto) { }
    }

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
        public override void End() { }

        public Teleop() : base(StateName.Teleop) { }
    }
    
    public class MatachResults : MatchState {
        public override void Start() {
            base.Start();

            DynamicUIManager.CreateModal<MatchResultsModal>();
        }

        public override void Update() { }
        public override void End() { }

        public MatachResults() : base(StateName.MatchResults) { }
    }

    #endregion

    public enum StateName {
        None,
        MatchConfig,
        RobotPositioning,
        Auto,
        Teleop,
        MatchResults
    }
}