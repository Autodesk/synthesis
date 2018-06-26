using System;

public class BrowseRobotState : BrowseFileState
{
    /// <summary>
    /// Initializes a new <see cref="BrowseFileState"/> instance.
    /// </summary>
    public BrowseRobotState() : base("RobotDirectory",
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//synthesis//Robots")
    {
    }
}
