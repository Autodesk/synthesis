using System;
using System.Collections.Generic;
using System.Text;

namespace Mirabuf.Motor {
    public partial class SimpleMotor {

        // Extremely arbitrary
        public const float METRIC_TO_UNITY_TORQUE = 2000f / 50f;

        private UnityEngine.JointMotor? _unityMotor;
        public UnityEngine.JointMotor UnityMotor {
            get {
                if (!_unityMotor.HasValue) {
                    _unityMotor = new UnityEngine.JointMotor {
                        force = StallTorque * METRIC_TO_UNITY_TORQUE,
                        targetVelocity = MaxVelocity * UnityEngine.Mathf.Rad2Deg,
                        freeSpin = BrakingConstant > 0
                    };
                }
                return _unityMotor.Value;
            }
        }
    }
}
