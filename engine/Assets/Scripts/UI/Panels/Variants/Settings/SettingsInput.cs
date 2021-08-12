using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
namespace Synthesis.UI.Panels.Variant
{
    public class SettingsInput : MonoBehaviour
    {
        private SettingsPanel.InputType _type;//type
        public SettingsPanel.InputType Type
        {
            get => _type;
        }
        private string _title = "sample-title";
        public string Title
        {
            get => _title;
            private set
            {
                _title = value;
                titleText.text = _title;
            }
        }
        [SerializeField] public TextMeshProUGUI titleText;//shared

        [Header("Toggle Field")]
        [SerializeField] public Toggle toggle;

        [Header("Keybinding Field")]
        [SerializeField] public TextMeshProUGUI key;
        private bool _bind = false;

        [Header("Dropdown Field")]
        [SerializeField] public TMP_Dropdown dropdown;

        [Header("Slider Field")]
        [SerializeField] public Slider slider;
        [SerializeField] public TextMeshProUGUI sliderValue;

        #region Initializers

        public void Init(string title, bool state)
        { //TOGGLE
            _type = SettingsPanel.InputType.Toggle;
            Title = title;
            toggle.isOn = state;
        }

        public void Init(string title, string control)
        { //KEYBIND
            _type = SettingsPanel.InputType.Keybind;
            Title = title;
            key.text = control;
        }

        public void Init(string title, string[] dropdownList, int value)
        { //DROPDOWN LIST
            dropdown.options.Clear();
            _type = SettingsPanel.InputType.Dropdown;
            Title = title;
            foreach (string c in dropdownList)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(c));//CREATE DROPDOWN
            }
            dropdown.value = value;//CURRENT STATE
        }

        public void Init(string name, int lowValue, int highValue, int value)
        { //SLIDER
            _type = SettingsPanel.InputType.Slider;
            Title = name;
            slider.minValue = lowValue;
            slider.maxValue = highValue;
            slider.value = value;        //Make sure it is formatted correctly
            sliderValueChanged();
        }

        #endregion

        public void sliderValueChanged()
        {
            sliderValue.text = slider.value.ToString();
        }

        public void setKeybind()
        { //CALLED BY KEYBIND
            _bind = true;
        }

        private void OnGUI()
        {
            if (_type == SettingsPanel.InputType.Keybind && _bind)
            { //SET KEYBIND
                Event e = Event.current;
                if (e.isKey)
                {
                    key.text = e.keyCode.ToString();
                    _bind = false;
                }
            }
        }
        public void OnValueChanged()
        {
            GameObject.FindObjectOfType<SettingsPanel>().onValueChanged(this);
        }
    }
}