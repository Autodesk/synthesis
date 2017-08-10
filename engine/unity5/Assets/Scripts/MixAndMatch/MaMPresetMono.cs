using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaMPresetMono : MonoBehaviour {
    public GameObject preset;
    public Transform parent;

    List<Vector2> positions = new List<Vector2>(new Vector2[] { new Vector2(450, 0), new Vector2(700, 0), new Vector2(900, 0)});
    // Use this for initialization
    void Start () {
        
    }
	
    /// <summary>
    /// Creates a GameObject clone of the PresetPrefab
    /// </summary>
    /// <param name="maMPreset"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public GameObject CreateClone(MaMPreset maMPreset, int position)
    {
        GameObject clone = Instantiate(preset, parent);
        clone.GetComponent<RectTransform>().anchoredPosition = positions[position];

        return clone;
    }
	// Update is called once per frame
	void Update () {
		
	}
}
