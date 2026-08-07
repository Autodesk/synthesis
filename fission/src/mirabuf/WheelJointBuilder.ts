import MirabufParser from "@/mirabuf/MirabufParser"
import { mirabuf } from "@/proto/mirabuf"
import { isWheel } from "@/systems/physics/ConstraintSettingsUtilities"
import type { WheelAxis } from "@/util/geometry/WheelAxisFit"

/** Prefix for throwaway separator joints; never made into real constraints. */
export const WHEEL_SEPARATOR_JOINT_PREFIX = "manual_wheel_separator_"

export interface WheelAssignment {
    /** Part-instance GUID of the occurrence the user picked as the wheel. */
    wheelPartGuid: string
    /** Part-instance GUID of the occurrence the user picked as the wheel's parent/chassis. */
    parentPartGuid: string

    name: string
    /** World-space (assembly rest-pose) axis fit — center in metres, axis normalized. */
    axisFit: WheelAxis
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

/** Adds a throwaway separator joint between every pair of wheels (existing + new) to keep each in its own rigid node. */
function addWheelSeparatorJoints(assembly: mirabuf.Assembly, assignments: WheelAssignment[]): void {
    const joints = assembly.data!.joints!

    const existingWheelParts = Object.values(joints.jointInstances!)
        .filter(inst => {
            const jDef = joints.jointDefinitions?.[inst.jointReference!] as mirabuf.joint.Joint | undefined
            return jDef && isWheel(jDef)
        })
        .map(inst => inst.childPart!)
    const wheelParts = [...existingWheelParts, ...assignments.map(a => a.wheelPartGuid)]

    for (let i = 0; i < wheelParts.length; i++) {
        for (let j = i + 1; j < wheelParts.length; j++) {
            const token = `${WHEEL_SEPARATOR_JOINT_PREFIX}${crypto.randomUUID()}`
            const name = `Manual Wheel Separator ${i}-${j}`

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
                parentPart: wheelParts[i],
                childPart: wheelParts[j],
                jointReference: token,
                offset: { x: 0, y: 0, z: 0 },
            }
        }
    }
}
/** Sets the "reversed" attribute for all wheels in the assembly */
export function setWheelReversal(assembly: mirabuf.Assembly, reversed: boolean): void {
    const jointDefs = assembly.data?.joints?.jointDefinitions
    if (!jointDefs) throw new Error("Assembly has no joints container")
    Object.values(jointDefs).forEach(joint => {
        if (joint.userData?.data?.wheel === "true") {
            joint.userData.data.reversed = reversed ? "true" : "false"
        }
    })
}
/** Mutates assembly in place: adds a REVOLUTE wheel joint per assignment and unbandages its rigid-node subtree. */
export function applyWheelAssignments(assembly: mirabuf.Assembly, assignments: WheelAssignment[]): void {
    const joints = assembly.data?.joints
    if (!joints) throw new Error("Assembly has no joints container")

    joints.jointDefinitions ??= {}
    joints.jointInstances ??= {}

    // Must run before the per-assignment loop below.
    addWheelSeparatorJoints(assembly, assignments)

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
