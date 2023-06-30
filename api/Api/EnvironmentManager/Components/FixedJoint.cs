using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MathNet.Spatial.Euclidean;

#nullable enable

namespace SynthesisAPI.EnvironmentManager.Components
{
    public class FixedJoint : IJoint
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

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
