using Synthesis.UI.Dynamic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MatchMode : IMode {
    public static int currentFieldIndex = -1;
    public static int currentRobotIndex = -1;

    public const string PREVIOUS_SPAWN_LOCATION = "Previous Spawn Location";
    public const string PREVIOUS_SPAWN_ROTATION = "Previous Spawn Rotation";
    
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
                    return;
                case MatchState.RobotPositioning:
                    return;
                case MatchState.Auto:
                    return;
                default:
                    Debug.LogError($"No behavior for current state ({value})");
                    return;
            }
        }
    }

    // Start is called before the first frame update
    public void Start()
    {
        CurrentState = MatchState.MatchConfig;
    }

    // Update is called once per frame
    public void Update() {
    }
    public void End() {
    }

    public void OpenMenu() { }

    public void CloseMenu() { }

    public enum MatchState
    {
        None,
        MatchConfig,
        RobotPositioning,
        Auto
        
    }
}
