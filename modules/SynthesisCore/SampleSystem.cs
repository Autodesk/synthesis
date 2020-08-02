using System.Collections.Generic;
using MathNet.Spatial.Euclidean;
using SynthesisAPI.AssetManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.InputEvents;
using SynthesisAPI.InputManager.Inputs;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Runtime;
using SynthesisCore.Components;

namespace SynthesisCore
{
    [ModuleExport]
    public class SampleSystem : SystemBase
    {
        private Selectable selectable;
        private Transform transform;
        private Rigidbody rigidbody;

        private Entity A, B;

        public override void OnPhysicsUpdate() { }

        public override void Setup()
        {
            A = CreateTestEntity(new Vector3D(0, 2f, 0));
            B = CreateTestEntity(new Vector3D(0, 3.1f, 0));

            var rb = A.GetComponent<Rigidbody>();
            rb.Mass = 5;

            #region Hard-coded joints for demonstration

            var hingeJoint = B.AddComponent<HingeJoint>();
            hingeJoint.ConnectedBody = A.GetComponent<Rigidbody>();
            hingeJoint.Anchor = new Vector3D(0, -0.55f, 0);
            hingeJoint.Axis = new Vector3D(0, 1, 0);
            hingeJoint.UseMotor = true;
            hingeJoint.Motor = new JointMotor()
            {
                Force = 500,
                FreeSpin = false
            };

            #endregion

            Digital[] test = { new Digital("w"), new Digital("a"), new Digital("s"), new Digital("d") };
            InputManager.AssignDigitalInputs("move", test);
            InputManager.AssignDigitalInput("fly", new Digital("f"));
            InputManager.AssignDigitalInput("spin", new Digital("r"));
            InputManager.AssignDigitalInput("break", new Digital("x"));
            InputManager.AssignDigitalInput("power_fwd", new Digital("i"));
            InputManager.AssignDigitalInput("power_rwd", new Digital("k"));
        }

        private static uint nextChannel = 5;
        private Entity CreateTestEntity(Vector3D pos)
        {
            var physMat = new PhysicsMaterial();
            // physMat.Bounciness = 1.0f;
            // physMat.DynamicFriction = 0.05f;
            // physMat.BounceCombine = PhysicMaterialCombine.Maximum;

            Entity e = EnvironmentManager.AddEntity();
            transform = e.AddComponent<Transform>();
            transform.Position = pos;
            Mesh m = e.AddComponent<Mesh>();
            cube(m);

            var audio = new AudioSource();
            audio.AudioClip = AssetManager.GetAsset<AudioClipAsset>("/modules/synthesis_core/hit-sound-effect.wav");
            audio.IsPlaying = false;
            e.AddComponent(audio);

            var audio = e.AddComponent<AudioSource>();
            audio.AudioClip = AssetManager.GetAsset<AudioClipAsset>("/modules/synthesis_core/hit-sound-effect.wav");
            audio.IsPlaying = false;

            var collider = e.AddComponent<MeshCollider>();
            collider.Material = physMat;
            selectable = e.AddComponent<Selectable>();
            e.AddComponent<Moveable>().Channel = nextChannel;
            nextChannel++;

            rigidbody = e.AddComponent<Rigidbody>();
            // rigidbody.Velocity = new Vector3D(10, 0, 0);
            // rigidbody.useGravity = false;
            rigidbody.Mass = 1.0f;
            rigidbody.MaxAngularVelocity = 400.0f;
            rigidbody.Drag = 0.1f;

            rigidbody.OnEnterCollision += magnitude =>
            {
                ApiProvider.Log(magnitude, LogLevel.Info);
                var volume = (magnitude - 2.0) / 22.0;
                volume += 0.2;
                if (volume < 0) volume = 0;
                if (volume > 1) volume = 1;
                audio.Volume = (float)volume;
                audio.IsPlaying = true;
            };

            return e;
        }

        public override void OnUpdate() { }

        private void cube(Mesh m)
        {
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
        }

        [TaggedCallback("input/fly")]
        public void Fly(DigitalEvent de)
        {
            if (de.State == DigitalState.Down)
            {
                // ApiProvider.Log("Key Pressed");
                var rb = Selectable.Selected?.Entity?.GetComponent<Rigidbody>();
                rb.Velocity = new Vector3D(0, 5, 0);
            }
        }

        [TaggedCallback("input/spin")]
        public void Spin(DigitalEvent de)
        {
            if (de.State == DigitalState.Down)
            {
                // ApiProvider.Log("Key Pressed");
                Selectable.Selected?.Entity?.GetComponent<Rigidbody>().AddTorque(new Vector3D(350, 0, 0));
            }
        }

        [TaggedCallback("input/break")]
        public void Break(DigitalEvent de)
        {
            if (de.State == DigitalState.Down)
            {
                // ApiProvider.Log("Key Pressed");
                B.GetComponent<FixedJoint>().BreakForce = 0f;
            }
        }

        [TaggedCallback("input/power_fwd")]
        public void SpinFwd(DigitalEvent de)
        {
            if (de.State == DigitalState.Down)
            {
                ApiProvider.Log("Spin?", LogLevel.Debug);
                var j = Selectable.Selected?.Entity?.GetComponent<HingeJoint>();
                if (j != null)
                {
                    j.UseMotor = true;
                    var m = j.Motor;
                    m.TargetVelocity = 100;
                    j.Motor = m;
                    ApiProvider.Log("SPIN", LogLevel.Debug);
                }
            }
            else if (de.State == DigitalState.Up)
            {
                var j = Selectable.Selected?.Entity?.GetComponent<HingeJoint>();
                if (j != null)
                {
                    j.UseMotor = false;
                    var m = j.Motor;
                    m.TargetVelocity = 0;
                    j.Motor = m;
                    ApiProvider.Log("Awwww", LogLevel.Debug);
                }
            }
        }

        [TaggedCallback("input/power_rwd")]
        public void SpinRwd(DigitalEvent de)
        {
            if (de.State == DigitalState.Down)
            {
                var j = Selectable.Selected?.Entity?.GetComponent<HingeJoint>();
                if (j != null)
                {
                    j.UseMotor = true;
                    var m = j.Motor;
                    m.TargetVelocity = -100;
                    j.Motor = m;
                }
            }
            else if (de.State == DigitalState.Up)
            {
                var j = Selectable.Selected?.Entity?.GetComponent<HingeJoint>();
                if (j != null)
                {
                    j.UseMotor = false;
                    var m = j.Motor;
                    m.TargetVelocity = 0;
                    j.Motor = m;
                }
            }
        }

        /*
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
        */
    }
}
