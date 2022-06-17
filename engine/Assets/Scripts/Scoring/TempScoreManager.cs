using System.Collections.Generic;
using SynthesisAPI.EventBus;
using UnityEngine;

public class TempScoreManager : MonoBehaviour
{
    private List<ScoringZone> ScoringZones;

    private void Start()
    {
        ScoringZones = new List<ScoringZone>();
        CreateScoringZone(1, 1, 1, Alliance.RED, 5);
        CreateTestGamepiece(3, 1, 1, PrimitiveType.Cube);

        EventBus.NewTypeListener<OnScoreEvent>(e =>
        {
            OnScoreEvent scoreEvent = (OnScoreEvent) e;
            Debug.Log($"{scoreEvent.zone.alliance.ToString()} alliance scored {scoreEvent.zone.points} points!");
        });
    }


    public void CreateScoringZone(int x, int y, int z, Alliance alliance, int points, bool destroyObject = true)
    {
        GameObject zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zone.transform.position = new Vector3(x, y, z);
        GizmoManager.SpawnGizmo(GizmoStore.GizmoPrefabStatic, zone.transform, zone.transform.position);
        ScoringZones.Add(new ScoringZone(zone, alliance, points, destroyObject));
    }

    public void CreateTestGamepiece(int x, int y, int z, PrimitiveType type)
    {
        GameObject gamepiece = GameObject.CreatePrimitive(type);
        gamepiece.transform.position = new Vector3(x, y, z);
        gamepiece.name = "Test Gamepiece";
        gamepiece.tag = "gamepiece";
        gamepiece.AddComponent<Rigidbody>();
    }
}