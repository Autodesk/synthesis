using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Highlighter : MonoBehaviour {
    public bool EnableHighlight = true;

    private Renderer _renderer;
    private Material _highlightMat;
    private Material _originalMat;

    private void Start() {
        _renderer = GetComponent<Renderer>();
        _originalMat = _renderer.material;
        _highlightMat = new Material(Shader.Find("Shader Graph/HighlightSynthesisShader"));
    }

    private void Enable() {
        _renderer.material = _highlightMat;
    }

    private void Disable() {
        _renderer.material = _originalMat;
    }
}
