using Synthesis.UI.Dynamic;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchMode : IMode {
    public static int currentFieldIndex = -1;
    public static int currentRobotIndex = -1;

    public const string PREVIOUS_SPAWN_LOCATION = "Previous Spawn Location";
    public const string PREVIOUS_SPAWN_ROTATION = "Previous Spawn Rotation";

    public void Start() {
        DynamicUIManager.CreateModal<MatchModeModal>();
    }

    public void Update() {
    }

    public void End() {
    }

    public void OpenMenu() {
    }

    public void CloseMenu() {
    }
}
