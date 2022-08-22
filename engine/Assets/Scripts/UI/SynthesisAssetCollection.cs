using System;
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
    public List<GameObject> DynamicUIPrefabs;
    [SerializeField]
    public List<TMPro.TMP_FontAsset> Fonts;
    [SerializeField]
    public GameObject BlurVolumePrefab;
    [SerializeField]
    public GameObject ReplaySlider;
    [SerializeField]
    public GameObject GizmoPrefab;

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
    public static GameObject GizmoPrefabStatic => Instance.GizmoPrefab;

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

        Screen.fullScreenMode = FullScreenMode.MaximizedWindow;
        var biggestWidth = 0;
        for (int i = 1; i < Screen.resolutions.Length; i++) {
            if (Screen.resolutions[i].width > Screen.resolutions[biggestWidth].width)
                biggestWidth = i;
        }
        var res = Screen.resolutions[biggestWidth];
        Screen.SetResolution(res.width, res.height, FullScreenMode.MaximizedWindow);
    }

    public static Sprite GetSpriteByName(string name)
        => Instance.SpriteAssets.First(x => x.name == name);

    public static GameObject GetPanelByName(string name)
        => Instance.PanelPrefabs.First(x => x.name == name);

    public static GameObject GetUIPrefab(string name)
        => Instance.DynamicUIPrefabs.First(x => x.name == name);
    public static TMPro.TMP_FontAsset GetFont(string name)
        => Instance.Fonts.First(x => x.name == name);


    public void OnDestroy()
    {
        AnalyticsManager.LogEvent(new AnalyticsEvent(category: "app", action: $"close", label:""));
    }
}
