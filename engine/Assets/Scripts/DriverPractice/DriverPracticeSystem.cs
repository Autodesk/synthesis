using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Synthesis.Entitys;
using Synthesis.ModelManager.Models;
using UnityEngine;

namespace Synthesis.DriverPractice {
    public class DriverPracticeSystem: MonoBehaviour {
        public void Update() {
            // var robots = EntityManager.GetEntitiesOfType<Robot>();

            float vert = Input.GetAxisRaw("Vertical");
            float horz = Input.GetAxisRaw("Horizontal");
            
            if (ModelManager.ModelManager.Models.Any()) {
                var robot = ModelManager.ModelManager.Models.Values.ElementAt(0);
                // robot.robot.Wheels.ForEachIndex((i, x) => {
                //     var jm = x.motor;
                //     jm.targetVelocity = Mathf.Clamp(vert + (robot.robot.WheelConfig[i] ? horz : -horz), -1, 1) * robot.robot.MaxSpeed;
                //     x.motor = jm;
                // });

                if (robot.DrivetrainMeta.Type == DrivetrainType.Arcade) {
                    var leftGearbox = robot.DrivetrainMeta.SelectedGearboxes[0];
                    var rightGearbox = robot.DrivetrainMeta.SelectedGearboxes[1];
                    var leftUuids = leftGearbox.MotorUuids;
                    var rightUuids = rightGearbox.MotorUuids;

                    foreach (var left in leftUuids) {
                        var m = robot.Motors.Find(x => x.Meta.Uuid == left);
                        var jm = new JointMotor() {
                            force = leftGearbox.Torque,
                            freeSpin = false,
                            targetVelocity = Mathf.Clamp(vert + horz, -1, 1) * leftGearbox.MaxSpeed
                        };
                        if (!ReferenceEquals(m, null)) {
                            if (!m.Joint.useMotor)
                                m.Joint.useMotor = true;
                            m.Joint.motor = jm;
                        }
                    }
                    
                    foreach (var right in rightUuids) {
                        var m = robot.Motors.Find(x => x.Meta.Uuid == right);
                        var jm = new JointMotor() {
                            force = rightGearbox.Torque,
                            freeSpin = false,
                            targetVelocity = Mathf.Clamp(vert - horz, -1, 1) * rightGearbox.MaxSpeed
                        };
                        if (!ReferenceEquals(m, null)) {
                            if (!m.Joint.useMotor)
                                m.Joint.useMotor = true;
                            m.Joint.motor = jm;
                        }
                    }
                }
            }
        }
    }
}
