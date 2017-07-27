using UnityEngine;
using System.Collections;

public class Settings : MonoBehaviour {

	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this);
        //gameObject.AddComponent<Controls>();
        Controls.Reset();
        Controls.Load();
	}
	
	// Update is called once per frame
	void Update ()
    {
	    
	}
}
