using System;
using SynthesisAPI.AssetManager;
using SynthesisAPI.UIManager.VisualElements;

namespace SynthesisCore.UI
{
    public class Toggle
    {
        #region UIElements

        private static VisualElementAsset _toggleAsset;

        private VisualElement _visualElement;

        private Button _button;

        #endregion

        #region Properties

        private bool _enabled;

        public bool Enabled { get => _enabled; set { _enabled = value; SetButton(); } }

        public delegate void SubscribeEvent(bool b);

        public event SubscribeEvent OnValueChanged;

        #endregion

        public static implicit operator VisualElement(Toggle t) => t._visualElement;

        public Toggle(string name)
        {
            Init(name);
            Enabled = false;
        }

        public Toggle(string name, bool enabled)
        {
            Init(name);
            Enabled = enabled;
        }

        private void Init(string name)
        {
            if (_toggleAsset == null)
                _toggleAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/Toggle.uxml");
            _visualElement = _toggleAsset.GetElement(name);
            InitButton();
        }

        #region Button

        private void InitButton()
        {
            _button = (Button)_visualElement.Get("toggle-button");
            _button.Subscribe(x => ToggleButton());
        }

        private void ToggleButton()
        {
            Enabled = !_enabled;
            OnValueChanged?.Invoke(_enabled);
        }

        private void SetButton()
        {
            _button.SetStyleProperty("background-image", _enabled ?
                    "/modules/synthesis_core/UI/images/toggle-on-icon.png" :
                    "/modules/synthesis_core/UI/images/toggle-off-icon.png");
        }

        #endregion
    }
}
