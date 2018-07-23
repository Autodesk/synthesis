using System;

namespace Synthesis.States
{
    public class CopyBrowseFieldState : BrowseFileState
    {
        /// <summary>
        /// Initializes a new <see cref="BrowseFieldState"/> instance.
        /// </summary>
        public CopyBrowseFieldState() : base("FieldDirectory",
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//synthesis//Fields")
        {
        }
    }
}
