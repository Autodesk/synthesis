using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.VersionControl;
using UnityEngine;

public class ScoreZoneManipulatorManager : MonoBehaviour
{

    public bool Selected;

    private GameObject[] Arrows = new GameObject[3];
    private MeshRenderer CubeMeshRenderer;

    private ScoreZoneSelectible selectable;
    
    Color blueSelectColor = new Color(127/255f, 127/255f, 255/255f, 255/255f);
    Color blueDeselectColor = new Color(127/255f, 127/255f, 255/255f, 127/255f);
    Color redSelectColor = new Color(255/255f, 127/255f, 127/255f, 255/255f);
    Color redDeselectColor = new Color(255/255f, 127/255f, 127/255f, 127/255f);
    
    // GameObject 

    // Use this for initialization
    void Start ()
    {
        Selected = false;
        
        CubeMeshRenderer = transform.Find("Cube").gameObject.GetComponent<MeshRenderer>();
        selectable = GetComponent<ScoreZoneSelectible>();
        
        for (var i=0; i<Arrows.Length; i++ )
            Arrows[i] = transform.GetChild(i+1).gameObject;

        Arrows[0].GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        Arrows[1].GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
        Arrows[2].GetComponentInChildren<MeshRenderer>().material.color = Color.green;
    }
	
    // Update is called once per frame
    void Update () {
        for (var i = 0; i < Arrows.Length; i++)
        {
            if (Arrows[i] == null) continue;
            Arrows[i].GetComponent<Collider>().enabled = Selected;
            Arrows[i].transform.GetChild(0).GetComponent<MeshRenderer>().enabled = Selected;
        }

        CubeMeshRenderer.material.color = // Nasty multi line ternary, but it gets the job done.
            Selected
                ? selectable.SettingsContainer.TeamZone == ScoreZoneSettingsContainer.Team.Blue
                    ? blueSelectColor
                    : redSelectColor
                : selectable.SettingsContainer.TeamZone == ScoreZoneSettingsContainer.Team.Blue
                    ? blueDeselectColor
                    : redDeselectColor;

        // CubeMeshRenderer.enabled = Selected;
    }

    public void Select()
    {
        Selected = true;
    }

    public void DeSelect()
    {
        Selected = false;
    }
}