using System;
using Newtonsoft.Json;

public partial class EngineData
{
    [JsonProperty("roborio")]
    public SendData Roborio { get; set; }

    public EngineData()
    {
        Roborio = new SendData();
    }
}

public partial class SendData
{
    [JsonProperty("digital_hdrs")]
    public long[] DigitalHdrs { get; set; }

    [JsonProperty("joysticks")]
    public JoystickData[] Joysticks { get; set; }

    [JsonProperty("encoders", NullValueHandling=NullValueHandling.Ignore)]
    public EncoderData[] Encoders { get; set; }

    [JsonProperty("digital_mxp")]
    public OutputDigitalMxp[] DigitalMxp { get; set; }

    [JsonProperty("match_info")]
    public MatchInfo MatchInfo { get; set; }

    [JsonProperty("robot_mode")]
    public RobotMode RobotMode { get; set; }

    public SendData()
    {
        Joysticks = new JoystickData[6];
        for (int i = 0; i < 6; i++)
            Joysticks[i] = new JoystickData();
        DigitalHdrs = new long[10];
        for (int i = 0; i < 10; i++)
            DigitalHdrs[i] = 0;    
        DigitalMxp = new OutputDigitalMxp[16];
        for (int i = 0; i < 16; i++)
            DigitalMxp[i] = new OutputDigitalMxp();
        Encoders = new EncoderData[8];
        for (int i = 0; i < 8; i++)
            Encoders[i] = new EncoderData();
        MatchInfo = new MatchInfo();
        RobotMode = new RobotMode();
    }
}

public partial class OutputDigitalMxp
{
    [JsonProperty("config")]
    public string Config { get; set; }

    [JsonIgnore]
    public Config ConfigEnum { get; set; }

    [JsonProperty("value")]
    public double Value { get; set; }

    public OutputDigitalMxp()
    {
        Config = "DI";
        ConfigEnum = 0;
        Value = 0;
    }
}

public partial class JoystickData
{
    [JsonProperty("is_xbox")]
    public long IsXbox { get; set; }

    [JsonProperty("type")]
    public long Type { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("buttons")]
    public long Buttons { get; set; }

    [JsonProperty("button_count")]
    public long ButtonCount { get; set; }

    [JsonProperty("axes")]
    public long[] Axes { get; set; }

    [JsonProperty("axis_count")]
    public long AxisCount { get; set; }

    [JsonProperty("axis_types")]
    public long[] AxisTypes { get; set; }

    [JsonProperty("povs")]
    public long[] Povs { get; set; }

    [JsonProperty("pov_count")]
    public long PovCount { get; set; }

    [JsonProperty("outputs")]
    public long Outputs { get; set; }

    [JsonProperty("left_rumble")]
    public long LeftRumble { get; set; }

    [JsonProperty("right_rumble")]
    public long RightRumble { get; set; }


    public JoystickData()
    {
        Axes = new long[12];
        AxisTypes = new long[12];
        Povs = new long[12];

        IsXbox = 0;
        Type = 0;
        Name = "";
        Buttons = 0;
        ButtonCount = 10;
        for (int i = 0; i < 12; i++)
            Axes[i] = 0;
        AxisCount = 12;
        for (int i = 0; i < 12; i++)
            AxisTypes[i] = 0;
        for (int i = 0; i < 12; i++)
            Povs[i] = 0;
        PovCount = 12;
        Outputs = 0;
        LeftRumble = 0;
        RightRumble = 0;
    }

    public JoystickData(int axisCount, int buttonCount, int povCount)
    {
        Axes = new long[12];
        AxisTypes = new long[12];
        Povs = new long[12];

        IsXbox = 0;
        Type = 0;
        Name = "";
        Buttons = 10;
        ButtonCount = buttonCount;
        for (int i = 0; i < 12; i++)
            Axes[i] = 0;
        AxisCount = axisCount;
        for (int i = 0; i < 12; i++)
            AxisTypes[i] = 0;
        for (int i = 0; i < 12; i++)
            Povs[i] = 0;
        PovCount = povCount;
        Outputs = 0;
        LeftRumble = 0;
        RightRumble = 0;
    }

    public JoystickData(bool isXbox, int type, string name, int axisCount, int buttonCount, int povCount)
    {
        Axes = new long[12];
        AxisTypes = new long[12];
        Povs = new long[12];

        IsXbox = isXbox?1:0;
        Type = type;
        Name = name;
        Buttons = 0;
        ButtonCount = buttonCount;
        for (int i = 0; i < 12; i++)
            Axes[i] = 0;
        AxisCount = axisCount;
        for (int i = 0; i < 12; i++)
            AxisTypes[i] = 0;
        for (int i = 0; i < 12; i++)
            Povs[i] = 0;
        PovCount = povCount;
        Outputs = 0;
        LeftRumble = 0;
        RightRumble = 0;
    }


