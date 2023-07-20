using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SimObjects.MixAndMatch
{
    public class PartEditorPart
    {
        public string Name;
        public Transform Transform => _unityObject.transform;
        public GameObject UnityObject => _unityObject;
        
        public List<GameObject> ConnectionPoints = new();

        public PartEditorPart ConnectedPartEditorPart;

        private GameObject _unityObject;
        public string Folder = "Mira";

        public int Index;

        public PartEditorPart()
        {
            var root = ParsePath(Path.Combine("$appdata/Autodesk/Synthesis", Folder), '/');
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            var files = Directory.GetFiles(root).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();
            
            var partObject = MixAndMatchSimObject.CreatePartMesh(files[1]);

            _unityObject = partObject;
        }

        public static string ParsePath(string p, char c) {
            string[] a = p.Split(c);
            string b   = "";
            for (int i = 0; i < a.Length; i++) {
                switch (a[i]) {
                    case "$appdata":
                        b += System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                        break;
                    default:
                        b += a[i];
                        break;
                }
                if (i != a.Length - 1)
                    b += Path.AltDirectorySeparatorChar;
            }
            return b;
        }

        public PartEditorPart Duplicate()
        {
            var part = new PartEditorPart();
            
            ConnectionPoints.ForEach(p =>
            {
                part.AddConnectionPoint(p.transform.localPosition, p.transform.localRotation);
            });
            return part;
        }

        public MixAndMatchPartData ToPartData() {
            var connectionPoints = ConnectionPoints.Select(point =>
                (point.transform.localPosition, point.transform.forward)).ToArray();
            
            return new MixAndMatchPartData(Transform.localPosition, Transform.rotation, connectionPoints);
            
        }

        public void AddConnectionPoint() {
            AddConnectionPoint(Vector3.zero, Quaternion.identity);
        }

        public void AddConnectionPoint(Vector3 position, Quaternion rotation) {
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            point.transform.localScale = Vector3.one * .5f;
            point.transform.SetParent(Transform);
            point.layer = LayerMask.NameToLayer("ConnectionPoint");
            
            point.GetComponent<Collider>().enabled = false;
            point.GetComponent<Collider>().isTrigger = true;

            point.AddComponent<ConnectionPointReference>().part = this;
            
            point.transform.localPosition = position;
            point.transform.localRotation = rotation;
            
            ConnectionPoints.Add(point);
        }
    }

    public class ConnectionPointReference : MonoBehaviour {
        public PartEditorPart part;
    }
}