import { mirabuf } from "@/proto/mirabuf"
import { URDF_WHEEL_TAG } from "@/urdf/URDFUserData"
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
 * Removes `partGuid` from any RigidGroup it's currently listed in. Logs what else was in the group so a
 * still-fused sibling occurrence (e.g. a separate tire/rim part not caught by this single-GUID removal)
 * is visible instead of silently re-bandaging the wheel to whatever it shared a group with.
 */
function removeFromRigidGroups(assembly: mirabuf.Assembly, partGuid: string, otherPartGuid: string): void {
    const rigidGroups = assembly.data?.joints?.rigidGroups
    if (!rigidGroups) return

    rigidGroups.forEach((group, index) => {
        if (!group.occurrences) return
        const idx = group.occurrences.indexOf(partGuid)
        if (idx === -1) return

        const remaining = group.occurrences.filter(g => g !== partGuid)
        console.debug(
            `[WheelJointBuilder] RigidGroup[${index}] contained '${partGuid}' alongside [${remaining.join(", ")}] -- removing '${partGuid}'.`
        )
        if (remaining.includes(otherPartGuid)) {
            console.warn(
                `[WheelJointBuilder] RigidGroup[${index}] also lists '${otherPartGuid}' directly -- ` +
                    `after ungrouping, MirabufParser's bandage pass may still merge them back together.`
            )
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
 * Joint.origin is stored in centimetres, Y-up, world-space at the assembly's rest pose (see
 * WheelDetector.ts and URDFConverter.ts positionToYup) -- callers must pass a world-space circle fit
 * captured while the assembly is at that rest pose.
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
            // Also tag urdfWheel so createWheelConstraint infers the vehicle's forward/up/steering axes
            // from `axis` instead of assuming the native-Fusion default orientation, which our
            // arbitrarily-picked axis (from an AABB fit, not a fixed CAD convention) won't generally match.
            userData: { data: { wheel: "true", wheelType: "0", [URDF_WHEEL_TAG]: "true" } },
        }

        joints.jointInstances![token] = {
            info: { GUID: token, name, version: 1 },
            parentPart: assignment.parentPartGuid,
            childPart: assignment.wheelPartGuid,
            jointReference: token,
            offset: { x: 0, y: 0, z: 0 },
        }

        console.debug(
            `[WheelJointBuilder] Created joint '${token}': parentPart='${assignment.parentPartGuid}' childPart='${assignment.wheelPartGuid}' ` +
                `origin(cm)=(${origin.x!.toFixed(2)}, ${origin.y!.toFixed(2)}, ${origin.z!.toFixed(2)}) axis=(${axis.x!.toFixed(3)}, ${axis.y!.toFixed(3)}, ${axis.z!.toFixed(3)})`
        )
    })
}
