using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourceLedger : MonoBehaviour
{
    public string[] Keys;
    public VisualTreeAsset[] Values;

    private void Awake()
    {
        Instance = this;
    }
    
    public static ResourceLedger Instance { get; private set; }
}
