namespace SynthesisAPI.InputManager
{
    public struct ControllerInfo
    {
        public string Name;

        public ControllerType Type;

        public ControllerInfo(string name, ControllerType type)
        {
            Name = name;
            Type = type;
        }

        public const int MaxControllers = 11;
    }

    public enum ControllerType
    {
        Ps4, Other
    }
}
