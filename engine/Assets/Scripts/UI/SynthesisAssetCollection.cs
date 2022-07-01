using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

public class SynthesisAssetCollection : MonoBehaviour {

    public static SynthesisAssetCollection Instance;

    [SerializeField]
    public List<Sprite> SpriteAssets;
    [SerializeField]
    public List<GameObject> PanelPrefabs;
    [SerializeField]
    public List<GameObject> DynamicModalPrefabs;
    [SerializeField]
    public List<TMPro.TMP_FontAsset> Fonts;
    [SerializeField]
    public Volume BlurVolume;
    [SerializeField]
    public GameObject ReplaySlider;
    public static Volume BlurVolumeStatic => Instance.BlurVolume;
    public static GameObject ReplaySliderStatic => Instance.ReplaySlider;

    public void Awake() {
        Instance = this;
    }

    public static Sprite GetSpriteByName(string name)
        => Instance.SpriteAssets.First(x => x.name == name);

    public static GameObject GetPanelByName(string name)
        => Instance.PanelPrefabs.First(x => x.name == name);

    public static GameObject GetModalPrefab(string name)
        => Instance.DynamicModalPrefabs.First(x => x.name == name);
    public static TMPro.TMP_FontAsset GetFont(string name)
        => Instance.Fonts.First(x => x.name == name);
}
