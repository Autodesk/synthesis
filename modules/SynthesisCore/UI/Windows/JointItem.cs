using MathNet.Spatial.Euclidean;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.UIManager.VisualElements;
using SynthesisAPI.Utilities;
using SynthesisCore.Systems;
using System.Collections.Generic;

namespace SynthesisCore.UI
{
    public class JointItem
    {
        public VisualElement JointElement { get; }

        public JointItem(VisualElementAsset jointAsset, IJoint joint)
        {
            JointElement = jointAsset.GetElement("joint");
            // HighlightJoint();
        }

        private void HighlightJoint()
        {
            Entity jointTest = EnvironmentManager.AddEntity();
            Bundle b = new Bundle();

            Transform t = new Transform();
            t.Position = new Vector3D(10, 10, 10); //joint anchor position
            CameraController.Instance.SetNewFocus(t.Position, true);
            b.Components.Add(t);
            b.Components.Add(cube()); //replace the cube function with your mesh creation
            jointTest.AddBundle(b);
            //when done
            jointTest.RemoveEntity();
        }

        private Mesh cube()
        {
            Mesh m = new Mesh();
            if (m == null)
                m = new Mesh();

            m.Vertices = new List<Vector3D>()
            {
                new Vector3D(-0.5,-0.5,-0.5),
                new Vector3D(0.5,-0.5,-0.5),
                new Vector3D(0.5,0.5,-0.5),
                new Vector3D(-0.5,0.5,-0.5),
                new Vector3D(-0.5,0.5,0.5),
                new Vector3D(0.5,0.5,0.5),
                new Vector3D(0.5,-0.5,0.5),
                new Vector3D(-0.5,-0.5,0.5)
            };
            m.Triangles = new List<int>()
            {
                0, 2, 1, //face front
			    0, 3, 2,
                2, 3, 4, //face top
			    2, 4, 5,
                1, 2, 5, //face right
			    1, 5, 6,
                0, 7, 4, //face left
			    0, 4, 3,
                5, 4, 7, //face back
			    5, 7, 6,
                0, 6, 7, //face bottom
			    0, 1, 6
            };
            return m;
        }
    }

}