using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI {
    [RequireComponent(typeof(Image))]
    public class GradientImageUpdater : MonoBehaviour {
        private static readonly int Props          = Shader.PropertyToID("_WidthHeightRadius");
        private static readonly int StartColorProp  = Shader.PropertyToID("_StartColor");
        private static readonly int EndColorProp = Shader.PropertyToID("_EndColor");
        private static readonly int HorizontalProp = Shader.PropertyToID("_Horizontal");

        public float Radius = 8;
        public Color StartColor;
        public Color EndColor;
        public bool Horizontal = true;

        private Material _material;

        private void GetMaterial() {
            if (_material == null)
                _material = _material = new Material(Shader.Find("ImageGradient/ImageGradient"));
        }

        private void Start() {
            GetMaterial();
            Refresh();

            GetComponent<Image>().material = _material;
        }

        private void OnRectTransformDimensionsChange() {
            if (enabled && _material != null) {
                Refresh();
            }
        }

        public void Refresh() {
            GetMaterial();

            var rect = ((RectTransform) transform).rect;

            _material.SetVector(Props, new Vector4(rect.width, rect.height, Radius * 2, 0));
            _material.SetColor(StartColorProp, StartColor);
            _material.SetColor(EndColorProp, EndColor);
            _material.SetInt(HorizontalProp, Horizontal ? 1 : 0);
        }
    }
}
