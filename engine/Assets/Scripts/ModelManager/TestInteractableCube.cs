using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.UI;
using Synthesis.Attributes;

public class TestInteractableCube : InteractableObject
{
    private void Start() {
        base.ContextMenuUID = "Cube";
    }

    [ContextMenuOption("Delete")]
    public void Delete() {
        Destroy(gameObject);
    }
}
