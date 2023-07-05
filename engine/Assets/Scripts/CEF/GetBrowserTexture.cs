using UnityEngine;
using System.Collections;

public class GetBrowserTexture : MonoBehaviour {
    public CEFOffScreenBrowser Browser;

    private Material _material;

    private void Start() {
        _material             = GetComponent<Renderer>().material;
        _material.mainTexture = Browser.BrowserTexture;
    }
}
