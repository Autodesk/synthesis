using System.Collections.Generic;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.EnvironmentManager
{
    public class Bundle
    {
        public UniqueTypeList<Component> Components { get; }
        public List<Bundle> ChildBundles { get; }
    }
}
