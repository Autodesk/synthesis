using System;
using Synthesis.UI.Dynamic;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Synthesis.Physics;
using UnityEngine;
using TMPro;

public class MatchMode : IMode {
    public static int CurrentFieldIndex = -1;
    public static int[] SelectedRobots = new int[6];
    
    /// Whether or not the robot should snap to a grid in positioning mode
    public static bool[] RoundSpawnLocation = new bool[6];
    
    /// The un-rounded spawn position of all robots
    public static (Vector3 position, Quaternion rotation)[] RawSpawnLocations = 
        new (Vector3 position, Quaternion rotation)[6];

    /// Rounds the robots spawn position if the user chooses to
    public static (Vector3 position, Quaternion rotation) GetSpawnLocation(int robot)
    {
        if (RoundSpawnLocation[robot])
            return SpawnLocationPanel.RoundSpawnLocation(RawSpawnLocations[robot]);

        return RawSpawnLocations[robot];
    }

    public static List<RobotSimObject> Robots = new List<RobotSimObject>();

    public const string PREVIOUS_SPAWN_LOCATION = "Previous Spawn Location";
    public const string PREVIOUS_SPAWN_ROTATION = "Previous Spawn Rotation";

    private MatchStateMachine _stateMachine;
    public void Start()
    {
        Array.Fill(SelectedRobots, -1);
        Array.Fill(RawSpawnLocations, (Vector3.zero, Quaternion.identity));

        _stateMachine = MatchStateMachine.Instance;
        _stateMachine.SetState(MatchStateMachine.StateName.MatchConfig);
    }

    public void Update() {
        if (_stateMachine != null)
            _stateMachine.Update();
    }
    public void End() {
    }

    public void OpenMenu() { }

    public void CloseMenu() { }

    /// Spawns in all of the selected robots and disables physics for spawn location selection
    public static void SpawnAllRobots()
    {
        var robotsFolder = ParsePath("$appdata/Autodesk/Synthesis/Mira", '/');
        if (!Directory.Exists(robotsFolder))
            Directory.CreateDirectory(robotsFolder);
        var robotFiles = Directory.GetFiles(robotsFolder).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();

        int i = 0;
        SelectedRobots.ForEach(x =>
        {
            if (x != -1)
            {
                Vector3 position = new Vector3(2 * i - 6, -2.5f, 0);
                RawSpawnLocations[i].position = position;
                
                RobotSimObject.SpawnRobot(robotFiles[x], position, Quaternion.identity, false);
                Robots.Add(RobotSimObject.GetCurrentlyPossessedRobot());
            }
            else 
                Robots.Add(null);
            i++;
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
