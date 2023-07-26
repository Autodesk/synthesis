using UI.EventListeners;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    [RequireComponent(typeof(Image))]
    [ExecuteAlways]
    public class GradientImageUpdater : MonoBehaviour {
        private static readonly int Props          = Shader.PropertyToID("_WidthHeightRadius");
        private static readonly int StartColorProp = Shader.PropertyToID("_StartColor");
        private static readonly int EndColorProp   = Shader.PropertyToID("_EndColor");
        private static readonly int TintColorProp  = Shader.PropertyToID("_TintColor");
        private static readonly int HorizontalProp = Shader.PropertyToID("_Horizontal");
        private static readonly int OffsetProp     = Shader.PropertyToID("_Offset");

        public float Radius = 30;
        public Color StartColor;
        public Color EndColor;
        public Color TintColor = Color.white;

        public bool Horizontal = true;

        private Material _material;

        private void GetMaterial() {
            if (_material == null)
                _material = _material = new Material(Shader.Find("ImageGradient/ImageGradient"));
        }

        private void Start() {
            GetMaterial();

            var image = GetComponent<Image>();
            if (image.sprite != null) {
                _material = SynthesisAssetCollection.GetDefaultSpriteMat();
                Destroy(this);
            }
            image.material = _material;

            if (TryGetComponent<HoverEventListener>(out var listener))
                listener.ImageUpdater = this;

            Refresh();
        }

        private void OnRectTransformDimensionsChange() {
            if (enabled && _material != null) {
                Refresh();
            }
        }

        private void OnValidate() {
            Refresh();
        }

        public void Refresh() {
            GetMaterial();

            var rect = ((RectTransform) transform).rect;

            _material.SetVector(Props, new Vector4(rect.width, rect.height, Radius * 2, 0));
            _material.SetColor(StartColorProp, StartColor);
            _material.SetColor(EndColorProp, EndColor);
            _material.SetColor(TintColorProp, TintColor);

            _material.SetInt(HorizontalProp, Horizontal ? 1 : 0);
            _material.SetFloat(OffsetProp, 0);
        }
    }
}
