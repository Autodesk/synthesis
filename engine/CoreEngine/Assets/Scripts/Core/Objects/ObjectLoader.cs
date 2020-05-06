using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.Core.Object {

  public class ObjectLoader : MonoBehaviour {

    public static ObjectLoader Instance { get; private set; }

    private void Awake() {
      Instance = this;
    }

    public void CreateObj(string name) {
      GameObject g = new GameObject(name);
    }

  }

}