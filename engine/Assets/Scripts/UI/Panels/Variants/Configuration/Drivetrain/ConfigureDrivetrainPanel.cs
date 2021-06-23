using Synthesis.ModelManager;
using Synthesis.ModelManager.Models;
using Synthesis.UI.Panels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using DTT = Synthesis.ModelManager.Models.DrivetrainType;

public class ConfigureDrivetrainPanel : Panel
{
    public TMP_Dropdown ModelDropdown;
    public TMP_Dropdown DrivetrainType;

    public List<DrivetrainSubPanel> typePanels;

    public DrivetrainSubPanel SelectedSubPanel;
    public Model SelectedModel = null;
    public DrivetrainMeta Meta;

    private void Start()
    {
        // Model Select
        var options = new List<string>();
        options.AddRange(ModelManager.Models.Keys);
        ModelDropdown.AddOptions(options);
        ModelDropdown.onValueChanged.AddListener(x =>
        {
            SelectedSubPanel.Hide();

            SelectedModel = ModelManager.Models[ModelDropdown.options[
                ModelDropdown.value].text];

            Meta = SelectedModel.DrivetrainMeta;
            ShowSelectedSubPanel();
            // SelectedSubPanel.Show(SelectedModel, this);
        });
        if (ModelDropdown.options.Count > 0)
        {
            SelectedModel = ModelManager.Models[ModelDropdown.options[
                ModelDropdown.value].text];
            Meta = SelectedModel.DrivetrainMeta;
        }

        // Drivetrain Type Select
        List<string> names = new List<string>();
        typePanels.ForEach(x => names.Add(x.name));
        DrivetrainType.AddOptions(names);
        DrivetrainType.onValueChanged.AddListener(x =>
        {
            // typePanels.ForEach(y => y.gameObject.SetActive(false));
            if (SelectedSubPanel != null)
                SelectedSubPanel.Hide();
            SelectedSubPanel = typePanels[x];

            ShowSelectedSubPanel();
        });
        ShowSelectedSubPanel();
    }

    public void ShowSelectedSubPanel()
    {
        if (Meta.Type != DTT.NotSelected)
        {
            SelectedSubPanel = typePanels.Find(x => x.name == Enum.GetName(typeof(DTT), Meta.Type));
            if (SelectedSubPanel == null)
                throw new Exception("Couldn't load selected sub panel");
        }
        else
        {
            SelectedSubPanel = typePanels[DrivetrainType.value];
        }
        if (SelectedModel != null)
            SelectedSubPanel.Show(SelectedModel, this);
    }

    public override void Close()
    {
        if (Meta.Type != DTT.NotSelected && SelectedModel != null)
            SelectedModel.DrivetrainMeta = Meta;

        base.Close();
    }
}
