using MathNet.Numerics;

namespace SynthesisCore.UI
{
    public struct ControlInfo
    {
        public string Name { get; }
        public string Key { get; }

        public ControlInfo(string name, string key)
        {
            Name = name;
            Key = key;
        }
    }
}