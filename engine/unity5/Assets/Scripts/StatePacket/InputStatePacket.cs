using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synthesis.StatePacket
{
    public class InputStatePacket
    {
        public enum SignalType
        {
            DIGITAL = 0,
            ANALOG = 1
        }

        public enum DigitalState
        {
            LOW = 0,
            HIGH = 1
        }

        public struct DIOModule
        {
            public const int LENGTH = 4;
            public int digitalInput;
        }

        public struct Encoders
        {
            public const int LENGTH = 4;
            public int value;
        }

        public class AnalogInput
        {
            public const int LENGTH = 32;
            public int[] analogValues = new int[32];
        }

        public struct Counter
        {
            public const int LENGTH = 4;
            public int value;
        }

        public DIOModule[] dio = new DIOModule[2];
        public Encoders[] encoders = new Encoders[4];
        public AnalogInput ai = new AnalogInput();
        public Counter[] counter = new Counter[8];

        public static Dictionary<RobotSensorType, SignalType> signalLookup = new Dictionary<RobotSensorType, SignalType>()
    {
        {RobotSensorType.ENCODER, SignalType.DIGITAL},
        {RobotSensorType.LIMIT, SignalType.DIGITAL},
        {RobotSensorType.POTENTIOMETER, SignalType.ANALOG}
    };

        public InputStatePacket()
        {
            for (int i = 0; i < dio.Length; i++)
                dio[i] = new DIOModule();

            for (int i = 0; i < encoders.Length; i++)
                encoders[i] = new Encoders();

            ai = new AnalogInput();

            for (int i = 0; i < counter.Length; i++)
                counter[i] = new Counter();
        }

        public int Write(byte[] packet)
        {
            int head = 0;

            for (int i = 0; i < dio.Length; i++)
            {
                Buffer.BlockCopy(new int[] { dio[i].digitalInput }, 0, packet, head, DIOModule.LENGTH);
                head += DIOModule.LENGTH;
            }

            for (int i = 0; i < encoders.Length; i++)
            {
                Buffer.BlockCopy(new int[] { encoders[i].value }, 0, packet, head, Encoders.LENGTH);
                head += Encoders.LENGTH;
            }

            Buffer.BlockCopy(ai.analogValues, 0, packet, head, AnalogInput.LENGTH);
            head += AnalogInput.LENGTH;

            for (int i = 0; i < counter.Length; i++)
            {
                Buffer.BlockCopy(new int[] { counter[i].value }, 0, packet, head, Counter.LENGTH);
                head += Counter.LENGTH;
            }

            return head;
        }
    }
}