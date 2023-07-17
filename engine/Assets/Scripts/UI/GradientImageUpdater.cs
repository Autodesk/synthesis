using UnityEngine;
using UnityEngine.UI;

namespace UI {
    [RequireComponent(typeof(Image))]
    public class GradientImageUpdater : MonoBehaviour {
        private static readonly int Props          = Shader.PropertyToID("_WidthHeightRadius");
        private static readonly int LeftColorProp  = Shader.PropertyToID("_LeftColor");
        private static readonly int RightColorProp = Shader.PropertyToID("_RightColor");

        public float Radius = 8;
        public Color LeftColor;
        public Color RightColor;

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
            _material.SetColor(LeftColorProp, LeftColor);
            _material.SetColor(RightColorProp, RightColor);
        }
    }
}
