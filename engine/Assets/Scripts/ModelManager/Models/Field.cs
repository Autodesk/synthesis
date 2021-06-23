using System;
using Synthesis.ModelManager;
using UnityEngine;

namespace Synthesis.ModelManager.Models
{
    public class Field : Model
    {
        public Field(string filePath)
        {
            Parse.AsField(filePath, this);
        }
    }
}
