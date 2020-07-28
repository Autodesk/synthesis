using System;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.EnvironmentManager.Bundles
{
    public class ObjectBundle : IBundle
    {
        public UniqueTypeList<Component> Components { get; }

        public ObjectBundle()
        {

        }

    }
}
