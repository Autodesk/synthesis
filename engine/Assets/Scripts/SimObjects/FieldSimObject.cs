using System.Collections;
using System.Collections.Generic;
using Mirabuf;
using Synthesis.Import;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

using Bounds = UnityEngine.Bounds;

public class FieldSimObject : SimObject {

    public static FieldSimObject CurrentField { get; private set; }

    public Assembly MiraAssembly { get; private set; }
    public GameObject GroundedNode { get; private set; }
    public GameObject FieldObject { get; private set; }
    public Bounds FieldBounds { get; private set; }
    public List<GamepieceSimObject> Gamepieces { get; private set; }
    
    public List<ScoringZone> ScoringZones { get; private set; }

    public FieldSimObject(string name, ControllableState state, Assembly assembly, GameObject groundedNode, List<GamepieceSimObject> gamepieces) : base(name, state) {
        MiraAssembly = assembly;
        GroundedNode = groundedNode;
        FieldObject = groundedNode.transform.parent.gameObject;
        FieldBounds = groundedNode.transform.GetBounds();
        Gamepieces = gamepieces;
        ScoringZones = new List<ScoringZone>();

        // Level the field
        var position = FieldObject.transform.position;
        position.y -= FieldBounds.center.y - FieldBounds.extents.y;
        FieldObject.transform.position = position;

        CurrentField = this;
        // SynthesisAssetCollection.DefaultFloor.SetActive(false);
    }

    public void DeleteField() {
        GameObject.Destroy(FieldObject);
        SimulationManager.RemoveSimObject(this);
        // SynthesisAssetCollection.DefaultFloor.SetActive(true);
    }

    public static void SpawnField(string filePath) {
        var mira = Importer.MirabufAssemblyImport(filePath);
        mira.MainObject.transform.SetParent(GameObject.Find("Game").transform);
    }

    public void CreateScoringZone(Alliance alliance, int points, bool destroyObject = true)
    {
        GameObject zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ScoringZones.Add(new ScoringZone(zone, alliance, points, destroyObject));
    }

    public void CreateTestGamepiece(PrimitiveType type)
    {
        GameObject gamepiece = GameObject.CreatePrimitive(type);
        gamepiece.tag = "gamepiece";
        gamepiece.AddComponent<Rigidbody>();
    }
}
