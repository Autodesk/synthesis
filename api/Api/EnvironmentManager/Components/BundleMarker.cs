using System;
using SynthesisAPI.Modules.Attributes;

namespace SynthesisAPI.EnvironmentManager.Components {
    [BuiltinComponent]
    public class BundleMarker : Component {
        public string Name { get; set; }
    }
}