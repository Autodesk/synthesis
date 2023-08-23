using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.EventListeners {
    public class HoverEventListener : MonoBehaviour,
                                      IPointerEnterHandler,
                                      IPointerExitHandler,
                                      IPointerDownHandler,
                                      IPointerUpHandler {
        [SerializeField]
        private Image _image;
        [SerializeField]
        private Color _defaultColor = Color.white;
        [SerializeField]
        private Color _hoverColor = Color.grey;
        [SerializeField]
        private Color _selectedColor = Color.gray;
        [SerializeField]
        private float _hoverScaleMultiplier = 1.05f;
        [SerializeField]
        private float _clickedScaleMultiplier = 1.1f;
        [SerializeField]
        private Transform _scaledObj;
        [SerializeField]
        private bool _isSlider;

        private Color _setColor;

        private GradientImageUpdater _imageUpdater;

        public GradientImageUpdater ImageUpdater { set => _imageUpdater = value; }

        private void Start() {
            if (_image)
                _setColor = _image.color;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            SetTintColor(_hoverColor);
            SetScaleMultiplier(_hoverScaleMultiplier);
        }

        public void OnPointerExit(PointerEventData eventData) {
            if (_isSlider && _pointerDown)
                return;

            SetTintColor(_defaultColor);
            SetScaleMultiplier(1);
        }

        private bool _pointerDown;

        public void OnPointerDown(PointerEventData eventData) {
            SetTintColor(_selectedColor);
            SetScaleMultiplier(_clickedScaleMultiplier);
            _pointerDown = true;
        }

        public void OnPointerUp(PointerEventData eventData) {
            SetTintColor(_defaultColor);
            SetScaleMultiplier(1);
            _pointerDown = false;
        }

        private void SetTintColor(Color color) {
            if (_imageUpdater) {
                _imageUpdater.TintColor = color;
                _imageUpdater.Refresh();
            } else if (_image)
                if (_isSlider)
                    _image.color = color;
                else
                    _image.color = color * _setColor;
        }

        private void SetScaleMultiplier(float multiplier) {
            if (_scaledObj == null)
                transform.localScale = Vector3.one * multiplier;
            else
                _scaledObj.localScale = Vector3.one * multiplier;
        }
    }
}
