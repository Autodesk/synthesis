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

    private MatchStateMachine _stateMachine;
    public void Start()
    {
        _stateMachine = new MatchStateMachine();
        _stateMachine.CurrentState = MatchStateMachine.MatchState.MatchConfig;
    }

    public void Update() {
    }
    public void End() {
    }

    public void OpenMenu() { }

    public void CloseMenu() { }
    
}
