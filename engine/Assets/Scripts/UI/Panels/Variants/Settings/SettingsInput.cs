using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsInput : MonoBehaviour
{
    SettingsPanel.InputType type;//type


    public TextMeshProUGUI _name;//shared

    //Toggle
    public Toggle _toggle;

    //keybind
    public TextMeshProUGUI _key;
    private bool bind = false;

    //dropdown list
    public TMP_Dropdown _dropdown;

    public Slider _slider;
    public TextMeshProUGUI _sliderValue;


    public void Init(string name, bool state)//TOGGLE
    {
        type = SettingsPanel.InputType.TOGGLE;
        _name.text = name;
        _toggle.isOn = state;
    }
    public SettingsPanel.InputType getType()
    {
        return type;
    }
    public void Init(string name, string control)//KEYBIND
    {
        type = SettingsPanel.InputType.KEYBIND;
        _name.text = name;
        _key.text = control;
    }
    public void Init(string name, string[] dropdownList, int value)//DROPDOWN LIST
    {
        type = SettingsPanel.InputType.DROPDOWN;
        _name.text = name;
        foreach (string c in dropdownList)
        {
            _dropdown.options.Add(new TMP_Dropdown.OptionData(c));//CREATE DROPDOWN
        }
        _dropdown.value = value;//CURRENT STATE
    }
    public void Init(string name, int lowValue, int highValue, int value)//SLIDER
    {
        type = SettingsPanel.InputType.SLIDER;
        _name.text = name;
        _slider.minValue = lowValue;
        _slider.maxValue = highValue;
        _slider.value = value;        //Make sure it is formatted correctly
        sliderValueChanged();
    }
    public void sliderValueChanged(){
        _sliderValue.text = _slider.value.ToString();
    }
    public void setKeybind() //CALLED BY KEYBIND
    {
        bind = true;
    }
    private void OnGUI()
    {
        if (type== SettingsPanel.InputType.KEYBIND&&bind) //SET KEYBIND
        {
            Event e = Event.current;
            if (e.isKey)
            {
                _key.text = e.keyCode.ToString();
                bind = false;
            }
        }
    }   

    //ON CHANGE FUNCTION
    
}
