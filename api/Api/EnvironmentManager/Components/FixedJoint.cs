using SynthesisAPI.Modules.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisAPI.EnvironmentManager.Components
{
    [BuiltinComponent]
    public class FixedJoint : Component
    {
        internal Action<string, object> LinkedSetter = (n, o) => throw new Exception("Setter not assigned");
        internal Func<string, object> LinkedGetter = n => throw new Exception("Setter not assigned");

        private void Set(string n, object o) => LinkedSetter(n, o);
        private T Get<T>(string n) => (T)LinkedGetter(n);

        #region Properties

        public Rigidbody ConnectedBody {
            get => Get<Rigidbody>("connectedbody");
            set => Set("connectedbody", value);
        }
        public float BreakForce {
            get => Get<float>("breakforce");
            set => Set("breakforce", value);
        }
        public float BreakTorque {
            get => Get<float>("breaktorque");
            set => Set("breaktorque", value);
        }
        public bool EnableCollision {
            get => Get<bool>("enablecollision");
            set => Set("enablecollision", value);
        }

        #endregion
    }
}
