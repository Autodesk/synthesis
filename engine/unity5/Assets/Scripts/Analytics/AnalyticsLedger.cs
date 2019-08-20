using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnalyticsLedger
{

    public static int getMilliseconds()
    {
        return (int)(Time.unscaledTime * 1000);
    }

    #region Anatomy of Analytics Events
    /* Categories: Groups of objects
     * Actions: Actions of objects
     * Labels: Additional information */
 

    #endregion

    /// <summary>
    /// Categories group multiple objects together. Each main category is grouped by the tabs
    /// in the simulator. Most events will fall into one of the tab categories (e.g. HomeTab.)
    /// </summary>
    public class EventCatagory {
        public const string

            // Main Menu has been deprecated. May consider removing or archiving MainMenu code.
            MainSimMenu = "Main Menu",
            MixAndMatchMenu = "Mix and Match Menu",
            MultiplayerMenu = "LAN Multiplayer Menu",
            MixAndMatchSimulator = "Mix and Match Simulator",

            // Start of analytics tracking
            MainSimulator = "Main Simulator",
            //MultiplayerSimulator = "multiplayerSimulator", // network multiplayer temporarily disabled.

            // Toolbar tabs
            MenuTab = "Menu Tab",
            HomeTab = "Home Tab",
            DPMTab = "Gamepiece Tab",
            ScoringTab = "Scoring Tab",
            SensorTab = "Sensor Tab",
            EmulationTab = "Emulation Tab",
            ExitTab = "Exit Tab",

            // Global categories
            AddRobot = "Add Robot",
            ChangeRobot = "Change Robot",
            LoadRobot = "Load Robot",
            ChangeField = "Change Field",
            Reset = "Reset",
            CameraView = "Camera View",
            Help = "Help Menu",
            Tutorials = "Tutorials";
    }

    /// <summary>
    /// Actions for user behaviors
    /// </summary>
    public class EventAction
    {
        public const string
            StartSim = "Started Simulator",
            TutorialRequest = "Requested Tutorial",
            BackedOut = "Back",
            Next = "Next",
            Clicked = "Clicked",
            Added = "Added",
            Removed = "Removed",
            Edited = "Edited",
            Toggled = "Toggled",
            Viewed = "Viewed",
            Load = "Load",
            Exit = "Exit",
            Changed = "Changed", 
            
            CodeType = "Code Language";
    }

    /// <summary>
    /// Not currently in use but implemented on backend 08/2019
    /// </summary>
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

    /// <summary>
    /// Similar to event categories, timing categories organize objects
    /// into various groups. 
    /// </summary>
    public class TimingCatagory
    {
        public const string
            Main = "Main Menu",
            MixMatch = "Mix and Match",
            Multiplater = "Multiplayer",

            MainSimulator = "In Simulator",
            MenuTab = "Menu Tab",
            HomeTab = "Home Tab",
            DPMTab = "Gamepiece Tab",
            ScoringTab = "Scoring Tab",
            SensorTab = "Sensor Tab",
            EmulationTab = "Emulation Tab",
            Tab = "Toolbar Tab";
    }

    /// <summary>
    /// Actions for timing events
    /// </summary>
    public class TimingVarible
    {
        public const string
            Loading = "Loading",
            Playing = "Playing",
            Customizing = "Customizing",
            Viewing = "Viewing",
            Starting = "Starting";
    }

    /// <summary>
    /// Additional information to expand on the timing categories. 
    /// </summary>
    public class TimingLabel
    {
        public const string
            MixAndMatchMenu = "Mix and Match Menu",
            MainSimMenu = "Main Menu",
            MultiplayerLobbyMenu = "Multiplayer Lobby Menu",

            MainSimulator = "Main Simulator",
            ResetField = "Reset Field",
            ChangeField = "Change Field",
            MixAndMatch = "Mix and Match Mode",
            ReplayMode = "Replay Mode",

            HomeTab = "Home Tab",
            DPMTab = "Gamepiece Tab",
            ScoringTab = "Scoring Tab",
            SensorTab = "Sensor Tab",
            EmulationTab = "Emulation Tab";
    }
}
