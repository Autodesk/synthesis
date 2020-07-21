using System;
using System.Collections.Generic;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.InputEvents;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;
using Entity = System.UInt32;

namespace SynthesisCore
{
    [ModuleExport]
    public class SampleSystem : SystemBase
    {
        bool start = true;

        public override void OnPhysicsUpdate() { }

        public override void OnUpdate()
        {
            if (start)
            {
                Entity e = EnvironmentManager.AddEntity();
                Mesh m = e.AddComponent<Mesh>();
                cube(m);

                Input[] test = { new Digital("w"), new Digital("a"), new Digital("s"), new Digital("d") };
                InputManager.AssignInputsToEvent("move",test);

                Input[] test2 = { new Analog("Mouse X"), new Analog("Mouse Y") };
                InputManager.AssignInputsToEvent("mouse", test2);

                start = false;
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

        [TaggedCallback("input/move")]
        public void Move(DigitalEvent digitalEvent)
        {
            if(digitalEvent.State == DigitalState.Held)
            {
                switch (digitalEvent.Name)
                {
                    case "w":
                        ApiProvider.Log("w");
                        break;
                    case "a":
                        ApiProvider.Log("a");
                        break;
                    case "s":
                        ApiProvider.Log("s");
                        break;
                    case "d":
                        ApiProvider.Log("d");
                        break;
                    default:
                        break;
                }
            }
        }

        [TaggedCallback("input/mouse")]
        public void Mouse(AnalogEvent analogEvent)
        {
            switch (analogEvent.Name)
            {
                case "Mouse X":
                    ApiProvider.Log(analogEvent.Value);
                    break;
                case "Mouse Y":
                    ApiProvider.Log(analogEvent.Value);
                    break;
                default:
                    break;
            }
        }
    }
}
