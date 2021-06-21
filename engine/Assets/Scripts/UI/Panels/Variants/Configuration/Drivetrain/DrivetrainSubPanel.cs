using Synthesis.ModelManager.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrivetrainSubPanel : MonoBehaviour
{
    public bool IsShown { get; set; }
    protected Model selectedModel { get; set; }
    protected ConfigureDrivetrainPanel panel { get; set; }

    public virtual void Show(Model selectedModel, ConfigureDrivetrainPanel panel) {
        IsShown = true;
        this.selectedModel = selectedModel;
        this.panel = panel;
        gameObject.SetActive(true);
    }
    public virtual void Hide() {
        IsShown = false;
        this.selectedModel = null;
        gameObject.SetActive(false);
    }
}
