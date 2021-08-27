using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.ModelManager;

public class SetLocation : MonoBehaviour
{
    public GameObject gizmo;
    public void SetRobotSpawn()
    {
        if (ModelManager.primaryModel!=null)
        {
            Transform currentRobot = ModelManager.primaryModel._object.transform;
            GizmoManager.SpawnGizmo(gizmo, currentRobot,ModelManager.spawnLocation);

        }
    }
    public void ResetRobot()
    {
        ModelManager.primaryModel._object.transform.position = ModelManager.spawnLocation;
    }
}
