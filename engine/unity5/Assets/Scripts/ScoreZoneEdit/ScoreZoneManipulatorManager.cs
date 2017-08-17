using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.VersionControl;
using UnityEngine;

public class ScoreZoneManipulatorManager : MonoBehaviour
{

    public bool Selected;

    private GameObject[] Arrows = new GameObject[3];
    private MeshRenderer CubeMeshRenderer;
    
    // GameObject 

    // Use this for initialization
    void Start ()
    {
        
        Selected = false;

        CubeMeshRenderer = transform.Find("Cube").gameObject.GetComponent<MeshRenderer>();
        
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

        CubeMeshRenderer.material.color =
            Selected
                ? new Color(0x19 / 0xFF, 0xFF / 0xFF, 0xFF / 0xFF, 0xFF / 0xFF)
                : new Color(0x19 / 0xFF, 0xFF / 0xFF, 0xFF / 0xFF, 0x19 / 0xFF);

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