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
            ExitTab = "Exit Tab",
            HomeTab = "Home Tab",
            DPMTab = "Gamepiece Tab",
            ScoringTab = "Scoring Tab",
            SensorTab = "Sensor Tab",
            EmulationTab = "Emulation Tab",

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

    public class EventAction
    {
        public const string
            StartSim = "Started Simulator",
            StartFeature = "Started Feature",
            TutorialRequest = "Requested Tutorial",
            Saved = "saved",
            BackedOut = "Back",
            Next = "Next",
            Continued = "continued",
            Clicked = "buttonClicked",
            Added = "Added",
            Removed = "Removed",
            Edited = "Edited",
            Hide = "Hide",
            Active = "Using",
            Viewed = "viewed",
            Load = "Load",
            Changed = "changed";
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
            Multiplater = "multiplayer",

            MainSimulator = "inSimulator",
            HomeTab = "homeTab",
            DPMTab = "dpmTab",
            ScoringTab = "scoringTab",
            SensorTab = "sensorTab",
            EmulationTab = "emulationTab",
            Tab = "toolbarTab";
    }

    public class TimingVarible
    {
        public const string
            Loading = "loading",
            Playing = "playing",
            Customizing = "customizing",
            Viewing = "viewing",
            Starting = "starting";
    }

    public class TimingLabel
    {
        public const string
            MixAndMatchMenu = "mixMenu",
            MainSimMenu = "mainSimMenu",
            MultiplayerLobbyMenu = "multiplayerLobbyMenu",

            MainSimulator = "mainSimulator",
            ResetField = "resetField",
            ChangeField = "changedField",
            MixAndMatch = "mixAndMatchMode",
            ReplayMode = "replayMode",

            HomeTab = "homeTab",
            DPMTab = "dpmTab",
            ScoringTab = "scoringTab",
            SensorTab = "sensorTab",
            EmulationTab = "emulationTab";
    }
}
