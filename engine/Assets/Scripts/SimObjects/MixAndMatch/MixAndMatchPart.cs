using System.Collections.Generic;
using UnityEngine;

namespace SimObjects.MixAndMatch
{
    public class MixAndMatchPart
    {
        public string Name;
        public Transform Transform => _unityObject.transform;
        public List<GameObject> SnapPoints = new();

        private GameObject _unityObject;

        public MixAndMatchPart()
        {
            _unityObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject.Destroy(_unityObject.GetComponent<Collider>());
        }
    }
}