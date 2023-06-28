using System.Collections.Generic;
using Synthesis.UI.Dynamic;
using UnityEngine;
using Synthesis.Physics;

public class MatchStateMachine
{
    private static MatchStateMachine _instance;

    public static MatchStateMachine Instance
    {
        get
        {
            if (_instance != null)
                return _instance;
            
            return new MatchStateMachine();
        }
    }

    #region State Management
    
    private MatchState _currentState;
    private Dictionary<StateName, MatchState> _matchStates = new Dictionary<StateName, MatchState>();

    public void SetState(StateName stateName)
    {
        Debug.Log($"State set to {stateName}");
        var newState = _matchStates[stateName];
        if (newState == null)
        {
            Debug.LogError($"No state found for {stateName}");
            return;
        }

        if (newState == _currentState)
        {
            Debug.LogError($"New state is the same as the current state ({stateName})");
            return;
        }
        
        _currentState.End();
        _currentState = newState;
        Debug.Log(newState.GetType());
        _currentState.Start();
    }
    
    public MatchStateMachine()
    {
        _matchStates.Add(StateName.None, new None());
        _matchStates.Add(StateName.MatchConfig, new MatchConfig());
        _matchStates.Add(StateName.RobotPositioning, new RobotPositioning());
        _matchStates.Add(StateName.Auto, new Auto());
        _matchStates.Add(StateName.Teliop, new Teliop());

        _currentState = _matchStates[StateName.None];
    }

    public void Update()
    {
        _currentState.Update();
    }
    
    #endregion

    #region Match States
    
    public interface MatchState
    {
        public void Start();
        public void Update();
        public void End();
    }
    
    public class None : MatchState
    {
        public void Start() { }
        public void Update() { }
        public void End() { }
    }
    
    public class MatchConfig : MatchState
    {
        public void Start()
        {
            Debug.Log("Match config start");
            DynamicUIManager.CreateModal<MatchModeModal>();
            ((MatchModeModal)DynamicUIManager.ActiveModal).OnAccepted += 
                () => MatchStateMachine.Instance.SetState(StateName.RobotPositioning);
        }
        public void Update() { }
        public void End() { }
    }
    
    public class RobotPositioning : MatchState
    {
        public void Start()
        {
            Debug.Log("Robot positioning start");
            MatchMode.SpawnAllRobots();
            PhysicsManager.IsFrozen = true;
            
            if (Camera.main != null) 
                Camera.main.GetComponent<CameraController>().CameraMode = new FreeCameraMode();
        }
        public void Update() { }
        public void End() { }
    }
    
    public class Auto : MatchState
    {
        public void Start() { }
        public void Update() { }
        public void End() { }
    }
    
    public class Teliop : MatchState
    {
        public void Start() { }
        public void Update() { }
        public void End() { }
    }
    
    #endregion
    
    public enum StateName
    {
        None,
        MatchConfig,
        RobotPositioning,
        Auto,
        Teliop
    }
}