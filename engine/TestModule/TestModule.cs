using SynthesisAPI;

namespace TestModule
{
    public class TestModule : IModule
    {
        public void OnStart()
        {
            ObjectManager.CreateObject("Module_Created_Object");
        }

        public void OnUpdate() { }
    }
}