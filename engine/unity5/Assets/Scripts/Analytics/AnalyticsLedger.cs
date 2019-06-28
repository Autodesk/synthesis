using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnalyticsLedger
{

    public static int getMilliseconds()
    {
        return (int)(Time.unscaledTime * 1000);
    }

    public class EventCatagory {
        public const string
            MainSimMenu = "simMenu",
            MixAndMatchMenu = "mixMenu",
            SettingsMenu = "settingsMenu",
            MultiplayerMenu = "multiplayerMenu",
            MainSimulator = "mainSimulator",
            MixAndMatchSimulator = "mixSimulator",
            MultiplayerSimulator = "multiplayerSimulator";
    }

    public class EventAction
    {
        public const string
            Start = "start",
            TutorialRequest = "requestedTutorial",
            Saved = "saved",
            BackedOut = "backedOut",
            Continued = "continued";
    }

    public class PageView
    {
        public const string
            MainSimMenu = "simMenu",
            MixAndMatchMenu = "mixMenu",
            MultiplayerMenu = "multiplayerMenu",
            MainSimulator = "mainSimulator",
            MixAndMatchSimulator = "mixSimulator",
            MultiplayerSimulator = "multiplayerSimulator";
    }

    public class TimingCatagory
    {
        public const string
            Main = "main",
            MixMatch = "mixAndMatch",
            Multiplater = "multiplayer";
    }

    public class TimingVarible
    {
        public const string
            Loading = "loading",
            Playing = "playing",
            Customizing = "customizing";
    }

    public class TimingLabel
    {
        public const string
            ControlPanel = "controlPanel",
            MixAndMatchMenu = "mixMenu",
            MainSimMenu = "mainSimMenu",
            MultiplayerLobbyMenu = "multiplayerLobbyMenu",
            MixAndMatchSim = "mixSim",
            MainSim = "mainSim",
            MultiplayerSim = "multiplayerSim";
    }
}
