using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.Attributes;

namespace Synthesis.ModelManager.Controllables {
    public class TestControllable : Controllable {

        [ControllableInput("One")]
        public float TestAxisOne { get; set; } = 2.0f;

        [ControllableInput("Two")]
        public float TestAxisTwo { get; set; } = 7.0f;

    }
}
