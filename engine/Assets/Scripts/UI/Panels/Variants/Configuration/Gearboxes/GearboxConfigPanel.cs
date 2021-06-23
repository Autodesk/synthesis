using Synthesis.ModelManager;
using Synthesis.ModelManager.Models;
using Synthesis.UI.Panels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GearboxConfigPanel : Panel
{
    public Transform list;
    public GameObject GearboxItem;
    public TMP_Dropdown ModelDropdown;
    public Model SelectedModel = null;

    private List<GearboxItem> GearboxItems;

    private void Start()
    {
        GearboxItems = new List<GearboxItem>();

        PopulateDropdown();
        if (ModelDropdown.options.Count > 0)
            SelectedModel = ModelManager.Models[ModelDropdown.options[ModelDropdown.value].text];

        ModelDropdown.onValueChanged.AddListener(x =>
        {
            var prev = SelectedModel;
            SelectedModel = ModelManager.Models[ModelDropdown.options[ModelDropdown.value].text];
            EvaluateGearboxes(prev);
        });

        if (SelectedModel != null)
            LoadFrom(SelectedModel);
    }

    public void EvaluateGearboxes(Model previousModel = null)
    {
        // Save gearbox data
        if (previousModel != null)
            SaveTo(previousModel);

        // Clear gearboxes
        GearboxItems.RemoveAll(x =>
        {
            Destroy(x.gameObject);
            return true;
        });

        // Repopulate with currently selected model
        if (SelectedModel != null)
            LoadFrom(SelectedModel);
    }

    public void AddGearboxItem() => AddGearboxItem(null);

    public void AddGearboxItem(GearboxData? data = null)
    {
        if (SelectedModel == null)
            return; // Not really expecting this but just in case

        var gearbox = Instantiate(GearboxItem, list.transform);
        if (data.HasValue)
            gearbox.GetComponent<GearboxItem>().Init(this, data.Value);
        else
            gearbox.GetComponent<GearboxItem>().Init(this);
        // gearbox.transform.SetParent(list.transform);
        GearboxItems.Add(gearbox.GetComponent<GearboxItem>());
    }

    public void RemoveGearboxItem(GearboxItem item)
    {
        var a = GearboxItems.Find(x => x == item);

        if (a == null)
            throw new Exception("Can't find GearboxItem");

        GearboxItems.Remove(a);
        Destroy(a.gameObject);
    }

    private void PopulateDropdown()
    {
        var options = new List<string>();
        options.AddRange(ModelManager.Models.Keys);
        ModelDropdown.AddOptions(options);
    }

    private void SaveTo(Model model)
    {
        model.GearboxMeta.Clear();
        GearboxItems.ForEach(x => model.GearboxMeta.Add(x.Data));
    }

    private void LoadFrom(Model model) => model.GearboxMeta.ForEach(x => AddGearboxItem(x));

    public List<GearboxData> CompileData()
    {
        List<GearboxData> data = new List<GearboxData>();
        GearboxItems.ForEach(x => data.Add(x.Data));
        return data;
    }

    public override void Close()
    {
        if (SelectedModel != null)
            SaveTo(SelectedModel);

        base.Close();
    }
}
