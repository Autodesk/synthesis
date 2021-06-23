using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JointItem : MonoBehaviour
{
    public Toggle UseToggle;
    public TextMeshProUGUI JointName;

    private GearboxItem item;
    public string Uuid;

    public void Init(GearboxItem item, string name, string uuid)
    {
        this.item = item;
        Uuid = uuid;
        UseToggle.isOn = false;
        UseToggle.onValueChanged.AddListener(x =>
        {
            if (x)
                item.SelectedJoints.Add(Uuid);
            else
                item.SelectedJoints.RemoveAll(y => y == Uuid);
        });
        JointName.text = name;
    }
}
