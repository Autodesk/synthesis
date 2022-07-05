using System.Collections.Generic;
using SynthesisAPI.EventBus;
using UnityEngine;

public class TempScoreManager : MonoBehaviour
{
    private bool _RenderScoringZones = true;

    public bool RenderScoringZones
    {
        get => _RenderScoringZones;
        set
        {
            if (_RenderScoringZones != value)
            {
                _RenderScoringZones = value;
                ScoringZones.ForEach(zone =>
                {
                    zone.SetVisibility(value);
                });
            }
        }
    }
    private List<ScoringZone> ScoringZones;
    public static int redScore = 0;
    public static int blueScore = 0;

    private void Start()
    {
        ScoringZones = new List<ScoringZone>();
        GameObject parent = new GameObject();
        parent.name = "Gamepiece Parent";
        List<GameObject> zones = new List<GameObject>(
        );
        for (int i = -5; i < 5; i++)
        {
            GameObject newZone = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            newZone.transform.position = new Vector3(i, 1, 0);
            zones.Add(newZone);
            newZone.transform.parent = parent.transform;
        }
        CreateScoringZonesFromParent(parent, Alliance.RED, 1);
        CreateTestGamepiece(3, 1, 2, PrimitiveType.Cube);
        
        EventBus.NewTypeListener<OnScoreEvent>(e =>
        {
            OnScoreEvent se = (OnScoreEvent) e;
            switch (se.zone.alliance)
            {
                case Alliance.RED:
                    redScore += se.zone.points;
                    break;
                case Alliance.BLUE:
                    blueScore += se.zone.points;
                    break;
            }
            Debug.Log($"{se.zone.alliance.ToString()} alliance scored {se.zone.points} points!");
        });
    }

    public void CreateScoringZones(List<GameObject> gameObjects, Alliance alliance, int points,
        bool destroyObject = true)
    {
        gameObjects.ForEach(obj =>
        {
            ScoringZones.Add(new ScoringZone(obj, alliance, points, destroyObject));
        });
    }

    public void CreateScoringZonesFromParent(GameObject parent, Alliance alliance, int points,
        bool destroyObject = true)
    {
        List<GameObject> children = parent.GetComponentsInChildren<Collider>().Map(c => c.gameObject);
        CreateScoringZones(children, alliance, points, destroyObject);
    }


    public void CreateScoringZoneManual(int x, int y, int z, Alliance alliance, int points, bool destroyObject = true)
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