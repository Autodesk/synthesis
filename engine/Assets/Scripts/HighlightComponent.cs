using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HighlightComponent : MonoBehaviour {

    private Material _material;
    private MeshRenderer[] _renderers;
    private Material[] _originalMaterials;

    private Color _color;
    public Color Color {
        get => _color;
        set {
            _color = value;
            OnEnable();
        }
    }

    public void Awake() {
        _renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
        _originalMaterials = _renderers.Select(x => x.material).ToArray();
        _material = new Material(Shader.Find("Shader Graphs/HighlightShader"));
    }

    public void OnEnable() {
        _material.SetColor("_Color", _color);
        for (int i = 0; i < _renderers.Length; i++) {
            _renderers[i].material = _material;
        }
        
        Debug.Log($"{transform.name}: Enabled");
    }

    public void OnDisable() {
        for (int i = 0; i < _renderers.Length; i++) {
            _renderers[i].material = _originalMaterials[i];
        }
        
        Debug.Log($"{transform.name}: Disabled");
    }
}
