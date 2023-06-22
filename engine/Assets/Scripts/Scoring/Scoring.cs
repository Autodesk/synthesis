using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Scoring {
    public static int redScore  = 0;
    public static int blueScore = 0;

    public static void ResetScore() {
        redScore  = 0;
        blueScore = 0;
    }

    public static void CreatePowerupScoreZones() {
        CreateRectangularScorezone(
            new Vector3(0, 1.8f, -1.83f), new Vector3(1, 0.5f, 0.7f), new Vector3(0, 0, 0), Alliance.RED);
        CreateRectangularScorezone(
            new Vector3(0, 1.8f, 1.83f), new Vector3(1, 0.5f, 0.7f), new Vector3(0, 0, 0), Alliance.BLUE);
    }

    private static void CreateRectangularScorezone(Vector3 position, Vector3 scale, Vector3 rotation, Alliance color) {
        GameObject zone                           = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zone.transform.position                   = position;
        zone.transform.localScale                 = scale;
        zone.transform.eulerAngles                = rotation;
        zone.GetComponent<Collider>().isTrigger   = true;
        zone.GetComponent<MeshRenderer>().enabled = false;
        zone.tag                                  = color == Alliance.RED ? "red zone" : "blue zone";
    }
}
