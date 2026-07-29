import MirabufParser from "@/mirabuf/MirabufParser"
import { mirabuf } from "@/proto/mirabuf"
import { isWheel } from "@/systems/physics/ConstraintSettingsUtilities"
import type { WheelAxis } from "@/util/geometry/WheelAxisFit"

/** Prefix for throwaway separator joints; never made into real constraints. */
export const MANUAL_SEPARATOR_JOINT_PREFIX = "manual_separator_"

export interface WheelAssignment {
    /** Part-instance GUID of the occurrence the user picked as the wheel. */
    wheelPartGuid: string
    /** Part-instance GUID of the occurrence the user picked as the wheel's parent/chassis or pod. */
    parentPartGuid: string
    /** World-space (assembly rest-pose) axis fit — center in metres, axis normalized. */
    axisFit: WheelAxis
}

export interface PodAssignment {
    /** Part-instance GUID of the occurrence the user picked as the swerve module pod. */
    podPartGuid: string
    /** Part-instance GUID of the occurrence the user picked as the pod's parent/chassis. */
    parentPartGuid: string
    /** World-space (assembly rest-pose) pivot point, in metres. */
    origin: { x: number; y: number; z: number }
}

/** Dry-run parse with RigidGroups emptied to find the wheel's true post-split subtree. */
function computeWheelSubtreeParts(assembly: mirabuf.Assembly, wheelPartGuid: string): ReadonlySet<string> {
    const joints = assembly.data!.joints!
    const savedRigidGroups = joints.rigidGroups
    joints.rigidGroups = []
    try {
        const dryRunParser = new MirabufParser(assembly)
        const node = dryRunParser.partToNodeMap.get(wheelPartGuid)
        if (!node) {
            return new Set([wheelPartGuid])
        }
        return new Set(node.parts)
    } finally {
        joints.rigidGroups = savedRigidGroups
    }
}

/** Removes wheelSubtree's parts from every RigidGroup, dropping groups left under 2 occurrences. */
function unbandageWheelSubtree(assembly: mirabuf.Assembly, wheelSubtree: ReadonlySet<string>): void {
    const rigidGroups = assembly.data?.joints?.rigidGroups
    if (!rigidGroups) {
        return
    }

    for (let index = rigidGroups.length - 1; index >= 0; index--) {
        const group = rigidGroups[index]
        if (!group.occurrences) continue

        const before = group.occurrences.length
        group.occurrences = group.occurrences.filter(guid => !wheelSubtree.has(guid))
        if (group.occurrences.length === before) continue

        if (group.occurrences.length < 2) rigidGroups.splice(index, 1)
    }
}

/** Adds a throwaway separator joint between every pair of the given parts to keep each in its own rigid node. */
function addSeparatorJoints(assembly: mirabuf.Assembly, parts: string[], label: string): void {
    const joints = assembly.data!.joints!

    for (let i = 0; i < parts.length; i++) {
        for (let j = i + 1; j < parts.length; j++) {
            const token = `${MANUAL_SEPARATOR_JOINT_PREFIX}${crypto.randomUUID()}`
            const name = `${label} ${i}-${j}`

            joints.jointDefinitions![token] = {
                info: { GUID: token, name, version: 1 },
                origin: { x: 0, y: 0, z: 0 },
                jointMotionType: mirabuf.joint.JointMotion.REVOLUTE,
                rotational: {
                    rotationalFreedom: {
                        axis: { x: 0, y: 1, z: 0 },
                        dynamics: { damping: 0, friction: 0 },
                        value: 0,
                    },
                },
            }

            joints.jointInstances![token] = {
                info: { GUID: token, name, version: 1 },
                parentPart: parts[i],
                childPart: parts[j],
                jointReference: token,
                offset: { x: 0, y: 0, z: 0 },
            }
        }
    }
}

/** Mutates assembly in place: adds a REVOLUTE wheel joint per assignment and unbandages its rigid-node subtree. */
export function applyWheelAssignments(assembly: mirabuf.Assembly, assignments: WheelAssignment[]): void {
    const joints = assembly.data?.joints
    if (!joints) throw new Error("Assembly has no joints container")

    joints.jointDefinitions ??= {}
    joints.jointInstances ??= {}

    // Must run before the per-assignment loop below.
    const existingWheelParts = Object.values(joints.jointInstances!)
        .filter(inst => {
            const jDef = joints.jointDefinitions?.[inst.jointReference!] as mirabuf.joint.Joint | undefined
            return jDef && isWheel(jDef)
        })
        .map(inst => inst.childPart!)
    addSeparatorJoints(
        assembly,
        [...existingWheelParts, ...assignments.map(a => a.wheelPartGuid)],
        "Manual Wheel Separator"
    )

    assignments.forEach((assignment, i) => {
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
            // wheelRadius/wheelWidth are centimetres, matching origin.
            userData: {
                data: {
                    wheel: "true",
                    wheelType: "0",
                    wheelRadius: String(assignment.axisFit.radius * 100),
                    wheelWidth: String(assignment.axisFit.width * 100),
                },
            },
        }

        joints.jointInstances![token] = {
            info: { GUID: token, name, version: 1 },
            parentPart: assignment.parentPartGuid,
            childPart: assignment.wheelPartGuid,
            jointReference: token,
            offset: { x: 0, y: 0, z: 0 },
        }

        // Must run after the joint above is added.
        const wheelSubtree = computeWheelSubtreeParts(assembly, assignment.wheelPartGuid)
        unbandageWheelSubtree(assembly, wheelSubtree)
    })
}

/**
 * Mutates assembly in place: adds an untagged vertical REVOLUTE hinge (chassis -> pod) per
 * assignment and unbandages its rigid-node subtree. No userData marker is needed -- an ordinary
 * near-vertical hinge is all `SynthesisBrain.detectSwerve()` looks for at runtime.
 */
export function applyPodAssignments(assembly: mirabuf.Assembly, assignments: PodAssignment[]): void {
    if (assignments.length === 0) return

    const joints = assembly.data?.joints
    if (!joints) throw new Error("Assembly has no joints container")

    joints.jointDefinitions ??= {}
    joints.jointInstances ??= {}

    // Must run before the per-assignment loop below.
    addSeparatorJoints(
        assembly,
        assignments.map(a => a.podPartGuid),
        "Manual Pod Separator"
    )

    assignments.forEach((assignment, i) => {
        const token = `manual_pod_${crypto.randomUUID()}`
        const name = `Manual Pod ${i + 1}`

        const origin: mirabuf.IVector3 = {
            x: assignment.origin.x * 100,
            y: assignment.origin.y * 100,
            z: assignment.origin.z * 100,
        }

        joints.jointDefinitions![token] = {
            info: { GUID: token, name, version: 1 },
            origin,
            jointMotionType: mirabuf.joint.JointMotion.REVOLUTE,
            rotational: {
                rotationalFreedom: {
                    axis: { x: 0, y: 1, z: 0 },
                    dynamics: { damping: 0, friction: 0 },
                    value: 0,
                },
            },
        }

        joints.jointInstances![token] = {
            info: { GUID: token, name, version: 1 },
            parentPart: assignment.parentPartGuid,
            childPart: assignment.podPartGuid,
            jointReference: token,
            offset: { x: 0, y: 0, z: 0 },
        }

        // Must run after the joint above is added.
        const podSubtree = computeWheelSubtreeParts(assembly, assignment.podPartGuid)
        unbandageWheelSubtree(assembly, podSubtree)
    })
}
