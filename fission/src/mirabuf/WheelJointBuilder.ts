import { mirabuf } from "@/proto/mirabuf"
import type { WheelAxis } from "@/util/geometry/WheelAxisFit"

export interface WheelAssignment {
    /** Part-instance GUID of the occurrence the user picked as the wheel. */
    wheelPartGuid: string
    /** Part-instance GUID of the occurrence the user picked as the wheel's parent/chassis. */
    parentPartGuid: string
    /** World-space (assembly rest-pose) axis fit — center in metres, axis normalized. */
    axisFit: WheelAxis
}

/**
 * Removes `partGuid` from any RigidGroup it's currently listed in. Warns if `otherPartGuid` is still
 * listed in the same group -- MirabufParser's bandage pass may re-merge them after ungrouping.
 */
function removeFromRigidGroups(assembly: mirabuf.Assembly, partGuid: string, otherPartGuid: string): void {
    const rigidGroups = assembly.data?.joints?.rigidGroups
    if (!rigidGroups) return

    rigidGroups.forEach((group, index) => {
        if (!group.occurrences) return
        const idx = group.occurrences.indexOf(partGuid)
        if (idx === -1) return

        if (group.occurrences.includes(otherPartGuid)) {
            console.warn(`[WheelJointBuilder] RigidGroup[${index}] still contains both wheel and parent parts.`)
        }
        group.occurrences.splice(idx, 1)
    })
}

/**
 * Mutates `assembly` in place: for each assignment, removes the wheel part from any RigidGroup it's
 * currently fused into (so MirabufParser's ancestral-break pass can split it into its own rigid node)
 * and synthesizes a REVOLUTE Joint + JointInstance tagged as a wheel, matching the exact userData
 * convention PhysicsSystem.isWheel()/WheelDetector already expect.
 *
 * Radius/width for the Jolt wheel constraint are intentionally NOT set here — PhysicsSystem's existing
 * AABB-based inference (createWheelConstraint / resolveWheelRadii) picks those up automatically once the
 * joint exists. This function only needs to get origin + axis right.
 *
 * Joint.origin is centimetres, Y-up, ASSEMBLY space (`parser.globalTransforms`) NOT a live scene
 * matrix, which bakes in the physics body's current world transform.
 */
export function applyWheelAssignments(assembly: mirabuf.Assembly, assignments: WheelAssignment[]): void {
    const joints = assembly.data?.joints
    if (!joints) throw new Error("Assembly has no joints container")

    joints.jointDefinitions ??= {}
    joints.jointInstances ??= {}

    assignments.forEach((assignment, i) => {
        removeFromRigidGroups(assembly, assignment.wheelPartGuid, assignment.parentPartGuid)

        const token = `manual_wheel_${crypto.randomUUID()}`
        const name = `Manual Wheel ${i + 1}`

        const origin: mirabuf.IVector3 = {
            x: assignment.axisFit.center.x * 100,
            y: assignment.axisFit.center.y * 100,
            z: assignment.axisFit.center.z * 100,
        }
        const axis: mirabuf.IVector3 = {
            x: assignment.axisFit.axis.x,
            y: assignment.axisFit.axis.y,
            z: assignment.axisFit.axis.z,
        }

        joints.jointDefinitions![token] = {
            info: { GUID: token, name, version: 1 },
            origin,
            jointMotionType: mirabuf.joint.JointMotion.REVOLUTE,
            rotational: {
                rotationalFreedom: {
                    axis,
                    dynamics: { damping: 0, friction: 0 },
                    value: 0,
                },
            },
            // Basis/radius inference keys off isURDFImport(assembly), not a per-joint tag.
            userData: { data: { wheel: "true", wheelType: "0" } },
        }

        joints.jointInstances![token] = {
            info: { GUID: token, name, version: 1 },
            parentPart: assignment.parentPartGuid,
            childPart: assignment.wheelPartGuid,
            jointReference: token,
            offset: { x: 0, y: 0, z: 0 },
        }
    })
}
