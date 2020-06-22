using SynthesisAPI.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthesisAPI.InputManager.Behaviors
{
    public class InputGlobalBehavior : GlobalBehavior<InputBehavior>
    {
        private static InputGlobalBehavior instance;
        public static InputGlobalBehavior Instance {
            get {
                if (instance == null) instance = new InputGlobalBehavior();
                return instance;
            }
        }

        private InputGlobalBehavior(): base() { }
    }
}
