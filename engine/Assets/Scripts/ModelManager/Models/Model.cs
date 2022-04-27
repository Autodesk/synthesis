using System;
using System.Collections.Generic;
using Synthesis.Import;
using SynthesisAPI.Simulation;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Synthesis.ModelManager.Models
{
    public class Model
    {
        public string Name { get; set; }

        public GameObject _object;

        protected HashSet<Motor> motors = new HashSet<Motor>();
        public HashSet<Motor> Motors { get => motors; } // TODO: This bad, go back and fix
        public List<GearboxData> GearboxMeta = new List<GearboxData>();

        public DrivetrainMeta DrivetrainMeta;

        public static implicit operator GameObject(Model model) => model._object;

        protected Model() {}

        public Model(string filePath, Vector3 position, Quaternion rotation, bool reverseSideMotors = false)
        {
            _object = Importer.MirabufAssemblyImport(filePath, reverseSideMotors);
            _object.transform.SetParent(GameObject.Find("Game").transform);
            DrivetrainMeta = new DrivetrainMeta { Type = DrivetrainType.Arcade };
            _object.transform.position = position;
            _object.transform.rotation = rotation;
            // var robotInstance = _object.GetComponent<RobotInstance>();
            // Name = robotInstance.Info?.Name;
            // robotInstance.ConfigureDrivebase(position,rotation, DrivetrainMeta);

            // Camera.main.GetComponent<CameraController>().FocusPoint =
            //     () => robotInstance.RootNode.transform.localToWorldMatrix.MultiplyPoint(robotInstance.RootBounds.center);
        }

        public void DestroyModel()
        {
            if (SimulationManager.SimulationObjects.TryGetValue(Name, out var so))
            {
                SimulationManager.RemoveSimObject(so);
            }
            Object.Destroy(_object);
        }

        private static int _counter = 0;
        private bool AddMotor(HingeJoint joint)
        {
            Motor m = _object.AddComponent<Motor>();
            m.Joint = joint;
            m.Meta = ($"Motor {_counter}", _counter.ToString());
            _counter++;
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
