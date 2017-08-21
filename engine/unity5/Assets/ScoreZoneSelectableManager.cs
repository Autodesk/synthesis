using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using System.Xml.Serialization;
using UnityEngine.SceneManagement;

public class ScoreZoneSelectableManager : MonoBehaviour
{
    public GameObject CurrentlySelected { get; private set; }

    public float ObjInstantiationDistance;

    private List<GameObject> scoreZones = new List<GameObject>();

    private ScoreZonePrefUIManager prefUIManager;
    
    public bool ObjectSelected { get; private set; }

    public GameObject CubeScoreZonePrefab;

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
        InstantiateZone(zoneType.value);
    }

    public GameObject InstantiateZone(int zoneType)
    {
        Vector3 instantiateSpot = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, ObjInstantiationDistance));
        // 0 is cube
        // 1 is cone
        if (zoneType== 0)
        {
            GameObject zone = (GameObject) Instantiate(CubeScoreZonePrefab, instantiateSpot, Quaternion.identity);
            scoreZones.Add(zone);
            CurrentlySelected = zone;
            return zone;
        }
        else return null; // We realistically never will be returning null since only cube is implimented rn 
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
        Debug.Log("Got upxmled reinstantiation pref of " + val);
        CurrentlySelected.GetComponent<ScoreZoneSelectible>().SetReinstantiationPref(val);
    }
    
    public void SetDestroyPref(bool val)
    {
        Debug.Log("Got upxmled destroy pref of " + val);
        CurrentlySelected.GetComponent<ScoreZoneSelectible>().SetDestroyPref(val);
    }
    
    public void SetScale(Vector3 scale)
    {
        if (CurrentlySelected != null)
            CurrentlySelected.GetComponent<ScoreZoneSelectible>().SetScale(scale);
    }

    public void SetScore(float score)
    {
        Debug.Log("Got upxmled score of " + score);
        CurrentlySelected.GetComponent<ScoreZoneSelectible>().SetScore(score);
    }
    
    public void SetTeam (ScoreZoneSettingsContainer.Team team)
    {
        Debug.Log("Got upxmled team of " + team.ToString());
        CurrentlySelected.GetComponent<ScoreZoneSelectible>().SetTeam(team);
    }

    // Saves all scoring zones to a file (ScoreZones.xml) in the directory of the current loaded field
    public void SaveZones()
    {
        
        List<ScoreZoneSettingsContainer> containerList = new List<ScoreZoneSettingsContainer>();
        foreach (GameObject i in scoreZones)
            containerList.Add(i.GetComponent<ScoreZoneSelectible>().SettingsContainer);
        
        
        string directory = PlayerPrefs.GetString("simSelectedField");

        
        var xmlSeralizer = new XmlSerializer(typeof(List<ScoreZoneSettingsContainer>));

        using (Stream stream = new FileStream(
            directory + "\\ScoreZones.xml",
            FileMode.Create,
            FileAccess.Write,
            FileShare.None)
        )
            xmlSeralizer.Serialize(stream, containerList);
        
        Debug.Log("Saved zones to " + directory + "\\ScoreZones.xml");
    }

    // Loads all scoring zones from a file (ScoreZones.xml) in the directory of the current loaded field
    public void LoadZones()
    {
        string directory = PlayerPrefs.GetString("simSelectedField");
        if (!File.Exists(directory + "\\ScoreZones.xml")) return; // File does not exist, so we don't do anything
        
        DestroyAllZones();

        List<ScoreZoneSettingsContainer> containerList = new List<ScoreZoneSettingsContainer>();
        
        var xmlSeralizer = new XmlSerializer(typeof(List<ScoreZoneSettingsContainer>));
        Stream stream = new FileStream(directory + "\\ScoreZones.xml", FileMode.Open, FileAccess.Read);
        containerList = (List<ScoreZoneSettingsContainer>) xmlSeralizer.Deserialize(stream);

        foreach (ScoreZoneSettingsContainer i in containerList)
        {
            GameObject newZone = InstantiateZone(i.ZoneType);
            newZone.transform.position = i.Position;
            newZone.transform.rotation = i.Rotation;
            newZone.GetComponent<ScoreZoneSelectible>().SetContainer(i);
            
            scoreZones.Add(newZone);
        }
        
        Debug.Log("Loaded zones from " + directory + "\\ScoreZones.xml");
    }

    public void DestroyAllZones()
    {
        if (scoreZones.Count == 0) return;
        CurrentlySelected = null;
        // while (scoreZones.Count > 0)
        while (scoreZones.Count > 0)
        {
            Destroy(scoreZones[0]);
            scoreZones.RemoveAt(0);
            Debug.Log("Removed. New count: " + scoreZones.Count);
        }
    }

    public void LoadMainLevel()
    {
        SceneManager.LoadScene("MainMenu");
    }
}