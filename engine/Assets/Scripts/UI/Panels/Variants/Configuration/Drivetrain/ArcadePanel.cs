using Synthesis.ModelManager.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ArcadePanel : DrivetrainSubPanel
{
    public TMP_Dropdown Left_Gearbox;
    public TMP_Dropdown Right_Gearbox;

    public override void Show(Model selectedModel, ConfigureDrivetrainPanel panel)
    {
        if (selectedModel == null)
            throw new Exception("Model has to be selected");

        base.Show(selectedModel, panel);

        List<string> gearboxes = new List<string>();
        gearboxes.Add("None");
        selectedModel.GearboxMeta.ForEach(x => gearboxes.Add(x.Name));

        Left_Gearbox.AddOptions(gearboxes);
        Right_Gearbox.AddOptions(gearboxes);
        Left_Gearbox.onValueChanged.AddListener(x =>
        {
            panel.Meta.SelectedGearboxes[0] = selectedModel.GearboxMeta.Find(
                y => y.Name == Left_Gearbox.options[x].text);
        });
        Right_Gearbox.onValueChanged.AddListener(x =>
        {
            panel.Meta.SelectedGearboxes[1] = selectedModel.GearboxMeta.Find(
                y => y.Name == Right_Gearbox.options[x].text);
        });

        if (panel.Meta.Type == DrivetrainType.Arcade && panel.Meta.SelectedGearboxes.Length == 2)
        {
            Debug.Log("Loading selections");
            Left_Gearbox.value = Left_Gearbox.options.FindIndex(x => x.text == panel.Meta.SelectedGearboxes[0].Name);
            Right_Gearbox.value = Right_Gearbox.options.FindIndex(x => x.text == panel.Meta.SelectedGearboxes[1].Name);
        }
        else
        {
            panel.Meta.Type = DrivetrainType.Arcade;
            panel.Meta.SelectedGearboxes = new GearboxData[2];
        }
    }

    public override void Hide()
    {
        base.Hide();
    }
}
