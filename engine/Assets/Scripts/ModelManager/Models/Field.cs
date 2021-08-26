using System;
using Synthesis.ModelManager;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Synthesis.ModelManager.Models
{
    public class Field : Model
    {
        private GameObject _fieldObject;

        public Field(string filePath, GameObject p = null)
        {
            if (p == null)
                p = GameObject.Find("Tester");
            _fieldObject = p.GetComponent<PTL>().SpawnField(filePath);
        }

        public void Destroy()
        {
            Object.Destroy(_fieldObject);
        }
    }
}
