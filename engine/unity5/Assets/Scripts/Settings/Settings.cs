using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this);
        Controls.Load();
	}
	
	// Update is called once per frame
	void Update ()
    {
	    
	}
}
