using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthesis.Simulator.Input
{
    public interface IDigitalInput
    {
        int Length { get; }
        bool GetUp();
        bool GetDown();
        bool GetHeld();
    }
}
