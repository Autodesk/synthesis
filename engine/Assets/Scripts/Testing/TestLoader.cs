using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.Simulator;
using Google.Protobuf.Collections;
using Google.Protobuf;
using System.IO;
using Google.Protobuf.WellKnownTypes;
using System.Threading.Tasks;
using System.Threading;
using Synthesis.Util;

public class TestLoader : MonoBehaviour
{
    public GameObject obj;
    public GameObject loadedInto;
    public GameObject pointObject;
    public Material m;

    private Task t = null;

    public void Start()
    {
        SimulatorHandler.Instance.FieldSpawned += (sender, field) =>
        {
            Debug.Log("'" + field.FieldName + "' has loaded in");
        };
    }

    public void LoadField() {
        SimulatorHandler.Instance.LoadField(ProtobufUtil.GetFieldFromFile("..\\exporter\\SynthesisExporter\\Test_Field.syn"));
    }
}
