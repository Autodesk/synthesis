using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private VisualElement root;

    public UIManager()
    {

    }

    void OnEnable()
    {
        var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/Toolbar/Toolbar.uxml");
        root = asset.CloneTree();
        Debug.Log("Test");
    }

    // Start is called before the first frame update
    void Start()
    {
        Button button = root.Q<Button>(name: "test-button");
        button.clickable.clicked += () =>
        {
            Debug.Log("Test button has been clicked");
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AddTab()
    {

    }

    private void RemoveTab()
    {
        
    }

}
