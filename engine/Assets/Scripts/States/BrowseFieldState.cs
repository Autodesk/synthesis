using System;
using System.IO;
using UnityEngine;

namespace Synthesis.States
{
    public class BrowseFieldState : BrowseFileState
    {
        /// <summary>
        /// Initializes a new <see cref="BrowseFieldState"/> instance.
        /// </summary>
        public BrowseFieldState() : base("FieldDirectory"){

        }

        public override void End()
        {
            base.End();
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString("FieldDirectory")) &&
                (string.IsNullOrEmpty(PlayerPrefs.GetString("simSelectedField")) || string.IsNullOrEmpty(PlayerPrefs.GetString("simSelectedFieldName"))))
            {
                PlayerPrefs.SetString("simSelectedField", PlayerPrefs.GetString("FieldDirectory") + Path.DirectorySeparatorChar + Field.UnityFieldDefinition.EmptyGridName);
                PlayerPrefs.SetString("simSelectedFieldName", Field.UnityFieldDefinition.EmptyGridName);
            }
        }
    }
}