using System;
using SynthesisAPI.Modules.Attributes;

#nullable enable

namespace SynthesisAPI.EnvironmentManager.Components {
    public class BundleMarker : Component {
        public string Name { get; set; }

        public BundleMarker() {
            Name = "";
        }
    }
}
