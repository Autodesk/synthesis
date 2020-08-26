using System;
using Api.InputManager;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EventBus;
using SynthesisAPI.InputManager;
using SynthesisAPI.Modules.Attributes;

namespace SynthesisCore
{
    [InitializationPriority(1)]
    public class InputSystem : SystemBase
    {
        public static bool IsAwaitingKey = false;
        
        public InputSystem() { }

        public override void Setup() { }

        public override void OnPhysicsUpdate() { }

        public override void OnUpdate()
        {
            InputManager.UpdateInputs();
        }

        public override void OnKeyPress(string kc)
        {
            if (IsAwaitingKey)
            {
                EventBus.Push(new KeyEvent(kc));
            }
        }

        public override void Teardown() { }
    }
}
