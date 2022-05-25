using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Events;

public class MultiplayerRobotItem : MonoBehaviour {
    private string _name;
    public string Name {
        get => _name;
    }
    [SerializeField]
    public TMP_Text RobotNameText;
    [SerializeField]
    public Image ColorImage;
    [SerializeField]
    public Button ItemButton;

    public void Init(string name, UnityAction callback) {
        SetName(name);
        ItemButton.onClick.AddListener(callback);
    }

    public void SetName(string name) {
        RobotNameText.text = name;
        _name = name;
    }

    public void SetColor(Color c) {
        ColorImage.color = c;
    }
}
