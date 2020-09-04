using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SynthesisAPI.EnvironmentManager.Components
{
    public interface IJoint {
        public event PropertyChangedEventHandler PropertyChanged;
        public Vector3D Anchor { get; set; }
    }
}
