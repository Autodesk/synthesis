using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.ModelManager;
using Synthesis.Configuration;

public class SetLocation : MonoBehaviour
{
    public GameObject gizmo;
    public MoveArrow gizmoScript;
    public void SetRobotSpawn()
    {
        if (ModelManager.primaryModel == null) return;        
        Transform currentRobot = ModelManager.primaryModel._object.transform;
        GizmoManager.SpawnGizmo(gizmo, currentRobot,ModelManager.spawnLocation);        
    }
    public void ResetRobot()
    {
        if (ModelManager.primaryModel == null) return;
        gizmoScript.HierarchyRigidbodiesToDictionary();
        gizmoScript.SetRigidbodies(false);
        ModelManager.primaryModel._object.transform.position = ModelManager.spawnLocation;
        gizmoScript.SetRigidbodies(true);
    }
}
