using SynthesisAPI.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Logger = SynthesisAPI.Utilities.Logger;

public class GridEntry : MonoBehaviour {
    public float Delay;
    public float Length;

    [SerializeField]
    public Renderer FloorRenderer;
    private Material _mat;
    private Material MatRef => FloorRenderer.material;

    private float innerRadius;
    private float featherRadius;

    private void Start() {
        innerRadius   = FloorRenderer.material.GetFloat("INNER_RADIUS");
        featherRadius = FloorRenderer.material.GetFloat("FEATHER_RADIUS");

        _mat                   = new Material(FloorRenderer.material);
        FloorRenderer.material = _mat;
        MatRef.SetFloat("INNER_RADIUS", 0f);
        MatRef.SetFloat("FEATHER_RADIUS", 0f);

        _startTime = Time.realtimeSinceStartup;
    }

    private float _startTime = 0f;

    private void Update() {
        if (Time.realtimeSinceStartup - _startTime > Length + Delay) {
            MatRef.SetFloat("INNER_RADIUS", innerRadius);
            MatRef.SetFloat("FEATHER_RADIUS", featherRadius);
            Destroy(this);
        } else {
            float progress = Mathf.Clamp(
                Mathf.Clamp(((Time.realtimeSinceStartup - _startTime) - Delay), 0f, float.MaxValue) / Length, 0f, 1f);
            Func<float, float> progressFunc = (float x) => (-Mathf.Pow((x - 1), 2)) + 1f;
            progress                                     = progressFunc(progress);
            MatRef.SetFloat("INNER_RADIUS", innerRadius * progress);
            MatRef.SetFloat("FEATHER_RADIUS", featherRadius * progress);
        }
    }
}
