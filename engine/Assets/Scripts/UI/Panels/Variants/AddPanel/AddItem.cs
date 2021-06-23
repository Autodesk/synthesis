using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Synthesis.ModelManager;
using Synthesis.UI.Panels;
using TMPro;

public class AddItem : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _name;

    private string _fullPath;
    private Panel _parentPanel;

    public void AddModel()
    {
        ModelManager.AddModel(_fullPath);
        _parentPanel.Close();
    }

    public void AddField()
    {
        ModelManager.SetField(_fullPath);
        _parentPanel.Close();
    }

    public void Init(string name, string path, Panel parentPanel)
    {
        _name.text = name;
        _fullPath = path;
        _parentPanel = parentPanel;
    }
}
