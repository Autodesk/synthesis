using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreZoneSelectible : MonoBehaviour
{
	private ScoreZoneSelectableManager manager;

	// Use this for initialization
	void Start ()
	{
		manager = GameObject.Find("EditScoreZoneManager").GetComponent<ScoreZoneSelectableManager>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnMouseOver()
	{
		if (Input.GetMouseButtonDown(0))
		{
			manager.SelectZone(this.gameObject);
		}
	}
}
