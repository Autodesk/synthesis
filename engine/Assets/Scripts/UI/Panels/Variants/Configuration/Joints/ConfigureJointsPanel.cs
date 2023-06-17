using System.Collections;
using System.Collections.Generic;
using SynthesisAPI.Simulation;
using UnityEngine;
using System.Linq;

namespace Synthesis.UI.Panels {
    public class ConfigureJointsPanel : Panel {

        [SerializeField]
        public GameObject ItemContainer;
        [SerializeField]
        public GameObject RotationalJointItemPrefab;

        public void Start() {
            PopulateItems();
        }

        public void PopulateItems() {
            // if (RobotSimObject.CurrentlyPossessedRobot == string.Empty)
            //     return;
            // SimulationManager.Drivers[RobotSimObject.CurrentlyPossessedRobot].Where(x => x is RotationalDriver).ForEach(y => {
            //     var obj = Instantiate(RotationalJointItemPrefab, ItemContainer.transform);
            //     var rotItem = obj.GetComponent<RotationalJointItem>();
            //     var rotDriver = y as RotationalDriver;
            //     // Gross
            //     rotItem.Label.text = RobotSimObject.GetCurrentlyPossessedRobot().MiraLive.MiraAssembly.Data.Signals.SignalMap[rotDriver.InputSignal].Info.Name;
            //     // rotItem.SpeedInput.text = (0.123f).ToString(); // eh? TODO
            //     rotItem.SpeedInput.text = rotDriver.Motor.targetVelocity.ToString();
            //     rotItem.SpeedInput.onValueChanged.AddListener(val => {
            //         float velo;
            //         if (!float.TryParse(val, out velo))
            //             velo = 0f;
            //         rotDriver.Motor = new JointMotor { targetVelocity = velo, force = rotDriver.Motor.force, freeSpin = rotDriver.Motor.freeSpin };
            //     });
            //     rotItem.TorqueInput.text = rotDriver.Motor.force.ToString();
            //     rotItem.TorqueInput.onValueChanged.AddListener(val => {
            //         float torq;
            //         if (!float.TryParse(val, out torq))
            //             torq = 0f;
            //         rotDriver.Motor = new JointMotor { targetVelocity = rotDriver.Motor.targetVelocity, force = torq, freeSpin = rotDriver.Motor.freeSpin };
            //     });
            // });
        }
    }
}
