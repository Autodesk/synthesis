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

        /*private bool _isKinematic = false;
        private float _mass = 1.0f;
        private float _drag = 0.0f;
        private float _angularDrag = 0.05f;
        private CollisionDetectionMode _collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;*/

        internal delegate void SetValue(string variableName, object o);
        internal delegate object GetValue(string variableName);

        // These delegates will be setup by the Adapter
        internal SetValue LinkedSetter = (n, o) => throw new Exception("Setter not assigned");
        internal GetValue LinkedGetter = n => throw new Exception("Getter not assigned");

        /// <summary>
        /// Essentially toggle for physics
        /// </summary>
        public bool IsKinematic {
            get => (bool)LinkedGetter("isKinematic");
            set => LinkedSetter("isKinematic", value);
        }
        /// <summary>
        /// Mass of body in kilograms
        /// </summary>
        public float Mass {
            get => (float)LinkedGetter("mass");
            set => LinkedSetter("mass", value < 0.001f ? 0.001f : value);
        }
        /// <summary>
        /// Velocity of body in meters/second
        /// </summary>
        public Vector3D Velocity {
            get => (Vector3D)LinkedGetter("velocity");
            set => LinkedSetter("velocity", value);
        }
        /// <summary>
        /// Linear drag coefficent of the body
        /// </summary>
        public float Drag {
            get => (float)LinkedGetter("drag");
            set => LinkedSetter("drag", value);
        }
        /// <summary>
        /// Angular velocity of the body in radians/second
        /// </summary>
        public Vector3D AngularVelocity {
            get => (Vector3D)LinkedGetter("angularVelocity");
            set => LinkedSetter("angularVelocity", value);
        }
        /// <summary>
        /// Angular drag coefficent of the body
        /// </summary>
        public float AngularDrag {
            get => (float)LinkedGetter("angularDrag");
            set => LinkedSetter("angularDrag", value);
        }
        public CollisionDetectionMode CollisionDetectionMode {
            get => (CollisionDetectionMode)LinkedGetter("collisionDetectionMode");
            set => LinkedSetter("collisionDetectionMode", value);
        }

        #endregion

        internal List<(Vector3D Force, Vector3D Position, bool Relative, ForceMode Mode)> AdditionalForces
            { get; private set; } = new List<(Vector3D Force, Vector3D Position, bool Relative, ForceMode Mode)>();
        internal List<(Vector3D Torque, bool Relative, ForceMode Mode)> AdditionalTorques
            { get; private set; } = new List<(Vector3D Torque, bool Relative, ForceMode Mode)>();

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
