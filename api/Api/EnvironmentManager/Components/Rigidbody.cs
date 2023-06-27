using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#nullable enable

namespace SynthesisAPI.EnvironmentManager.Components
{
    public class Rigidbody : Component
    {
        internal object? Adapter = null;

        #region Properties

        public delegate void CollisionFeedback(float magn);
        public CollisionFeedback OnEnterCollision = m => { };

        public event PropertyChangedEventHandler? PropertyChanged;

        public string ExportedJointUuid { get; set; } = string.Empty;

        internal bool useGravity = true;
        public bool UseGravity {
            get => useGravity;
            set {
                useGravity = value;
                OnPropertyChanged();
            }
        }
        internal bool isKinematic = false;
        /// <summary>
        /// Essentially toggle for physics. When Kinematic physical forces won't apply to this body
        /// </summary>
        public bool IsKinematic {
            get => isKinematic;
            set {
                isKinematic = value;
                OnPropertyChanged();
            }
        }
        internal float mass = 1.0f;
        /// <summary>
        /// Mass of body in kilograms
        /// </summary>
        public float Mass {
            get => mass;
            set {
                mass = value < 0.0001f ? 0.0001f : value;
                OnPropertyChanged();
            }
        }
        internal Vector3D velocity = new Vector3D(0, 0, 0);
        /// <summary>
        /// Velocity of body in meters/second
        /// </summary>
        public Vector3D Velocity {
            get => velocity;
            set {
                velocity = value; // TODO: Validate?
                OnPropertyChanged();
            }
        }
        internal float drag = 0.0f;
        /// <summary>
        /// Linear drag coefficent of the body
        /// </summary>
        public float Drag {
            get => drag;
            set {
                drag = value; // TODO: Validate?
                OnPropertyChanged();
            }
        }
        internal Vector3D angularVelocity = new Vector3D(0, 0, 0);
        /// <summary>
        /// Angular velocity of the body in radians/second
        /// </summary>
        public Vector3D AngularVelocity {
            get => angularVelocity;
            set {
                angularVelocity = value; // TODO: Validate?
                OnPropertyChanged();
            }
        }
        internal float angularDrag = 0.05f;
        /// <summary>
        /// Angular drag coefficent of the body
        /// </summary>
        public float AngularDrag {
            get => angularDrag;
            set {
                angularDrag = value; // TODO: Validate?
                OnPropertyChanged();
            }
        }
        internal float maxAngularVelocity = 7.0f;
        public float MaxAngularVelocity {
            get => maxAngularVelocity;
            set {
                maxAngularVelocity = value > 0.0f ? value : 0.0f;
                OnPropertyChanged();
            }
        }
        internal float maxDepenetrationVelocity = 30.0f; // I don't know what the default is
        public float MaxDepenetrationVelocity {
            get => maxDepenetrationVelocity;
            set {
                maxDepenetrationVelocity = value; // TODO: Validate?
                OnPropertyChanged();
            }
        }
        internal CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.Discrete;
        /// <summary>
        /// 
        /// </summary>
        public CollisionDetectionMode CollisionDetectionMode {
            get => collisionDetectionMode;
            set {
                collisionDetectionMode = value;
                OnPropertyChanged();
            }
        }
        internal RigidbodyConstraints constraints = RigidbodyConstraints.None;
        public RigidbodyConstraints Constraints {
            get => constraints;
            set {
                constraints = value;
                OnPropertyChanged();
            }
        }
        internal bool detectCollisions = true;
        public bool DetectCollisions
        {
            get => detectCollisions;
            set
            {
                detectCollisions = value;
                OnPropertyChanged();
            }
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

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
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

    [Flags]
    public enum RigidbodyConstraints
    {
        None = 0,
        FreezePositionX = 2,
        FreezePositionY = 4,
        FreezePositionZ = 8,
        FreezeRotationX = 0x10,
        FreezeRotationY = 0x20,
        FreezeRotationZ = 0x40,
        FreezePosition = 14,
        FreezeRotation = 112,
        FreezeAll = 126
    }

    #endregion
}
