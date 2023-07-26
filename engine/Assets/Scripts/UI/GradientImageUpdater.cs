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
        private static readonly int OffsetProp     = Shader.PropertyToID("_Offset");
        private static readonly int GradientAngleProp  = Shader.PropertyToID("_GradientAngle");
        private static readonly int GradientSpreadProp = Shader.PropertyToID("_GradientSpread");

        public float Radius = 30;
        public Color StartColor;
        public Color EndColor;
        public float GradientAngle  = 0;
        public float GradientSpread = 1;
        public Color TintColor = Color.white;
        public Vector2 Offset = new Vector2();

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

            GradientAngle %= 2f * Mathf.PI;
            if (GradientAngle < 0)
                GradientAngle += 2f * Mathf.PI;

            GradientSpread = Mathf.Clamp(GradientSpread, 0.1f, 2f);

            _material.SetVector(Props, new Vector4(rect.width, rect.height, Radius * 2, 0));
            _material.SetColor(StartColorProp, StartColor);
            _material.SetColor(EndColorProp, EndColor);
            _material.SetFloat("_GradientAngle", GradientAngle);
            _material.SetFloat(GradientSpreadProp, GradientSpread);
            _material.SetColor(TintColorProp, TintColor);
            _material.SetVector(OffsetProp, Offset);
        }
    }
}
