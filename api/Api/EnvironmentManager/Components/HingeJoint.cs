using MathNet.Spatial.Euclidean;
using System.ComponentModel;
using System.Runtime.CompilerServices;

#nullable enable

namespace SynthesisAPI.EnvironmentManager.Components
{
    public class HingeJoint : IJoint
    {
        #region Properties
        //TODO: Add parent and child body connections

        internal Vector3D anchor = new Vector3D(0, 0, 0);
        public Vector3D Anchor {
            get => anchor;
            set {
                anchor = value;
                OnPropertyChanged();
            }
        }
        internal Vector3D axis = new Vector3D(1, 0, 0);
        public Vector3D Axis {
            get => axis;
            set {
                axis = value;
                OnPropertyChanged();
            }
        }
        internal float breakForce = float.PositiveInfinity;
        public float BreakForce {
            get => breakForce;
            set {
                breakForce = value < 0.0f ? 0.0f : value;
                OnPropertyChanged();
            }
        }
        internal float breakTorque = float.PositiveInfinity;
        public float BreakTorque {
            get => breakTorque;
            set {
                breakTorque = value < 0.0f ? 0.0f : value;
                OnPropertyChanged();
            }
        }
        internal Rigidbody? connectedParent = null;
        public Rigidbody? ConnectedParent {
            get => connectedParent;
            set {
                connectedParent = value;
                OnPropertyChanged();
            }
        }
        internal Rigidbody? connectedChild = null;
        public Rigidbody? ConnectedChild {
            get => connectedChild;
            set {
                connectedChild = value;
                OnPropertyChanged();
            }
        }
        internal bool enableCollision = false;
        public bool EnableCollision {
            get => enableCollision;
            set {
                enableCollision = value;
                OnPropertyChanged();
            }
        }
        internal bool useLimits = false;
        public bool UseLimits {
            get => useLimits;
            set {
                useLimits = value;
                OnPropertyChanged();
            }
        }
        internal JointLimits limits = new JointLimits();
        public JointLimits Limits {
            get => limits;
            set {
                limits = value;
                OnPropertyChanged();
            }
        }
        internal float velocity = 0.0f;
        public float Velocity {
            get => velocity;
        }
        internal float angle = 0.0f; 
        public float Angle {
            get => angle;
        }
        internal bool useMotor = false;
        public bool UseMotor {
            get => useMotor;
            set {
                useMotor = value;
                OnPropertyChanged();
            }
        }
        internal JointMotor motor = new JointMotor();
        public JointMotor Motor {
            get => motor;
            set {
                motor = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }

    #region Enums

    public enum ConfigurableJointMotion
    {
        Locked,
        Limited,
        Free
    }

    #endregion

    public class JointLimits
    {
        private UnityEngine.JointLimits _container;

        public float Min {
            get => _container.min;
            set => _container.min = value;
        }
        public float Max {
            get => _container.max;
            set => _container.max = value;
        }
        public float Bounciness {
            get => _container.bounciness;
            set => _container.bounciness = value;
        }
        public float BounceMinVelocity {
            get => _container.bounceMinVelocity;
            set => _container.bounceMinVelocity = value;
        }
        public float ContactDistance {
            get => _container.contactDistance;
            set => _container.contactDistance = value;
        }

        public JointLimits()
        {
            _container = new UnityEngine.JointLimits();
        }

        internal JointLimits(UnityEngine.JointLimits container)
        {
            _container = container;
        }

        internal UnityEngine.JointLimits GetUnity() => _container;
    }

    public class JointMotor
    {
        private UnityEngine.JointMotor _container;

        public float TargetVelocity {
            get => _container.targetVelocity;
            set => _container.targetVelocity = value;
        }
        public float Force {
            get => _container.force;
            set => _container.force = value;
        }
        public bool FreeSpin {
            get => _container.freeSpin;
            set => _container.freeSpin = value;
        }

        public JointMotor() { _container = new UnityEngine.JointMotor(); }
        internal JointMotor(UnityEngine.JointMotor container) { _container = container; }
        internal UnityEngine.JointMotor GetUnity() => _container;
    }
}
