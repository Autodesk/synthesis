using Synthesis.ModelManager.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// TODO: Add warning label
public class GearboxItem : MonoBehaviour
{
    public TMP_InputField NameField;
    public GameObject JointList;
    public TMP_InputField MaxSpeedField;
    public TMP_InputField TorqueField;
    public GameObject JointItem;

    private static int nextGuid = 1;
    private int itemGuid;
    private GearboxConfigPanel panel;

    public List<string> SelectedJoints;
    private List<JointItem> jointItems;

    public GearboxData Data {
        get => new GearboxData() { Name = NameField.text, MotorUuids = SelectedJoints.ToArray(),
            MaxSpeed = GetFloatFromInput(MaxSpeedField), Torque = GetFloatFromInput(TorqueField) };
    }

    public void Remove()
    {
        panel.RemoveGearboxItem(this);
    }

    public void Init(GearboxConfigPanel panel, GearboxData data)
    {
        Init(panel);

        NameField.text = data.Name;
        foreach (JointItem i in jointItems)
        {
            if (data.MotorUuids.Contains(i.Uuid))
                i.UseToggle.isOn = true;
        }
        MaxSpeedField.text = data.MaxSpeed.ToString();
        TorqueField.text = data.Torque.ToString();
    }

    public void Init(GearboxConfigPanel panel)
    {
        this.panel = panel;
        itemGuid = ++nextGuid;

        SelectedJoints = new List<string>();
        PopulateJoints();

        NameField.onDeselect.AddListener(x =>
        {
            int count = 0;
            NameField.text = "TEMP";
            var allData = this.panel.CompileData();
            while (allData.Exists(y => y.Name == x + (count == 0 ? "" : $"-{count}")))
                count++;
            NameField.text = x + (count == 0 ? "" : $"-{count}");
        });
    }

    public override int GetHashCode()
    {
        return 847205710 * itemGuid;
    }

    public override bool Equals(object other)
    {
        if (!(other is GearboxItem))
            return false;

        return ((GearboxItem)other).GetHashCode() == GetHashCode();
    }

    public void PopulateJoints()
    {
        jointItems = new List<JointItem>();
        foreach (Motor m in panel.SelectedModel.Motors) {
            var item = Instantiate(JointItem, JointList.transform).GetComponent<JointItem>();
            item.Init(this, m.Meta.Name, m.Meta.Uuid);
            jointItems.Add(item);
        }
    }

    private float GetFloatFromInput(TMP_InputField input)
    {
        int.TryParse(input.text, out int result);
        return result;
    }

    public static bool operator ==(GearboxItem a, GearboxItem b) => a.Equals(b);
    public static bool operator !=(GearboxItem a, GearboxItem b) => !a.Equals(b);
}
