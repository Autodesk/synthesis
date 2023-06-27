using Synthesis.UI.Dynamic;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using TMPro;

public class MatchMode : IMode {
    public static int CurrentFieldIndex = -1;
    public static int[] SelectedRobots = new int[6];

    public static Vector3[] RobotSpawnLocations = new Vector3[6];

    public const string PREVIOUS_SPAWN_LOCATION = "Previous Spawn Location";
    public const string PREVIOUS_SPAWN_ROTATION = "Previous Spawn Rotation";

    private MatchStateMachine _stateMachine;
    public void Start()
    {
        SelectedRobots.ForEach(x => x = -1);

        _stateMachine = new MatchStateMachine();
        _stateMachine.CurrentState = MatchStateMachine.MatchState.MatchConfig;
    }

    public void Update() {
    }
    public void End() {
    }

    public void OpenMenu() { }

    public void CloseMenu() { }

    public static void SpawnAllRobots()
    {
        var robotsFolder = ParsePath("$appdata/Autodesk/Synthesis/Mira", '/');
        if (!Directory.Exists(robotsFolder))
            Directory.CreateDirectory(robotsFolder);
        var robotFiles = Directory.GetFiles(robotsFolder).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();

        MatchMode.SelectedRobots.ForEach(x =>
        {
            Debug.Log($"Attempting to spawn {x}");
            if (x != -1)
            {
                Debug.Log($"Spawned {x}");
                RobotSimObject.SpawnRobot(robotFiles[x]);
            }
        });
    }
    
    public static string ParsePath(string p, char c)
    {
        string[] a = p.Split(c);
        string b = "";
        for (int i = 0; i < a.Length; i++)
        {
            switch (a[i])
            {
                case "$appdata":
                    b += System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                    break;
                default:
                    b += a[i];
                    break;
            }
            if (i != a.Length - 1)
                b += System.IO.Path.AltDirectorySeparatorChar;
        }
        return b;
    }
}
