using MathNet.Spatial.Euclidean;
using SynthesisAPI.Modules.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisAPI.EnvironmentManager.Components
{
    [BuiltinComponent]
    public class HingeJoint : Component
    {
        internal Action<string, object> LinkedSetter = (n, o) => throw new Exception("Setter not setup");
        internal Func<string, object> LinkedGetter = n => throw new Exception("Getter not setup");

        private void Set(string n, object o) => LinkedSetter(n, o);
        private T Get<T>(string n) => (T)LinkedGetter(n);

        #region Properties

        public Vector3D Anchor {
            get => Get<Vector3D>("anchor");
            set => Set("anchor", value);
        }
        public Vector3D Axis {
            get => Get<Vector3D>("axis");
            set => Set("axis", value);
        }
        public float BreakForce {
            get => Get<float>("breakforce");
            set => Set("breakforce", value);
        }
        public float BreakTorque {
            get => Get<float>("breaktorque");
            set => Set("breaktorque", value);
        }
        public Rigidbody ConnectedBody {
            get => Get<Rigidbody>("connectedbody");
            set => Set("connectedbody", value);
        }
        public bool EnableCollision {
            get => Get<bool>("enablecollision");
            set => Set("enablecollision", value);
        }
        public bool UseLimits {
            get => Get<bool>("uselimits");
            set => Set("uselimits", value);
        }
        public JointLimits Limits {
            get => Get<JointLimits>("limits");
            set => Set("limits", value);
        }
        public float Velocity {
            get => Get<float>("velocity");
        }
        public float Angle {
            get => Get<float>("angle");
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
}
