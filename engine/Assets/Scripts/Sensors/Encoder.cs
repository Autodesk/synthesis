using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Sensors
{
    enum PortType
    {
        DIO,
        Analog
    };


    /* Encoder data: Port numbers (two ints), port types (two enums or strings representing either digital input or analog input), 
     * ticks per revolution (int)*/
    class Encoder
    {
        public Encoder(int port1, int port2, PortType portTypeA, PortType portTypeB, int ppr)
        {

        }
    }
}
