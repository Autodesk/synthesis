using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScoreZoneSelectableManager : MonoBehaviour
{
    public GameObject CurrentlySelected { get; private set; }

    public float ObjInstantiationDistance;

    private List<GameObject> scoreZones = new List<GameObject>();

    // Use this for initialization
    void Start ()
    {
        scoreZones.Add(GameObject.FindGameObjectWithTag("ScoreZone")); // Just for debugging we're adding the one that's presupplied in the scene
		
        CurrentlySelected = (scoreZones.Count >= 1) ? scoreZones[0] : null;
    }
	
    // Update is called once per frame
    void Update () {
        for (var i=0; i<scoreZones.Count; i++)
            if (!scoreZones[i].Equals(CurrentlySelected))
                scoreZones[i].BroadcastMessage("DeSelect");
        if (CurrentlySelected != null) CurrentlySelected.BroadcastMessage("Select");

        if (Input.GetMouseButtonDown(0))
        {
            CheckSelectedObject();
        }
        
        Debug.Log((CurrentlySelected == null) ? "Nothing selected" : "Something selected");
    } // Debug.Log("object is not a scorezone: " + hit.collider.gameObject.transform.parent.name);

    void CheckSelectedObject()
    {
        RaycastHit hit = new RaycastHit();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
        
        if (!Physics.Raycast(ray, out hit, 100)) // If we didn't hit anything
        {
            CurrentlySelected = null;
            return;
        }

        if (hit.transform.CompareTag("ScoreZone")) return; // We're done here
        
        // Maybe we hit one of the arrows or something
        if (hit.transform.parent == null) // If we hit top level that isn't scorezone, we don't care
        {
            CurrentlySelected = null;
            return;
        }
    

        if (hit.transform.parent.CompareTag("ScoreZone")) return; // This means we hit something good

        CurrentlySelected = null;

    }

    public void SelectZone(GameObject obj)
    {
        CurrentlySelected = obj;
    }

    public void AddCubeZone()
    {
        Vector3 instantiateSpot = Camera.main.transform.forward * ObjInstantiationDistance;
    }

    public GameObject GetCurrentSelected()
    {
        return CurrentlySelected;
    }
}