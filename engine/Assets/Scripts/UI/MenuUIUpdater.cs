using Synthesis.UI.Dynamic;
using UnityEngine;

namespace UI
{
    public class MenuUIUpdater : MonoBehaviour
    {
        void Update()
        {
            DynamicUIManager.Update();
        }
    }
}
