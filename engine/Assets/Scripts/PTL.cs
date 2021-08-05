using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.Import;
using System.IO;
using System;
using Synthesis.Entitys;
using Synthesis.ModelManager;
using Synthesis.ModelManager.Models;
using SynthesisAPI.Translation;
using System.Linq;
using Mirabuf;
using Vector3 = UnityEngine.Vector3;

// adjust the PTL for the script, add robot to a list of gameobjects when its spawned

// using Zippo = System.IO.Compression.Z

public class PTL : MonoBehaviour {

    private string DOZER;
    private string MEAN_MACHINE;
    private string AERIAL_ASSIST;
    private string SYNTHEPARK;
    private string DESTINATION_DEEP_SPACE;
    private string POWER_UP;
    private List<GameObject> robotList;
    private Boolean hasRobot;
    private GameObject Game;

    private void Start() {
        Game = GameObject.Find("Game");//Finds the parent to spawn objects under
        DOZER = ParsePath("$appdata/Autodesk/Synthesis/Robots/Dozer");
        MEAN_MACHINE = ParsePath("$appdata/Autodesk/Synthesis/Robots/2018 - 2471 Mean Machine");
        AERIAL_ASSIST = ParsePath("$appdata/Autodesk/Synthesis/Fields/2014 Aerial Assist");
        SYNTHEPARK = ParsePath("$appdata/Autodesk/Synthesis/Fields/SynthePark");
        DESTINATION_DEEP_SPACE = ParsePath("$appdata/Autodesk/Synthesis/Fields/2019 Destination Deep Space");
        POWER_UP = ParsePath("$appdata/Autodesk/Synthesis/Fields/2018 Power Up");
        var MIRA_TEST = ParsePath("$appdata/Autodesk/Synthesis/Mira/MotionDefinition_v72.mira");
        
        hasRobot = false;
        robotList = new List<GameObject>();
        // SpawnRobot(MEAN_MACHINE);

        Importer.AssemblyImport(Assembly.Parser.ParseFrom(File.ReadAllBytes(MIRA_TEST)));

        // var field = Importer.Import(POWER_UP, Importer.SourceType.PROTOBUF_FIELD,
        //     Translator.TranslationType.BXDF_TO_PROTO_FIELD, true);
        // var position = field.transform.position;
        // position = new Vector3(position.x, position.y + 0.5f, position.z);
        // field.transform.position = position;
    }
    public void SpawnField(string fieldPath)
    {
        // SpawnField(fieldPath, Vector3.zero, Importer.SourceType.PROTOBUF_FIELD, Translator.TranslationType.BXDF_TO_PROTO_FIELD);
    }
    public void SpawnField(string fieldPath, Vector3 pos, Importer.SourceType srcType, Translator.TranslationType transType = default)
    {
        // if (Directory.Exists(fieldPath)) fieldPath = Translator.Translate(fieldPath, transType, ParsePath("$appdata/Autodesk/Synthesis/Fields"));
        // var field = Importer.Import(fieldPath, srcType);
        // field.transform.parent = Game.transform;
        // field.transform.position = pos;        
    }
    public void SpawnRobot(string botPath)//overloaded
    {
        // SpawnRobot(botPath, Vector3.up * 2, Importer.SourceType.PROTOBUF_ROBOT, Translator.TranslationType.BXDJ_TO_PROTO_ROBOT);
    }

    /*
    public void RemoveRobot(String botPath)
    {
        RemoveRobot(botPath, Vector3.up * 2, Importer.SourceType.PROTOBUF_ROBOT, Translator.TranslationType.BXDJ_TO_PROTO_ROBOT);
    }
    */

    public void SpawnRobot(string botPath, Vector3 pos, Importer.SourceType srcType, Translator.TranslationType transType = default) {
        // if(Directory.Exists(botPath)) botPath = Translator.Translate(botPath, transType, ParsePath("$appdata/Autodesk/Synthesis/Robots"));
        // var robot = Importer.Import(botPath, srcType);
        //
        // robot.transform.parent = Game.transform;
        // robot.transform.position = pos;
        // var dynoMeta = robot.GetComponent<DynamicObjectMeta>();
        //
        // var model = new Model();
        // model.GameObject = robot;
        // foreach (var kvp in dynoMeta.Nodes) {
        //     if (dynoMeta.HasFlag(kvp.Key, EntityFlag.Wheel)) {
        //         model.AddMotor(dynoMeta.Nodes[kvp.Key].GetComponent<HingeJoint>());
        //     }
        // }
        // ModelManager.AddModel(dynoMeta.Name, model);
        //
        // Camera.main.GetComponent<CameraController>().FollowTransform = robot.transform.GetChild(0);
        // robotList.Add(robot);
    }
    /*
    public void RemoveRobot(int index)
    {
        ModelManager.Remove(j[index]);
        // just use ModelManager to destroy
    }*/


    public void Update() {
        // if (Input.GetKeyDown(KeyCode.Alpha1)) {
        //     SpawnRobot(MEAN_MACHINE, new Vector3(0, 10, 0), Importer.SourceType.PROTOBUF_ROBOT, Translator.TranslationType.BXDJ_TO_PROTO_ROBOT);
        // } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
        //     SpawnRobot(DOZER, new Vector3(3, 10, 0), Importer.SourceType.PROTOBUF_ROBOT, Translator.TranslationType.BXDJ_TO_PROTO_ROBOT);
        // }
    }

    private string ParsePath(string p) {
        string[] a = p.Split('/');
        string b = "";
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
        // Debug.Log(b);
        return b;
    }

    public GameObject getRobotAtIndex(int index)
    {
        if (robotList.Count > index)
        {
            return robotList.ElementAt(index);
        }
        else
        {
            hasRobot = false;
            return null;
        }
    }

    public Boolean hasRobotAtPosition(int index)
    {
        if (robotList.Count > index)
        {
            hasRobot = true;
            return hasRobot;
        }
        else
        {
            hasRobot = false;
            return hasRobot;
        }
    }

    public void fixTransformPosition(int index)
    {
        Camera.main.GetComponent<CameraController>().FollowTransform = robotList.ElementAt(index).transform.GetChild(0);
    }
}
