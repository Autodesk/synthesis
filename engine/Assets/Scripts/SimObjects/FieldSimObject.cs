using System.Collections;
using System.Collections.Generic;
using Mirabuf;
using Synthesis.Import;
using SynthesisAPI.Simulation;
using SynthesisAPI.Utilities;
using UnityEngine;

using Bounds = UnityEngine.Bounds;
using Transform = Mirabuf.Transform;
using Vector3 = UnityEngine.Vector3;

public class FieldSimObject : SimObject {

    public static FieldSimObject CurrentField { get; private set; }

    public Assembly MiraAssembly { get; private set; }
    public GameObject GroundedNode { get; private set; }
    public GameObject FieldObject { get; private set; }
    public Bounds FieldBounds { get; private set; }
    public List<GamepieceSimObject> Gamepieces { get; private set; }
    
    public List<ScoringZone> ScoringZones { get; private set; }

    private Vector3 _initialPosition;
    private Quaternion _initialRotation;

    public FieldSimObject(string name, ControllableState state, Assembly assembly, GameObject groundedNode, List<GamepieceSimObject> gamepieces) : base(name, state) {
        MiraAssembly = assembly;
        GroundedNode = groundedNode;
        // grounded node is what gets grabbed in god mode so it needs field tag to not get moved
        GroundedNode.transform.tag = "field";
        FieldObject = groundedNode.transform.parent.gameObject;
        FieldBounds = FieldObject.transform.GetBounds();
        Gamepieces = gamepieces;
        ScoringZones = new List<ScoringZone>();

        // Level the field
        var position = FieldObject.transform.position;
        position.y -= position.y - FieldBounds.extents.y;
        
        _initialPosition = GroundedNode.transform.position;
        _initialRotation = GroundedNode.transform.rotation;

        CurrentField = this;
        Gamepieces.ForEach(gp =>
        {
            UnityEngine.Transform gpTransform = gp.GamepieceObject.transform;
            gp.InitialPosition = gpTransform.position;
            gp.InitialRotation = gpTransform.rotation;
        });
    }

    public void ResetField()
    {
        GroundedNode.transform.position = _initialPosition;
        GroundedNode.transform.rotation = _initialRotation;
    }

    public void DeleteField() {
        GameObject.Destroy(FieldObject);
        
        foreach (var gamepiece in Gamepieces)
        {
            SimulationManager.RemoveSimObject(gamepiece.Name);
        }
        
        SimulationManager.RemoveSimObject(Name);
        
        Gamepieces.Clear();
        ScoringZones.Clear();
    }

    public static void SpawnField(string filePath) { 
        if (CurrentField != null)
        {
            CurrentField.DeleteField();
            CurrentField = null;
        }
        var mira = Importer.MirabufAssemblyImport(filePath);
        mira.MainObject.transform.SetParent(GameObject.Find("Game").transform);
        mira.MainObject.tag = "field";
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
