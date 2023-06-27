using Synthesis.UI.Dynamic;
using Synthesis.Util;
using UnityEngine;
using System.IO;
using System.Linq;

public class MatchStateMachine {
    private MatchState _currentState;

    public MatchState CurrentState
    {
        get
        {
            return _currentState;
        }
        set 
        {
            Debug.Log($"State set to {value}");
            if (value == _currentState)
            {
                Debug.LogError($"New state is the same as the current state ({value})");
                return; 
            }
            
            _currentState = value;
            switch (value)
            {
                case MatchState.MatchConfig:
                    DynamicUIManager.CreateModal<MatchModeModal>();
                    ((MatchModeModal)DynamicUIManager.ActiveModal).OnAccepted += () => CurrentState = MatchState.RobotPositioning;
                    return; 
                case MatchState.RobotPositioning:
                    //MatchMode.SpawnAllRobots();
                    return;
                case MatchState.Auto: 
                    return;
                default:
                    Debug.LogError($"No behavior for current state ({value})");
                    return;
            }
        }
    }
    

    public enum MatchState
    {
        None,
        MatchConfig,
        RobotPositioning,
        Auto
    }
}