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
    public GameObject BlurVolumePrefab;
    [SerializeField]
    public GameObject ReplaySlider;
    private static Volume _blurVolumeStatic = null;
    public static Volume BlurVolumeStatic {
        get {
            if (_blurVolumeStatic == null) {
                _blurVolumeStatic = GameObject.Instantiate(Instance.BlurVolumePrefab).GetComponent<Volume>();
            }
            return _blurVolumeStatic;
        }
    }
    public static GameObject ReplaySliderStatic => Instance.ReplaySlider;

#nullable enable
    private GameObject? _defaultFloor = null;
    public static GameObject? DefaultFloor {
        get {
            if (Instance._defaultFloor == null) {
                Instance._defaultFloor = GameObject.FindGameObjectWithTag("default-floor");
            }
            return Instance._defaultFloor;
        }
    }
#nullable disable

    public void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
