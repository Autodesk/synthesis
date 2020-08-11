using UnityEngine;
using UnityEngine.UIElements;

public class ResourceLedger : MonoBehaviour
{
    public string[] Keys;
    public VisualTreeAsset[] Values;

    public void OnEnable()
    {
        if(Instance == null)
            Instance = this;
    }

    public static ResourceLedger Instance { get; private set; } = null;
}
