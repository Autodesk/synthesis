using System;
using System.Collections.Generic;
using System.Text;

namespace Mirabuf.Motor {
    public partial class SimpleMotor {

        // Extremely arbitrary

        private UnityEngine.JointMotor? _unityMotor;
        public UnityEngine.JointMotor UnityMotor {
            get {
                if (!_unityMotor.HasValue) {
                    _unityMotor = new UnityEngine.JointMotor {
                        force = StallTorque,
                        targetVelocity = MaxVelocity,
                        freeSpin = BrakingConstant > 0
                    };
                }
                return _unityMotor.Value;
            }
        }
    }
}
