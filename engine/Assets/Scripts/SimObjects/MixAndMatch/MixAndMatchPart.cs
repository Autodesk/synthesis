using System.Collections.Generic;
using System.IO;
using System.Linq;
using Synthesis.UI;
using Synthesis.UI.Dynamic;
using UnityEngine;

namespace SimObjects.MixAndMatch
{
    public class MixAndMatchPart
    {
        public string Name;
        public Transform Transform => _unityObject.transform;
        public List<GameObject> SnapPoints = new();

        private GameObject _unityObject;
        
        public string Folder = "Mira";

        public MixAndMatchPart()
        {
            var root = ParsePath(Path.Combine("$appdata/Autodesk/Synthesis", Folder), '/');
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            var files = Directory.GetFiles(root).Where(x => Path.GetExtension(x).Equals(".mira")).ToArray();
            
            RobotSimObject.SpawnRobot(files[1], false);
            //RobotSimObject.GetCurrentlyPossessedRobot().Freeze();

            _unityObject = RobotSimObject.GetCurrentlyPossessedRobot().RobotNode;
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
                    b += System.IO.Path.AltDirectorySeparatorChar;
            }
            // Debug.Log(b);
            return b;
        }

        public MixAndMatchPart Duplicate()
        {
            var part = new MixAndMatchPart();
            
            SnapPoints.ForEach(p =>
            {
                GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                point.GetComponent<Collider>().isTrigger = true;
                point.transform.localScale = Vector3.one * .5f;
                point.transform.SetParent(part.Transform);
                point.layer = LayerMask.NameToLayer("SnapPoint");
                point.transform.localPosition = p.transform.localPosition;
                point.transform.localRotation = p.transform.localRotation;
                part.SnapPoints.Add(point);
            });
            return part;
        }
    }
}