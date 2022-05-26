using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SynthesisAssetCollection : MonoBehaviour {

    public static SynthesisAssetCollection Instance;

    [SerializeField]
    public List<Sprite> SpriteAssets;
    [SerializeField]
    public List<GameObject> PanelPrefabs;

    public void Awake() {
        Instance = this;
    }

    public static Sprite GetSpriteByName(string name)
        => Instance.SpriteAssets.First(x => x.name == name);

    public static GameObject GetPanelByName(string name)
        => Instance.PanelPrefabs.First(x => x.name == name);
}
