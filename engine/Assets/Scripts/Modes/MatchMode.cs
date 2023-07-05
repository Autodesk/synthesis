using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Synthesis.UI.Dynamic;
using SynthesisAPI.EventBus;
using SynthesisAPI.Utilities;
using UnityEngine;
using Logger = SynthesisAPI.Utilities.Logger;

public class MatchMode : IMode {
    public static int CurrentFieldIndex = -1;
    public static int[] SelectedRobots  = new int[6];

    /// Whether or not the robot should snap to a grid in positioning mode
    public static bool[] RoundSpawnLocation = new bool[6];

    /// The un-rounded spawn position of all robots
    public static (Vector3 position, Quaternion rotation)[] RawSpawnLocations = new(
        Vector3 position, Quaternion rotation)[6];

    /// Rounds the robots spawn position if the user chooses to
    public static (Vector3 position, Quaternion rotation) GetSpawnLocation(int robot) {
        if (RoundSpawnLocation[robot])
            return SpawnLocationPanel.RoundSpawnLocation(RawSpawnLocations[robot]);

        return RawSpawnLocations[robot];
    }

    public static List<RobotSimObject> Robots = new List<RobotSimObject>();

    private int _redScore  = 0;
    private int _blueScore = 0;

    private bool _showingScoreboard = false;

    public const string PREVIOUS_SPAWN_LOCATION = "Previous Spawn Location";
    public const string PREVIOUS_SPAWN_ROTATION = "Previous Spawn Rotation";

    private MatchStateMachine _stateMachine;

    // Start is called before the first frame update
    public void Start() {
        DynamicUIManager.CreateModal<MatchModeModal>();
        EventBus.NewTypeListener<OnScoreUpdateEvent>(HandleScoreEvent);
<<<<<<< HEAD
<<<<<<< HEAD

        EventBus.NewTypeListener<MatchStateMachine.OnStateStarted>(e => {
            MatchStateMachine.OnStateStarted onStateStarted = (MatchStateMachine.OnStateStarted) e;
            switch (onStateStarted.state.StateName) {
                case MatchStateMachine.StateName.Auto:
                    Scoring.targetTime = 15;
                    DynamicUIManager.CreatePanel<ScoreboardPanel>(true, true);
                    break;
                case MatchStateMachine.StateName.Transition:
                    Scoring.targetTime = 135;
                    break;
            }
        });
=======
>>>>>>> e7c59c915 (moved timer events into state start methods)

<<<<<<< HEAD
        MainHUD.AddItemToDrawer("Settings", b => DynamicUIManager.CreateModal<SettingsModal>(),
            icon: SynthesisAssetCollection.GetSpriteByName("settings"));
=======

        MainHUD.AddItemToDrawer("Settings", b => DynamicUIManager.CreateModal<SettingsModal>(),
            icon: SynthesisAssetCollection.GetSpriteByName("settings"));
>>>>>>> origin/feature/1553/match-state-flow
=======
        MainHUD.AddItemToDrawer("Settings", b => DynamicUIManager.CreateModal<SettingsModal>(),
            icon: SynthesisAssetCollection.GetSpriteByName("settings"));
>>>>>>> 95f4ffd7f (ran formatter)

        Array.Fill(SelectedRobots, -1);
        Array.Fill(RawSpawnLocations, (Vector3.zero, Quaternion.identity));

        _stateMachine = MatchStateMachine.Instance;
        _stateMachine.SetState(MatchStateMachine.StateName.MatchConfig);
    }
<<<<<<< HEAD
<<<<<<< HEAD
    
=======

>>>>>>> 95f4ffd7f (ran formatter)
    private void HandleScoreEvent(IEvent e) {
        if (e.GetType() != typeof(OnScoreUpdateEvent))
            return;
        OnScoreUpdateEvent scoreUpdateEvent = e as OnScoreUpdateEvent;
        if (scoreUpdateEvent == null)
            return;

        ScoringZone zone = scoreUpdateEvent.Zone;
<<<<<<< HEAD
        int points       = zone.Points * (scoreUpdateEvent.IncreaseScore ? 1 : -1);

        switch (zone.Alliance) {
=======

        private void HandleScoreEvent(IEvent e) {
            if (e.GetType() != typeof(OnScoreUpdateEvent))
                return;
            OnScoreUpdateEvent scoreUpdateEvent = e as OnScoreUpdateEvent;
            if (scoreUpdateEvent == null)
                return;

            ScoringZone zone = scoreUpdateEvent.Zone;
            int points       = zone.Points * (scoreUpdateEvent.IncreaseScore ? 1 : -1);

            switch (zone.Alliance) {
>>>>>>> origin/feature/1553/match-state-flow
=======

    int points = zone.Points * (scoreUpdateEvent.IncreaseScore ? 1 : -1);

    switch (zone.Alliance) {
>>>>>>> 95f4ffd7f (ran formatter)
            case Alliance.Blue:
                Scoring.blueScore += points;
                break;
            case Alliance.Red:
                Scoring.redScore += points;
                break;
        }
    }
<<<<<<< HEAD
<<<<<<< HEAD
    
=======

>>>>>>> 95f4ffd7f (ran formatter)

    public void Update() {
        if (_stateMachine != null) {
            _stateMachine.Update();

            if (Scoring.targetTime <= 0 && _stateMachine.CurrentState.StateName is >=
                                               MatchStateMachine.StateName.Auto and <=
                                               MatchStateMachine.StateName.Teleop)
                _stateMachine.AdvanceState();
        }
    }
    public void End() {
=======

public void Update() {
    if (_stateMachine != null) {
        _stateMachine.Update();

        if (Scoring.targetTime <= 0 && _stateMachine.CurrentState.StateName is >=
                                           MatchStateMachine.StateName.Auto and <= MatchStateMachine.StateName.Teleop)
            _stateMachine.AdvanceState();
    }
>>>>>>> origin/feature/1553/match-state-flow
    }

    public void OpenMenu() {}

    public void CloseMenu() {}

    /// Spawns in all of the selected robots and disables physics for spawn location selection
    public static void SpawnAllRobots() {
        var robotsFolder = ParsePath("$appdata/Autodesk/Synthesis/Mira", '/');
        if (!Directory.Exists(robotsFolder))
            Directory.CreateDirectory(robotsFolder);
        var robotFiles = Directory.GetFiles(robotsFolder).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();

        int i = 0;
        SelectedRobots.ForEach(x => {
            if (x != -1) {
                Vector3 position              = new Vector3(2 * i - 6, -2.5f, 0);
                RawSpawnLocations[i].position = position;

                RobotSimObject.SpawnRobot(robotFiles[x], position, Quaternion.identity, false);
                Robots.Add(RobotSimObject.GetCurrentlyPossessedRobot());
            } else
                Robots.Add(null);
            i++;
        });
    }

    public static string ParsePath(string p, char c) {
        string[] a = p.Split(c);
        string b   = "";
        for (int i = 0; i < a.Length; i++) {
            switch (a[i]) {
                case "$appdata":
                    b += Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    break;
                default:
                    b += a[i];
                    break;
            }
            if (i != a.Length - 1)
                b += Path.AltDirectorySeparatorChar;
        }
        return b;
    }
}
