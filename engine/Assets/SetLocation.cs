using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.ModelManager;
using Synthesis.Configuration;

public class SetLocation : MonoBehaviour
{
    public GameObject gizmo;
    public MoveArrow gizmoScript;
    public void SetRobotSpawn()//gizmo setup
    {
        if (GizmoManager.currentGizmo != null) return;
        if (ModelManager.primaryModel == null) return;
        ResetRobotChildren();
        Transform currentRobot = ModelManager.primaryModel._object.transform;
        GizmoManager.SpawnGizmo(gizmo, currentRobot,ModelManager.spawnLocation);        
    }
    public void ResetRobot()//resetting position
    {
        if (ModelManager.primaryModel == null) return;

        ModelManager.primaryModel._object.transform.position = ModelManager.spawnLocation;
        ModelManager.primaryModel._object.transform.rotation = ModelManager.spawnRotation;


        ResetRobotChildren();

        //reset inertia by turning by setting kinematic temporarily
        gizmoScript.HierarchyRigidbodiesToDictionary();
        gizmoScript.SetRigidbodies(false);
        gizmoScript.SetRigidbodies(true);
    }
    private void ResetRobotChildren()//resets all children of robot to the parent position
    {
        for(int i = 0; i < ModelManager.primaryModel._object.transform.childCount; i++)
        {
            ModelManager.primaryModel._object.transform.GetChild(i).transform.localRotation = Quaternion.identity;
            ModelManager.primaryModel._object.transform.GetChild(i).transform.localPosition = new Vector3(0, 0, 0);
        }
    }
}
