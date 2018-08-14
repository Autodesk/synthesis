// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using SimBridge;
//
//    var emuData = EmuData.FromJson(jsonString);

using System;
using Newtonsoft.Json;

public partial class EmuData
{
    [JsonProperty("roborio")]
    public Roborio Roborio { get; set; }

    public EmuData()
    {
        Roborio = new Roborio();
    }
}

public partial class Roborio
{
    [JsonProperty("pwm_hdrs")]
    public double[] PwmHdrs { get; set; }

    [JsonProperty("relays")]
    public string[] Relays { get; set; }

    [JsonProperty("analog_outputs")]
    public double[] AnalogOutputs { get; set; }

    [JsonProperty("digital_mxp")]
    public InputDigitalMxp[] DigitalMxp { get; set; }

    [JsonProperty("digital_hdrs")]
    public long[] DigitalHdrs { get; set; }

    [JsonProperty("can_motor_controllers")]
    public CANDevice[] CANDevices { get; set; }

    public Roborio()
    {
        PwmHdrs = new double[10];
        Relays = new string[4];
        AnalogOutputs = new double[2];
        DigitalMxp = new InputDigitalMxp[26];
        DigitalHdrs = new long[10];
        CANDevices = new CANDevice[63];

        for (int i = 0; i < PwmHdrs.Length; i++)
            PwmHdrs[i]= 0.0;
        for (int i = 0; i < Relays.Length; i++)
            Relays[i] = "";
        for (int i = 0; i < AnalogOutputs.Length; i++)
            AnalogOutputs[i] = 0.0;
        for (int i = 0; i < DigitalMxp.Length; i++)
            DigitalMxp[i] = new InputDigitalMxp();
        for (int i = 0; i < DigitalHdrs.Length; i++)
            DigitalHdrs[i] = 0;
        for (int i = 0; i < CANDevices.Length; i++)
            CANDevices[i] = new CANDevice();
    }

}

public partial class InputDigitalMxp
{
    [JsonProperty("config")]
    public string Config { get; set; }

    [JsonProperty("value")]
    public double Value { get; set; }

    public InputDigitalMxp()
    {
        Config = "DI";
        Value = 0.0;
    }
}

public class CANDevice
{
    [JsonProperty("type")]
    public string type { get; set; }

    [JsonProperty("id")]
    public int id { get; set; }

    [JsonProperty("percent_output")]
    public float speed { get; set; }

    [JsonProperty("inverted")]
    public int inverted { get; set; }

}