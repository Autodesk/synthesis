using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.Core.Object {

  public class ObjectManagment : MonoBehaviour {

    public ObjectManagment Instance { get; private set; }

    internal List<ISpawnable> SpawnedObjects = new List<ISpawnable>();

    private void Awake() {
      Instance = this;
    }

    

  }

}