    public void updateJoystick(double[] axes, bool[] buttons, double[] povs)
    {
        if (axes.Length > AxisCount)
            throw new Exception();
        if (povs.Length > PovCount)
            throw new Exception();

        int buttonValue = 0;
        for (int i = 0; i < axes.Length; i++)
        {
            Axes[i] = ((int)(axes[i] * 128) >= 128 ? 127 : (int)(axes[i] * 128));
        }
        for (int i = 0; i < buttons.Length && i < 32; i++)
            buttonValue += ((buttons[i] ? 1 : 0) << i);
        Buttons = buttonValue;
        for (int i = 0; i < povs.Length; i++)
            Povs[i] = (int)povs[i];
    }
}

public class MatchInfo
{
    [JsonProperty("event_name")]
    string EventName;

    [JsonProperty("game_specific_message")]
    string GameSpecificMessage;

    [JsonProperty("match_type")]
    string MatchType;

    [JsonProperty("match_number")]
    int MatchNumber;

    [JsonProperty("replay_number")]
    int ReplayNumber;

    [JsonProperty("alliance_station_id")]
    string AllianceStationId;

    [JsonProperty("match_time")]
    double MatchTime;

    public MatchInfo()
    {
        EventName = "";
        GameSpecificMessage = "LLL";
        MatchType = "NONE";
        MatchNumber = 0;
        ReplayNumber = 0;
        AllianceStationId = "RED1";
        MatchTime = 0.0;
    }

    void updateMatchInfo(string EventName = "", string GameSpecificMessage = "LLL", string MatchType = "", int MatchNumber = 0, int ReplayNumber = 0, string AllianceStationId = "RED1", double MatchTime = 0.0)
    {
        this.EventName = EventName;
        this.GameSpecificMessage = GameSpecificMessage;
        this.MatchType = MatchType;
        this.MatchNumber = MatchNumber;
        this.ReplayNumber = ReplayNumber;
        this.AllianceStationId = AllianceStationId;
        this.MatchTime = MatchTime;
    }

}

public class RobotMode
{
    [JsonProperty("mode")]
    string Mode;

    [JsonProperty("enabled")]
    int Enabled;

    [JsonProperty("emergency_stopped")]
    int EmergencyStopped;

    [JsonProperty("fms_attached")]
    int FMSAttached;

    [JsonProperty("ds_attached")]
    int DSAttached;

    public RobotMode()
    {
        Mode = "TELEOPERATED";
        Enabled = 0;
        EmergencyStopped = 0;
        FMSAttached = 1;
        DSAttached = 1;
    }

    /*public void updateRobotMode(string Mode = "TELEOPERATED", int Enabled = 0, int EmergencyStopped = 0, int FMSAttached = 1, int DSAttached = 1)
    {
        this.Mode = Mode;
        this.Enabled = Enabled;
        this.EmergencyStopped = EmergencyStopped;
        this.FMSAttached = FMSAttached;
        this.DSAttached = DSAttached;
    }*/

    public void updateRobotMode()
    {
        Enabled = Synthesis.GUI.EmulationDriverStation.Instance.isRobotDisabled?0:1;
        EmergencyStopped = 0;
        FMSAttached = 1;
        DSAttached = 1;
        switch(Synthesis.GUI.EmulationDriverStation.Instance.state)
        {
            case (Synthesis.GUI.EmulationDriverStation.DriveState.Teleop):
                Mode = "TELEOPERATED";
                break;
            case (Synthesis.GUI.EmulationDriverStation.DriveState.Auto):
                Mode = "AUTONOMOUS";
                break;
            case (Synthesis.GUI.EmulationDriverStation.DriveState.Test):
                Mode = "TEST";
                break;
            default:
                Mode = "TELEOPERATED";
                break;
        }
    }
}

public class EncoderData
{
    [JsonProperty("a_channel")]
    int ChannelA { get; set; }
    [JsonProperty("a_type")]
    string ChannelAType { get; set; }

    [JsonProperty("b_channel")]
    int ChannelB { get; set; }
    [JsonProperty("b_type")]
    string ChannelBType { get; set; }

    [JsonProperty("ticks")]
    int Count { get; set; }

    public EncoderData()
    {
        ChannelA = 0;
        ChannelB = 0;
        ChannelAType = "DI";
        ChannelBType = "DI";

        Count = 0;
    }

    public void updateEncoder(int channelA, string channelAType, int channelB, string channelBType, int count)
    {
        ChannelA = channelA;
        ChannelAType = channelAType;
        ChannelB = channelB;
        ChannelBType = channelBType;
        Count = count;
    }
}

public enum Config { Di, Do };
