using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TabButton : MonoBehaviour {
    [SerializeField]
    public Image ButtonImage;
    [SerializeField]
    public Button ActualButton;
    [SerializeField]
    public TMP_Text ButtonText;
    private string _name;
    public string Name {
        get => _name;
        set {
            _name           = value;
            ButtonText.text = _name;
        }
    }

    public void SetCallback(UnityAction a) {
        ActualButton.onClick.AddListener(a);
    }
}
