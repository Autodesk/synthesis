using UnityEngine;
using Synthesis.ModelManager;
using Synthesis.UI.Panels;
using TMPro;
using UnityEngine.UI;

public class AddItem : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _name;

    public Image background;

    private string _fullPath;
    GameObject p;
    private bool _isRobot;

    public void AddModel()
    {
        ModelManager.AddModel(_fullPath);
    }

    public void AddField()
    {
        ModelManager.SetField(_fullPath);
    }

    public void Init(string name, string path)
    {
        _name.text = name;
        _fullPath = path;
        p = GameObject.Find("Tester");
    }

    public void Darken()
    {
        background.color = new Color(0.972549f, 0.9568627f, 0.9568627f);
    }
    public void Lighten()
    {
        background.color = Color.white;
    }

}
