using System.Collections.Generic;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.EnvironmentManager
{
    public class Bundle
    {
        public UniqueTypeList<Component> Components { get; private set; }
        public List<Bundle> ChildBundles { get; private set; }
        public Bundle()
        {
            Components = new UniqueTypeList<Component>();
            ChildBundles = new List<Bundle>();
        }
    }
}
