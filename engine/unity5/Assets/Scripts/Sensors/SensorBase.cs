using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the template/parent class for all sensors within Synthesis.
/// </summary>
public abstract class SensorBase : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public abstract float ReturnOutput();
}
