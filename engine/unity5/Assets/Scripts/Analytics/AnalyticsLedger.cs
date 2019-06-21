using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnalyticsLedger
{
    public class EventCatagory {
        public const string
            Location = "location";
    }

    public class EventAction
    {
        public const string
            StartSimulator = "startSim",
            StartMixAndMatch = "startMix",
            TutorialRequest = "requestedTutorial";
    }

    public class PageView
    {
        public const string
            MainSimMenu = "simMenu",
            MixAndMatchMenu = "mixMenu";
    }

    public class TimingCatagory
    {
        public const string
            MainSim = "mainSim",
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
