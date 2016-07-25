using UnityEngine;
using System.Collections;

/// <summary>
/// The class that initalizes and loads all settings
/// </summary>
public class Settings : MonoBehaviour {

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this); //Makes it so that this settings gameobject persists throughout all scenes.

        //Initalizes and loads the controls.
        Controls controls = new Controls();
        Controls.ResetDefaults();
        Controls.LoadControls();
	}
	
	// Update is called once per frame
	void Update () {
	    
	}
}
