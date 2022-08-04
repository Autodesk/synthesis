using System.Collections;
using System.Collections.Generic;
using Synthesis.UI.Dynamic;
using UnityEngine;

public class UnityModalSpawner : MonoBehaviour {
    public void Exit() {
        DynamicUIManager.CreateModal<ExitSynthesisModal>();
    }
}
