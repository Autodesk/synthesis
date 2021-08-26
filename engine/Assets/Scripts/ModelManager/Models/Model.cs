using System;
using System.Collections.Generic;
using Synthesis.Import;
using SynthesisAPI.Simulation;
using UnityEngine;

namespace Synthesis.ModelManager.Models
{
    public class Model
    {
        public string Name { get; set; }

        private GameObject _object;

        protected HashSet<Motor> motors = new HashSet<Motor>();
        public HashSet<Motor> Motors { get => motors; } // TODO: This bad, go back and fix
        public List<GearboxData> GearboxMeta = new List<GearboxData>();

        public DrivetrainMeta DrivetrainMeta;

        public static implicit operator GameObject(Model model) => model._object;

        protected Model() {}

        public Model(string filePath)
        {
            _object = Importer.MirabufAssemblyImport(filePath);
            DrivetrainMeta = new DrivetrainMeta { Type = DrivetrainType.Arcade };
            _object.transform.position = Vector3.up * 0.5f;
            Name = _object.GetComponent<RobotInstance>().Info?.Name;
        }

        public void DestroyModel()
        {
            if (SimulationManager.SimulationObjects.TryGetValue(Name, out var so))
            {
                SimulationManager.RemoveSimObject(so);
            }
            GameObject.Destroy(_object);
        }

        private static int counter = 0;
        private bool AddMotor(HingeJoint joint)
        {
            Motor m = _object.AddComponent<Motor>();
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