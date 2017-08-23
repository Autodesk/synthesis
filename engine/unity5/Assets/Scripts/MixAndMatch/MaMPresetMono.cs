using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaMPresetMono : MonoBehaviour {
    public GameObject preset;
    public Transform parent;

    List<Vector2> positions = new List<Vector2>(new Vector2[] { new Vector2(-65, 0), new Vector2(125, 0), new Vector2(315, 0)});
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
