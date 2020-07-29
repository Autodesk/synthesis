using MathNet.Spatial.Euclidean;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.Modules.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.EnvironmentManager.Components
{
    [BuiltinComponent]
    public class Rigidbody : Component
    {
        #region Properties

        // These delegates will be setup by the Adapter
        internal Action<string, object> LinkedSetter = (n, o) => throw new Exception("Setter not assigned");
        internal Func<string, object> LinkedGetter = n => throw new Exception("Getter not assigned");

        private void Set(string name, object obj) => LinkedSetter(name, obj);
        private T Get<T>(string name) => (T)LinkedGetter(name);

        public bool useGravity {
            get => Get<bool>("useGravity");
            set => Set("useGravity", value);
        }
        /// <summary>
        /// Essentially toggle for physics. When Kinematic physical forces won't apply to this body
        /// </summary>
        public bool IsKinematic {
            get => Get<bool>("isKinematic");
            set => Set("isKinematic", value);
        }
        /// <summary>
        /// Mass of body in kilograms
        /// </summary>
        public float Mass {
            get => Get<float>("mass");
            set => Set("mass", value < 0.001f ? 0.001f : value);
        }
        /// <summary>
        /// Velocity of body in meters/second
        /// </summary>
        public Vector3D Velocity {
            get => Get<Vector3D>("velocity");
            set => Set("velocity", value);
        }
        /// <summary>
        /// Linear drag coefficent of the body
        /// </summary>
        public float Drag {
            get => Get<float>("drag");
            set => Set("drag", value);
        }
        /// <summary>
        /// Angular velocity of the body in radians/second
        /// </summary>
        public Vector3D AngularVelocity {
            get => Get<Vector3D>("angularVelocity");
            set => Set("angularVelocity", value);
        }
        /// <summary>
        /// Angular drag coefficent of the body
        /// </summary>
        public float AngularDrag {
            get => Get<float>("angularDrag");
            set => Set("angularDrag", value);
        }
        public float MaxAngularVelocity {
            get => Get<float>("maxangularvelocity");
            set => Set("maxangularvelocity", value);
        }
        public float MaxDepenetrationVelocity {
            get => Get<float>("maxdepenetrationvelocity");
            set => Set("maxdepenetrationvelocity", value);
        }
        /// <summary>
        /// 
        /// </summary>
        public CollisionDetectionMode CollisionDetectionMode {
            get => Get<CollisionDetectionMode>("collisionDetectionMode");
            set => Set("collisionDetectionMode", value);
        }

        #endregion

        internal List<(Vector3D Force, Vector3D Position, bool Relative, ForceMode Mode)> AdditionalForces
            { get; private set; } = new List<(Vector3D Force, Vector3D Position, bool Relative, ForceMode Mode)>();
        internal List<(Vector3D Torque, bool Relative, ForceMode Mode)> AdditionalTorques
            { get; private set; } = new List<(Vector3D Torque, bool Relative, ForceMode Mode)>();

        /// <summary>
        /// Add force to body
        /// </summary>
        /// <param name="force">Force in Newtons * DeltaT</param>
        /// <param name="relative"></param>
        /// <param name="mode"></param>
        public void AddForce(Vector3D force, bool relative = false, ForceMode mode = ForceMode.Force)
        {
            AdditionalForces.Add((force, Vector3D.NaN, relative, mode));
            Changed = true;
        }

        public void AddForce(Vector3D force, Vector3D position, ForceMode mode = ForceMode.Force)
        {
            AdditionalForces.Add((force, position, false, mode));
            Changed = true;
        }

        public void AddTorque(Vector3D torque, bool relative = false, ForceMode mode = ForceMode.Force)
        {
            AdditionalTorques.Add((torque, relative, mode));
            Changed = true;
        }

        internal bool Changed { get; private set; } = true;
        internal void ProcessedChanges() => Changed = false;

        private TResult ConvertEnum<TResult>(object i) => (TResult)Enum.Parse(typeof(TResult), i.ToString(), true);
    }

    #region Enums

    public enum CollisionDetectionMode
    {
        Discrete, Continuous, ContinuousDynamic,
        ContinuousSpeculative
    }

    public enum ForceMode
    {
        Force = 0, Acceleration = 5,
        Impulse = 1, VelocityChange = 2
    }

    #endregion
}
