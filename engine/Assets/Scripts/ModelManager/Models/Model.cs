using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis.ModelManager.Models
{
    public class Model
    {
        public string Name { get; set; }

        public GameObject GameObject { get; set; }

        protected HashSet<Motor> motors = new HashSet<Motor>();
        public HashSet<Motor> Motors { get => motors; } // TODO: This bad, go back and fix
        public List<GearboxData> GearboxMeta = new List<GearboxData>();

        public DrivetrainMeta DrivetrainMeta;

        public static implicit operator GameObject(Model model) => model.GameObject;

        public Model()
        {

        }

        public Model(string filePath)
        {
            // Parse.AsModel(filePath, this);
        }

        private static int counter = 0;
        public bool AddMotor(HingeJoint joint)
        {
            Motor m = GameObject.AddComponent<Motor>();
            m.Joint = joint;
            m.Meta = ($"Motor {counter}", counter.ToString());
            counter++;
            joint.name = m.Meta.Name;
            return motors.Add(m);
        }
    }

    public struct DrivetrainMeta
    {
        public DrivetrainType Type;
        public GearboxData[] SelectedGearboxes;
    }

    public enum DrivetrainType
    {
        NotSelected = 0, // Default
        Arcade = 1,
        Tank = 2
    }
}