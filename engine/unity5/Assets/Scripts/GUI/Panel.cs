using Synthesis.Input;
using Synthesis.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour
{
    private GameObject panelParent;
    // Use this for initialization
    void Start()
    {
        panelParent = Auxiliary.FindGameObject("Panels");
    }
    // Update is called once per frame
    void Update()
    {

    }
    /// <summary>
    /// Toggles panel on and off
    /// </summary>
    public void Toggle(GameObject panel)
    {
        if (!panel.activeSelf) CloseAll();
        panel.SetActive(!panel.activeSelf);
    }
    public void OpenOnly(GameObject panel)
    {
        if (!panel.activeSelf) CloseAll();
        panel.SetActive(true);
    }
    public void OpenOver(GameObject panel)
    {
        panel.SetActive(true);
    }
    public void Close(GameObject panel)
    {
        panel.SetActive(false);
    }
    private void DeepClose(GameObject panel, int depth)
    {
        if (depth > 0)
            foreach (Transform t in panel.transform)
                DeepClose(t.gameObject, depth--);
        else
            Close(panel);
    }
    public void Freeze(bool freeze)
    {
        FreezeCamera(freeze);
        FreezeControls(freeze);
    }
    public void FreezeCamera(bool freeze)
    {
        DynamicCamera.ControlEnabled = !freeze;
    }
    public void FreezeControls(bool freeze)
    {
        InputControl.freeze = freeze;
    }
    private void CloseAll()
    {
        FreezeCamera(false);
        FreezeControls(false);
        DeepClose(panelParent, 1);
    }
    private void CloseAllExceptions()
    {

    }
}
