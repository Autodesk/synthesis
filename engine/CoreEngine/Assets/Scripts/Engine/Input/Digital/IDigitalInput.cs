using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthesis.Simulator.Input
{
    // TODO: Maybe make the functions all into one that returns an enum of the state of the button
    public interface IDigitalInput
    {
        int Length { get; }
        DigitalState GetState();

        enum DigitalState
        {
            Up, Down, Held, None
        }
    }
}
