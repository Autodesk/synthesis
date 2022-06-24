using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModeManager
{
    public static Vector3 GamepieceSpawnpoint = new Vector3(0, 1, 0);
    public static Vector3 RobotSpawnpoint = new Vector3(0, 1, 0);
    public static Quaternion RobotSpawnRotation = Quaternion.identity;
    
    private static List<GamepieceSimObject> _gamepieces = new List<GamepieceSimObject>();
    
    public static void ResetRobot()
    {
        GameObject.Find("Game")
            .GetComponentsInChildren<Rigidbody>().ForEach(rb =>
            {
                GameObject obj = rb.gameObject;
                if (obj.CompareTag("robot"))
                {
                    obj.transform.position = RobotSpawnpoint;
                    obj.transform.rotation = RobotSpawnRotation;
                }
            });
    }

    public static void ResetField()
    {
        ResetGamepieces();
    }
    
    public static void ResetGamepieces()
    {
        _gamepieces.ForEach(gp =>
        {
            GameObject.Destroy(gp.GamepieceObject);
        });
        _gamepieces.Clear();
        FieldSimObject.CurrentField.Gamepieces.ForEach(gp =>
        {
            gp.Reset();
        });
    }

    public static void SpawnGamepiece(float scale = 1.0f, PrimitiveType type = PrimitiveType.Sphere)
    {
        SpawnGamepiece(GamepieceSpawnpoint, scale, type);
    }

    public static void SpawnGamepiece(int x, int y, int z, float scale = 1.0f, PrimitiveType type = PrimitiveType.Sphere)
    {
        SpawnGamepiece(new Vector3(x, y, z), scale, type);
    }

    // will change when we have gamepieces from mirabuf
    public static void SpawnGamepiece(Vector3 spawnPosition, float scale = 1.0f, PrimitiveType type = PrimitiveType.Sphere)
    {
        FieldSimObject currentField = FieldSimObject.CurrentField;
        // TODO this should be chosen by the user in a dropdown
        GamepieceSimObject chosenGamepiece = currentField.Gamepieces[0];
        GamepieceSimObject gamepiece = new GamepieceSimObject(chosenGamepiece.Name, chosenGamepiece.GamepieceObject);
        GameObject.Instantiate(gamepiece.GamepieceObject);
        _gamepieces.Add(gamepiece);
    }
}