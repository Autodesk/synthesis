using System;
using System.Diagnostics;
using Synthesis.GUI;
using EmulationService;
using UnityEngine;
using Synthesis.FSM;

namespace Synthesis
{
    public class EmulationController : MonoBehaviour
    {
        public static EmulationController Get()
        {
            return GameObject.Find("EmulationController").GetComponent<EmulationController>();
        }

        private static Process proc;

        public const string DEFAULT_HOST = "10.140.148.24"; // 127.0.0.1
        public const string DEFAULT_PORT = "50051";

        private const int TIMEOUT = 5;
        private const int RETRIES = 5;

        public string Ip = DEFAULT_HOST;
        public string Port = DEFAULT_PORT;

        private Channel<IMessage> inputCommander, inputListener;

        private Channel<IMessage> outputCommander, outputListener;

        private SenderTask sender;

        private ReceiverTask receiver;

        private System.Threading.Thread senderThread, receiverThread;

        public void Awake()
        {

            (inputCommander, inputListener) = Channel<IMessage>.CreateOneshot<IMessage>();
            (outputCommander, outputListener) = Channel<IMessage>.CreateOneshot<IMessage>();

            sender = new SenderTask(inputCommander, inputListener, Ip, Port, TIMEOUT, RETRIES);
            senderThread = ManagedTaskRunner.Create(sender);

            receiver = new ReceiverTask(outputCommander, outputListener, Ip, Port, TIMEOUT, RETRIES);
            receiverThread = ManagedTaskRunner.Create(receiver);

            senderThread.Start();
            receiverThread.Start();

            ProcessStartInfo startinfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = @"C:\Program Files\qemu\qemu-system-arm.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = " -machine xilinx-zynq-a9 -cpu cortex-a9 -m 2048 -kernel " + EmulationDriverStation.emulationDir + "zImage" + " -dtb " + EmulationDriverStation.emulationDir + "zynq-zed.dtb" + " -display none -serial null -serial mon:stdio -append \"console=ttyPS0,115200 earlyprintk root=/dev/mmcblk0 rw\" -net user,hostfwd=tcp::10022-:22,hostfwd=tcp::11000-:11000,hostfwd=tcp::11001-:11001,hostfwd=tcp::2354-:2354 -net nic -sd " + EmulationDriverStation.emulationDir + "rootfs.ext4",
                Verb = "runas"
            };
            proc = Process.Start(startinfo);
        }

        public void OnDestroy()
        {
            inputCommander.Send(new StandardMessage.ExitMessage());
            outputCommander.Send(new StandardMessage.ExitMessage());
            proc.Kill();
        }

        public bool IsConnected()
        {
            return sender.IsConnected() && receiver.IsConnected();
        }

        public static int getPWMCount()
        {
            var instance = OutputManager.Instance;
            return instance.PwmHeaders.Count;
        }

        public static double getPWM(int index)
        {
            var instance = OutputManager.Instance;

            if (index > instance.PwmHeaders.Count)
                throw new IndexOutOfRangeException();
            return instance.PwmHeaders[index];

        }

        public static double getCAN(int index)
        {
            var instance = OutputManager.Instance;

            if (index > 63)
                throw new IndexOutOfRangeException();
            foreach (var CANMotorController in instance.CanMotorControllers)
                if (CANMotorController.Id == index)
                    return CANMotorController.PercentOutput;
            throw new Exception("Detached emulation CAN output (ID" + index + "\"");
        }

        public static void UpdateJoystick(int index, double[] axes, bool[] buttons, double[] povs)
        {
            var instance = InputManager.Instance;

            if (index > instance.Joysticks.Count)
                throw new IndexOutOfRangeException();

            if (axes.Length > instance.Joysticks[index].AxisCount)
                throw new Exception();
            if (povs.Length > instance.Joysticks[index].PovCount)
                throw new Exception();

            for (int i = 0; i < axes.Length; i++)
                instance.Joysticks[index].Axes[i] = ((int)(axes[i] * 128) >= 128 ? 127 : (int)(axes[i] * 128));
            uint buttonValue = 0;
            for (int i = 0; i < buttons.Length && i < 32; i++)
                buttonValue += (buttons[i] ? 1u : 0u) << i;
            instance.Joysticks[index].Buttons = buttonValue;
            for (int i = 0; i < povs.Length; i++)
                instance.Joysticks[index].Povs[i] = (int)povs[i];
        }

        public static RobotInputs.Types.EncoderManager.Types.PortType ConvertPortType(SensorConnectionType type)
        {
            if (type == SensorConnectionType.DIO)
                return EmulationService.RobotInputs.Types.EncoderManager.Types.PortType.Di;
            if (type == SensorConnectionType.ANALOG)
                return EmulationService.RobotInputs.Types.EncoderManager.Types.PortType.Ai;
            throw new Exception("Invalid encoder port type");
        }
    }
}