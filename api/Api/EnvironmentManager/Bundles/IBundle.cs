using SynthesisAPI.Utilities;

namespace SynthesisAPI.EnvironmentManager.Bundles
{
    public interface IBundle
    {
        public UniqueTypeList<Component> Components { get; }
    }
}
