using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Digital;
using SynthesisAPI.InputManager.Events;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;
using Entity = System.UInt32;

namespace SynthesisCore
{
    [ModuleExport]
    public class SampleSystem : SystemBase
    {
        private Selectable selectable;
        private Transform transform;

        public override void OnPhysicsUpdate() { }

        public override void Setup()
        {
            Entity e = EnvironmentManager.AddEntity();
            transform = e.AddComponent<Transform>();
            selectable = e.AddComponent<Selectable>();
            Mesh m = e.AddComponent<Mesh>();
            cube(m);

            InputManager.AssignDigital("forwards", (KeyDigital)"W", Move);
            InputManager.AssignDigital("x", (KeyDigital)"X", Move);
            InputManager.AssignDigital("y", (KeyDigital)"Y", Move);
            InputManager.AssignDigital("z", (KeyDigital)"Z", Move);
        }

        public override void OnUpdate() {
            if (selectable.IsSelected)
            {
                ApiProvider.Log("Selected");
            }
        }

        private void cube(Mesh m)
        {
            m.Vertices = new List<Vector3>()
            {
                new Vector3(0,0,0),
                new Vector3(1,0,0),
                new Vector3(1,1,0),
                new Vector3(0,1,0),
                new Vector3(0,1,1),
                new Vector3(1,1,1),
                new Vector3(1,0,1),
                new Vector3(0,0,1)
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
        }

        public void Move(IEvent e)
        {
            if(e is DigitalStateEvent digitalStateEvent && digitalStateEvent.KeyState == DigitalState.Down)
            {
                if (digitalStateEvent.Name == "forwards")
                {
                    //transform.Position += new Vector3D(0.1, 0, 0);
                    // transform.Rotate(new Vector3D(30, 0, 0));
                }
                else if (digitalStateEvent.Name == "x")
                {
                    transform.Rotate(UnitVector3D.XAxis, Angle.FromDegrees(30));
                }
                else if (digitalStateEvent.Name == "y")
                {
                    transform.Rotate(UnitVector3D.YAxis, Angle.FromDegrees(30));
                }
                else if (digitalStateEvent.Name == "z")
                {
                    transform.Rotate(UnitVector3D.ZAxis, Angle.FromDegrees(30));
                }

            }
        }
    }
}
