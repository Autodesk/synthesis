using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisAPI
{
    public interface IModule
    {
        void OnStart();
        void OnUpdate();
    }
}
