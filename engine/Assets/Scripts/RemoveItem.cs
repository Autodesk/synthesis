/*
using UnityEngine;
using Synthesis.ModelManager;
using Synthesis.UI.Panels;
using TMPro;

public class RemoveItem : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _name;

    private string _fullPath;
    private Panel _parentPanel;
    GameObject p;
    private bool _isRobot;

    public void RemoveModel()
    {
        //ModelManager.AddModel(_fullPath);//importer
        Debug.Log("TEST REMOVE MODEL");
        p.GetComponent<PTL>().RemoveRobot(_fullPath);
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
*/
using UnityEngine;
using Synthesis.ModelManager;
using Synthesis.UI.Panels;
using TMPro;

// add all the robots that are spawned into a list
// the issue is that the script doesn't know what to remove
public class RemoveItem : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _name;

    private string _fullPath;
    private Panel _parentPanel;
    
    [SerializeField]
    GameObject p;

    private bool _isRobot;

    public void Start()
    {
        Instantiate(p);
    }
    public void AddModel()
    {
        //ModelManager.AddModel(_fullPath);//importer
        Debug.Log("TEST ADD MODEL");
        p.GetComponent<PTL>().SpawnRobot(_fullPath);
        _parentPanel.Close();
    }

    public void RemoveModel()
    {
        //ModelManager.AddModel(_fullPath);//importer
        Debug.Log("TEST REMOVE MODEL");
       // p.GetComponent<PTL>().RemoveRobot(_fullPath);
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
