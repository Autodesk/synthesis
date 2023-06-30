using System.Collections;
using System.Collections.Generic;
using Synthesis.Gizmo;
using Synthesis.PreferenceManager;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class StartMatchModePanel : PanelDynamic {

    public StartMatchModePanel() : base(new Vector2(300f, 200f)) { }

    public override bool Create() {

        Title.SetText("Start Match?").SetFontSize(25f);

        AcceptButton
            .StepIntoLabel(label => label.SetText("Start"))
            .AddOnClickedEvent(b => {
                StartMatch();
            });
        CancelButton
            .StepIntoLabel(label => label.SetText("Cancel"))
            .AddOnClickedEvent(b => {
                DynamicUIManager.CreateModal<MatchModeModal>();
            });
        
        return true;
    }

    private void StartMatch() {
        if (RobotSimObject.CurrentlyPossessedRobot != string.Empty) {
            Vector3 p = RobotSimObject.GetCurrentlyPossessedRobot().RobotNode.transform.position;
            PreferenceManager.SetPreference(MatchMode.PREVIOUS_SPAWN_LOCATION, new float[] { p.x, p.y, p.z});
            Quaternion q = RobotSimObject.GetCurrentlyPossessedRobot().RobotNode.transform.rotation;
            PreferenceManager.SetPreference(MatchMode.PREVIOUS_SPAWN_ROTATION, new float[] { q.x, q.y, q.z, q.w });
            PreferenceManager.Save();
        }
        
        // Shooting.ConfigureGamepieces();
        
        //TEMPORARY: FOR POWERUP ONLY
        
        // Scoring.CreatePowerupScoreZones();
        DynamicUIManager.CloseAllPanels(true);
        DynamicUIManager.CreatePanel<ScoreboardPanel>(true);

        GizmoManager.ExitGizmo();
    }

    public override void Delete() { }

    public override void Update() { }
}
