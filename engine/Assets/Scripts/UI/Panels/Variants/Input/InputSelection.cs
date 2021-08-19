using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace Synthesis.UI.Panels {
    public class InputSelection : MonoBehaviour {
        private string _inputKey;

        public string InputKey {
            get => _inputKey;
        }
        public TMP_Text Title;
        public TMP_Text SelectedValue;
        public Button Button;
        private InputPanel _panel;

        public void Init(string title, string inputKey, string inputValue, InputPanel panel) {
            Title.text = title;
            _inputKey = inputKey;
            _panel = panel;
            
            UpdateUI(inputValue);

            Button.onClick.AddListener(() => panel.RequestInput(this));
        }

        public void UpdateUI(string newKeyValue) {
            SelectedValue.text = newKeyValue;
        }
    }
}
