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

// adjust the PTL for the script, add robot to a list of gameobjects when its spawned

// using Zippo = System.IO.Compression.Z

public class PTL : MonoBehaviour {

    private string DOZER;
    private string MEAN_MACHINE;
    private string AERIAL_ASSIST;
    private string SYNTHEPARK;
    private string DESTINATION_DEEP_SPACE;
    private string POWER_UP;

    public int robotIndex;

    private GameObject Game;

    private void Start() {
        Game = GameObject.Find("Game");//Finds the parent to spawn objects under
        DOZER = ParsePath("$appdata/Autodesk/Synthesis/Robots/Dozer");
        MEAN_MACHINE = ParsePath("$appdata/Autodesk/Synthesis/Robots/2018 - 2471 Mean Machine");
        AERIAL_ASSIST = ParsePath("$appdata/Autodesk/Synthesis/Fields/2014 Aerial Assist");
        SYNTHEPARK = ParsePath("$appdata/Autodesk/Synthesis/Fields/SynthePark");
        DESTINATION_DEEP_SPACE = ParsePath("$appdata/Autodesk/Synthesis/Fields/2019 Destination Deep Space");
        POWER_UP = ParsePath("$appdata/Autodesk/Synthesis/Fields/2018 Power Up");
        
        SpawnRobot(MEAN_MACHINE);
    }
    public void SpawnField(string fieldPath)
    {
        SpawnField(fieldPath, Vector3.zero, Importer.SourceType.PROTOBUF_FIELD, Translator.TranslationType.BXDF_TO_PROTO_FIELD);
    }
    public void SpawnField(string fieldPath, Vector3 pos, Importer.SourceType srcType, Translator.TranslationType transType = default)
    {
        if (Directory.Exists(fieldPath)) fieldPath = Translator.Translate(fieldPath, transType, ParsePath("$appdata/Autodesk/Synthesis/Fields"));
        var field = Importer.Import(fieldPath, srcType);
        field.transform.parent = Game.transform;
        field.transform.position = pos;        
    }
    public void SpawnRobot(string botPath)
    {
        SpawnRobot(botPath, Vector3.up * 2, Importer.SourceType.PROTOBUF_ROBOT, Translator.TranslationType.BXDJ_TO_PROTO_ROBOT);
    }

    public void SpawnRobot(string botPath, Vector3 pos, Importer.SourceType srcType, Translator.TranslationType transType = default) {
        if (ModelManager.Models.Count() > 5)//limit to 6 models
        {
            ToastManager.Log("Cannot spawn more than 6 robots.");
            return;
        }

        if(Directory.Exists(botPath)) botPath = Translator.Translate(botPath, transType, ParsePath("$appdata/Autodesk/Synthesis/Robots"));
        var robot = Importer.Import(botPath, srcType);

        robot.transform.parent = Game.transform;
        robot.transform.position = pos;
        var dynoMeta = robot.GetComponent<DynamicObjectMeta>();

        var model = new Model();
        model.GameObject = robot;
        foreach (var kvp in dynoMeta.Nodes) {
            if (dynoMeta.HasFlag(kvp.Key, EntityFlag.Wheel)) {
                model.AddMotor(dynoMeta.Nodes[kvp.Key].GetComponent<HingeJoint>());
            }
        }

        if (ModelManager.Models.ContainsKey(dynoMeta.name)) //prevents error from being thrown: FIX LACK OF SUPPORT FOR DUPLICATE ROBOTS
        {
            ToastManager.Log("Duplicate Robot Loaded: Unaccounted for on model dictionary");
            return;
        }

        ModelManager.AddModel(dynoMeta.Name, model);
        SetCameraTransform(ModelManager.Models.Count() - 1);

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
        return b;
    }

    public void SetCameraTransform(int index)
    {
        robotIndex = index;
        Camera.main.GetComponent<CameraController>().FollowTransform = ModelManager.Models.ElementAt(index).Value.GameObject.transform.GetChild(0);
    }

}
