using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class DebugJointAxes : MonoBehaviour {
    public static List<(Vector3 point, Func<Matrix4x4> trans)> DebugPoints =
        new List<(Vector3 point, Func<Matrix4x4> trans)>();
    public static List<(UnityEngine.Bounds bounds, Func<Matrix4x4> trans)> DebugBounds =
        new List<(UnityEngine.Bounds bounds, Func<Matrix4x4> trans)>();

    public void OnDrawGizmos() {
        if (Application.isPlaying) {
            SynthesisAPI.Simulation.SimulationManager.Drivers.Select(x => x.Value)
                .ForEach(a => a /*.Where(c => c.Name == "e94f72e4-2d00-48a6-bf88-4d3eb98f9d22")*/.ForEach(y => {
                    if (y is Synthesis.RotationalDriver) {
                        var ArmDriver = y as Synthesis.RotationalDriver;

                        var anchorA = ArmDriver.JointA.anchor;
                        var axisA   = ArmDriver.JointA.axis;
                        var anchorB = ArmDriver.JointB.anchor;
                        var axisB   = ArmDriver.JointB.axis;

                        Vector3 globalAnchorA =
                            ArmDriver.JointA.gameObject.transform.localToWorldMatrix.MultiplyPoint(anchorA);
                        Vector3 globalAxisA =
                            ArmDriver.JointA.gameObject.transform.localToWorldMatrix.MultiplyVector(axisA);
                        Vector3 globalAnchorB =
                            ArmDriver.JointB.gameObject.transform.localToWorldMatrix.MultiplyPoint(anchorB);
                        Vector3 globalAxisB =
                            ArmDriver.JointB.gameObject.transform.localToWorldMatrix.MultiplyVector(axisB);

                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere(globalAnchorA, 0.01f);
                        Gizmos.DrawLine(globalAnchorA, globalAnchorA + (globalAxisA.normalized * 0.2f));
                        // Gizmos.color = Color.magenta;
                        // Gizmos.DrawSphere(globalAnchorB, 0.01f);
                        // Gizmos.DrawLine(globalAnchorB, globalAnchorB + (globalAxisB.normalized * 0.2f));

                        // Gizmos.color = Color.white;
                        // Gizmos.DrawSphere(ArmDriver.JointA.gameObject.transform.position, 0.05f);
                        // Gizmos.DrawSphere(ArmDriver.JointB.gameObject.transform.position, 0.05f);
                    }
                }));
            DebugPoints.RemoveAll(x => {
                try {
                    Gizmos.color = Color.white;
                    Gizmos.DrawSphere(x.trans().MultiplyPoint(x.point), 0.01f);
                    return false;
                } catch (Exception) {
                    return true;
                }
            });
            DebugBounds.RemoveAll(x => {
                try {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawWireCube(x.trans().MultiplyPoint(x.bounds.center), x.bounds.extents * 2f);
                    return false;
                } catch (Exception) {
                    return true;
                }
            });
        }
    }
}
