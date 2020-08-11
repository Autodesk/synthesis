using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using System;
using System.Collections.Generic;

namespace SynthesisCore.Systems
{
    public class ProgressBar
    {
        private static VisualElementAsset _progressBarAsset;

        public VisualElement ProgressBarElement;
        private VisualElement _progressElement;
        private Label _labelElement;
        private double _value;
        private string _label;

        public ProgressBar(VisualElement visualElement)
        {
            ProgressBarElement = visualElement;
        }

        public ProgressBar(string name)
        {
            if (_progressBarAsset == null)
                _progressBarAsset = AssetManager.GetAsset<VisualElementAsset>("/modules/synthesis_core/UI/uxml/ProgressBar.uxml");

            ProgressBarElement = _progressBarAsset.GetElement(name);
        }

        private void UpdateValue()
        {
            if (_progressElement == null)
                _progressElement = ProgressBarElement.Get("progress-bar-progress");

            int widthPercent = (int)(_value * 100);
            _progressElement.SetStyleProperty("width", $"{widthPercent}%");
        }

        private void UpdateLabel()
        {
            if (_labelElement == null)
                _labelElement = (Label)ProgressBarElement.Get("progress-bar-label"); ;

            _labelElement.Text = _label;
        }

        public double Value
        {
            get => _value;
            set
            {
                _value = value;
                if (_value < 0)
                    _value = 0;
                if (_value > 1)
                    _value = 1;
                UpdateValue();
            }
        }

        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                UpdateLabel();
            }
        }
    }
}
