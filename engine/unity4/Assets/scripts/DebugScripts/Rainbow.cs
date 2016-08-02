using UnityEngine;
using System.Collections;

public class Rainbow : MonoBehaviour {

    private float frequency;
    private Color color;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        frequency += 0.1f;
        color.r = Mathf.Sin(frequency + 0) * .49f + .5f;
        color.g = Mathf.Sin(frequency + 2) * .49f + .5f;
        color.b = Mathf.Sin(frequency + 4) * .49f + .5f;
        transform.renderer.material.color = color;
	}
}
