using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.EventListeners {
    public class ButtonEventListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {
        [SerializeField] private Color _hoverColor = Color.grey;
        [SerializeField] private Color _selectedColor = Color.gray;
        [SerializeField] private float _hoverScaleMultiplier = 1.05f;
        [SerializeField] private float _clickedScaleMultiplier = 1.1f;
    
        private GradientImageUpdater _imageUpdater;

        public GradientImageUpdater ImageUpdater {
            set => _imageUpdater = value;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            SetTintColor(_hoverColor);
            SetScaleMultiplier(_hoverScaleMultiplier);
        }

        public void OnPointerExit(PointerEventData eventData) {
            SetTintColor(Color.white);
            SetScaleMultiplier(1);

        }

        public void OnPointerDown(PointerEventData eventData) {
            SetTintColor(_selectedColor);
            SetScaleMultiplier(_clickedScaleMultiplier);
        }

        public void OnPointerUp(PointerEventData eventData) {
            SetTintColor(Color.white);
            SetScaleMultiplier(1);
        }

        private void SetTintColor(Color color) {
            if (_imageUpdater) {
                _imageUpdater.TintColor = color;
                _imageUpdater.Refresh();
            }
        }

        private void SetScaleMultiplier(float multiplier) {
            transform.localScale = Vector3.one * multiplier;
        }
    }
}
