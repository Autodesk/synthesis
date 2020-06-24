using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.UIElements.Runtime;
using UnityEngine.UIElements;

public class DefaultUI : MonoBehaviour
{
    public PanelRenderer DefaultUIRenderer;

    private void OnEnable()
    {
        Debug.Log("Wow");
        //DefaultUIRenderer.postUxmlReload = BindDefaultUI;
    }

    private IEnumerable<Object> BindDefaultUI()
    {
        VisualElement root = DefaultUIRenderer.visualTree;

        Button exitButton = root.Q<Button>(name: "exit-button");
        if (exitButton != null)
        {
            exitButton.clickable.clicked += () =>
            {
#if UNITY_EDITOR
                Debug.Break(); // Doesn't exit, just pauses. Here just for testing the button
#else
                Application.Quit();
#endif
            };
        }

        return null;
    }
}
