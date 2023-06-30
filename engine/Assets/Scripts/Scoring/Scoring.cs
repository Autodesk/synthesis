using System.Collections.Generic;
using UnityEngine;

public static class Scoring {
    public static int redScore     = 0;
    public static int blueScore    = 0;
    public static float targetTime = 135;
    public static bool matchEnd    = false;

    public static void ResetScore() {
        redScore   = 0;
        blueScore  = 0;
        targetTime = 135;
        matchEnd   = false;
    }

    public static List<GameObject> CreatePowerupScoreZones() {
        List<GameObject> gameObjects = new List<GameObject>();
        gameObjects.Add(CreateRectangularScorezone(
            new Vector3(0, 1.8f, -1.83f), new Vector3(1, 0.5f, 0.7f), new Vector3(0, 0, 0), Alliance.Red));
        gameObjects.Add(CreateRectangularScorezone(
            new Vector3(0, 1.8f, 1.83f), new Vector3(1, 0.5f, 0.7f), new Vector3(0, 0, 0), Alliance.Blue));
        return gameObjects;
    }

    private static GameObject CreateRectangularScorezone(
        Vector3 position, Vector3 scale, Vector3 rotation, Alliance color) {
        GameObject zone                           = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zone.transform.position                   = position;
        zone.transform.localScale                 = scale;
        zone.transform.eulerAngles                = rotation;
        zone.GetComponent<Collider>().isTrigger   = true;
        zone.GetComponent<MeshRenderer>().enabled = false;
        zone.tag                                  = color == Alliance.Red ? "red zone" : "blue zone";

        return zone;
    }
}