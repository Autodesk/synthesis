using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;

public class ScoreZoneSelectableManager : MonoBehaviour
{
    public GameObject CurrentlySelected { get; private set; }

    public float ObjInstantiationDistance;

    private List<GameObject> scoreZones = new List<GameObject>();

    private ScoreZonePrefUIManager prefUIManager;
    
    public bool ObjectSelected { get; private set; }

    public GameObject ScoreZonePrefab;

    // Use this for initialization
    void Start ()
    {
        scoreZones.Add(GameObject.FindGameObjectWithTag("ScoreZone")); // Just for debugging we're adding the one that's presupplied in the scene
		
        CurrentlySelected = (scoreZones.Count >= 1) ? scoreZones[0] : null;

        prefUIManager = GameObject.Find("ScoreZonePrefsPanel").GetComponent<ScoreZonePrefUIManager>();

        ObjectSelected = CurrentlySelected != null;
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
        
        // Debug.Log((CurrentlySelected == null) ? "Nothing selected" : "Something selected");
    } // Debug.Log("object is not a scorezone: " + hit.collider.gameObject.transform.parent.name);

    void CheckSelectedObject()
    {
        RaycastHit hit = new RaycastHit();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
        
        if (!Physics.Raycast(ray, out hit, 100)) // If we didn't hit anything
        {
            OnDeselect();
            CurrentlySelected = null;
            return;
        }

        if (hit.transform.CompareTag("ScoreZone")) return; // We're done here
        
        // Maybe we hit one of the arrows or something
        if (hit.transform.parent == null) // If we hit top level that isn't scorezone, we don't care
        {
            OnDeselect();
            CurrentlySelected = null;
            return;
        }
    

        if (hit.transform.parent.CompareTag("ScoreZone")) return; // This means we hit something good

        OnDeselect();
        CurrentlySelected = null;
    }

    public void SelectZone(GameObject obj)
    {
        if (CurrentlySelected != obj)
        {
            CurrentlySelected = obj;
            OnSelect();
        }
    }

    public void OnSelect()
    {
        Debug.Log("On first select called");
        
        prefUIManager.LoadPrefs(CurrentlySelected.GetComponent<ScoreZoneSelectible>().SettingsContainer);

        ObjectSelected = true;
    }

    public void OnDeselect()
    {
        Debug.Log("On deselect");
        
        ObjectSelected = false;
    }

    public void InstantiateZone(Dropdown zoneType)
    {
        Vector3 instantiateSpot = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, ObjInstantiationDistance));
        // 0 is cube
        // 1 is cone
        if (zoneType.value == 0)
        {
            GameObject zone = (GameObject) Instantiate(ScoreZonePrefab, instantiateSpot, Quaternion.identity);
            scoreZones.Add(zone);
            CurrentlySelected = zone;
        }
    }

    public GameObject GetCurrentSelected()
    {
        return CurrentlySelected;
    }

    public void DestroySelected()
    {
        scoreZones.Remove(CurrentlySelected);
        Destroy(CurrentlySelected);
        CurrentlySelected = null;
    }

    public void SetReinstantationPref(bool val)
    {
		Debug.Log("Got updated reinstantiation pref of " + val);
        CurrentlySelected.GetComponent<ScoreZoneSelectible>().SetReinstantiationPref(val);
    }
    
    public void SetDestroyPref(bool val)
    {
		Debug.Log("Got updated destroy pref of " + val);
        CurrentlySelected.GetComponent<ScoreZoneSelectible>().SetDestroyPref(val);
    }
    
    public void SetScale(Vector3 scale)
    {
        if (CurrentlySelected != null)
            CurrentlySelected.GetComponent<ScoreZoneSelectible>().SetScale(scale);
    }

    public void SetScore(float score)
    {
		Debug.Log("Got updated score of " + score);
        CurrentlySelected.GetComponent<ScoreZoneSelectible>().SetScore(score);
    }
    
    public void SetTeam (ScoreZoneSettingsContainer.Team team)
    {
		Debug.Log("Got updated team of " + team.ToString());
        CurrentlySelected.GetComponent<ScoreZoneSelectible>().SetTeam(team);
    }

    public void SaveZones()
    {
        string directory = PlayerPrefs.GetString("simSelectedField");
    }

    public void LoadZones()
    {
        DestroyAllZones();
        string directory = PlayerPrefs.GetString("simSelectedField");
    }

    public void DestroyAllZones()
    {
        if (scoreZones.Count == 0) return;
        CurrentlySelected = null;
        for (int i=0; i<scoreZones.Count; i++)
        {
            Destroy(scoreZones[i]);
        }
    }
}