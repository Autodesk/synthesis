using UnityEngine;
using Synthesis.ModelManager;
using Synthesis.UI.Panels;
using TMPro;

public class AddItem : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _name;

    private string _fullPath;
    private Panel _parentPanel;
    GameObject p;
    private bool _isRobot;

    public void AddModel()
    {
        //ModelManager.AddModel(_fullPath);//importer
        // Debug.Log("TEST ADD MODEL");
        p.GetComponent<PTL>().SpawnRobot(_fullPath);
        _parentPanel.Close();
    }

    public void AddField()
    {
        Debug.Log("TEST FIELD");
        p.GetComponent<PTL>().SpawnField(_fullPath);
        _parentPanel.Close();
    }

    public void Init(string name, string path, Panel parentPanel)
    {
        _name.text = name;
        _fullPath = path;
        _parentPanel = parentPanel;
        p = GameObject.Find("Tester");
    }
}
