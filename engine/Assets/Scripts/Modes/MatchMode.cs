using Synthesis.UI.Dynamic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MatchMode : IMode {
    //PracticeMode practiceMode = new PracticeMode();
    public static int currentFieldIndex = -1;
    public static int currentRobotIndex = -1;

    private int _redScore = 0;
    private int _blueScore = 0;

    public const string PREVIOUS_SPAWN_LOCATION = "Previous Spawn Location";
    public const string PREVIOUS_SPAWN_ROTATION = "Previous Spawn Rotation";

    // Start is called before the first frame update
    public void Start() {
        DynamicUIManager.CreateModal<MatchModeModal>();
        SynthesisAPI.EventBus.EventBus.NewTypeListener<OnScoreEvent>(
            e =>
            {
                ScoringZone zone = ((OnScoreEvent)e).zone;
                switch (zone.Alliance)
                {
                    case Alliance.Blue:
                        _blueScore += zone.Points;
                        break;
                    case Alliance.Red:
                        _redScore += zone.Points;
                        break;
                }
                Debug.Log($"{zone.Alliance.ToString()} scored {zone.Points} points! Blue: {_blueScore} Red: {_redScore}");
            });
        
        MainHUD.AddItemToDrawer("Scoring Zones", b => DynamicUIManager.CreatePanel<ScoringZonesPanel>());
    }

    // Update is called once per frame
    public void Update() {
    }
    public void End() {
    }

    public void OpenMenu() { }

    public void CloseMenu() { }

}
