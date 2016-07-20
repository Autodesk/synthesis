using UnityEngine;
using System.Collections;

public class Settings : MonoBehaviour {

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this);
        Controls controls = new Controls();
        Controls.ResetDefaults();
        Controls.LoadControls();
	}
	
	// Update is called once per frame
	void Update () {
	    
	}
}
