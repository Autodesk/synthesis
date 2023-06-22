using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.Attributes {
    [AttributeUsage(AttributeTargets.Property)]
    public class ControllableInputAttribute : Attribute {
        public string Name { get; private set; } = string.Empty;

        public ControllableInputAttribute() {
        }

        public ControllableInputAttribute(string name) {
            Name = name;
        }
    }
}
