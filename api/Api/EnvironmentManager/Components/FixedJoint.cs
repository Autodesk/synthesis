using SynthesisAPI.Modules.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using MathNet.Spatial.Euclidean;

namespace SynthesisAPI.EnvironmentManager.Components
{
    public class FixedJoint : IJoint
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region Properties

        internal Vector3D axis = new Vector3D(1, 0, 0);
        public Vector3D Axis {
            get => axis;
            set {
                axis = value;
                OnPropertyChanged();
            }
        }
        internal Vector3D anchor = new Vector3D(0, 0, 0);
        public Vector3D Anchor {
            get => anchor;
            set {
                anchor = value;
                OnPropertyChanged();
            }
        }
        internal Rigidbody connectedBody = null;
        public Rigidbody ConnectedBody {
            get => connectedBody;
            set {
                connectedBody = value;
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
        internal bool enableCollision = false;
        public bool EnableCollision {
            get => enableCollision;
            set {
                enableCollision = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}
