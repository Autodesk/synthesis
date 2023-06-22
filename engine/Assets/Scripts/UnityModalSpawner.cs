using Synthesis.UI.Dynamic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityModalSpawner : MonoBehaviour {
    public void Exit() {
        DynamicUIManager.CreateModal<ExitSynthesisModal>();
    }
}
