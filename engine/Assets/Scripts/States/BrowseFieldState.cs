using System;
using System.IO;

namespace Synthesis.States
{
    public class BrowseFieldState : BrowseFileState
    {
        /// <summary>
        /// Initializes a new <see cref="BrowseFieldState"/> instance.
        /// </summary>
        public BrowseFieldState() : base("FieldDirectory",
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Fields")
        {

        }
    }
}