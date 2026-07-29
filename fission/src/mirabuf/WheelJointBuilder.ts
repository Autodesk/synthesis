import { mirabuf } from "@/proto/mirabuf"
import type { WheelAxis } from "@/util/geometry/WheelAxisFit"

/** Prefix for throwaway separator joints; never made into real constraints. */
export const MANUAL_SEPARATOR_JOINT_PREFIX = "manual_separator_"

/** Prefix for RigidGroups added to keep an isolated part's former neighbours bandaged together. */
const MANUAL_BRIDGE_GROUP_PREFIX = "manual_bridge_"

/** userData flag marking a wheel whose parent body is a steering pod rather than the chassis. */
export const STEERED_WHEEL_KEY = "steeredWheel"

export interface WheelAssignment {
    /** Part-instance GUID of the occurrence the user picked as the wheel. */
    wheelPartGuid: string
    /** Part-instance GUID of the occurrence the user picked as the wheel's parent/chassis or pod. */
    parentPartGuid: string
    /** World-space (assembly rest-pose) axis fit — center in metres, axis normalized. */
    axisFit: WheelAxis
    /** True when parentPartGuid is a steering pod, so the wheel rolls in a direction the pod turns. */
    steered?: boolean
}

export interface PodAssignment {
    /** Part-instance GUID of the occurrence the user picked as the swerve module pod. */
    podPartGuid: string
    /** Part-instance GUID of the occurrence the user picked as the pod's parent/chassis. */
    parentPartGuid: string
    /** World-space (assembly rest-pose) pivot point, in metres. */
    origin: { x: number; y: number; z: number }
}

/** Design-hierarchy adjacency, which is what MirabufParser splits rigid nodes along. */
interface DesignTree {
    parentOf: Map<string, string>
    childrenOf: Map<string, string[]>
}

function buildDesignTree(assembly: mirabuf.Assembly): DesignTree {
    const parentOf = new Map<string, string>()
    const childrenOf = new Map<string, string[]>()

    const walk = (node: mirabuf.INode) => {
        if (!node.children?.length) return
        const children = node.children.filter(child => child.value).map(child => child.value!)
        if (node.value) childrenOf.set(node.value, children)
        for (const child of node.children) {
            if (node.value && child.value) parentOf.set(child.value, node.value)
            walk(child)
        }
    }

    assembly.designHierarchy?.nodes?.forEach(walk)

    return { parentOf, childrenOf }
}

/**
 * Adds a throwaway joint whose only job is to make MirabufParser start a new rigid node.
 *
 * The parser breaks every joint at the divergent children of its two endpoints' lowest common
 * ancestor, so a joint between a part and its own design parent breaks exactly on that part.
 */
function addSeparatorJoint(joints: mirabuf.joint.IJoints, parentPart: string, childPart: string, name: string): void {
    const token = `${MANUAL_SEPARATOR_JOINT_PREFIX}${crypto.randomUUID()}`

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
        parentPart,
        childPart,
        jointReference: token,
        offset: { x: 0, y: 0, z: 0 },
    }
}

/**
 * Drops `parts` from every RigidGroup so they can't be bandaged back onto the chassis.
 *
 * A conservative URDF import encodes the whole robot as a chain of two-occurrence groups, one per
 * discarded fixed joint. Removing a part from such a group deletes it (a one-occurrence group
 * bandages nothing), which would strand the neighbours that part used to link. So every part's
 * former neighbours are first collected into one replacement group that closes the gap around it.
 */
function excludeFromRigidGroups(assembly: mirabuf.Assembly, parts: ReadonlySet<string>): void {
    const rigidGroups = assembly.data?.joints?.rigidGroups
    if (!rigidGroups) return

    const bridges: mirabuf.joint.IRigidGroup[] = []
    for (const part of parts) {
        const neighbours = new Set<string>()
        for (const group of rigidGroups) {
            // Larger groups survive the removal, so only two-occurrence groups need bridging.
            if (group.occurrences?.length !== 2 || !group.occurrences.includes(part)) continue
            for (const occurrence of group.occurrences) {
                if (occurrence !== part && !parts.has(occurrence)) neighbours.add(occurrence)
            }
        }

        if (neighbours.size > 1) {
            bridges.push({ name: `${MANUAL_BRIDGE_GROUP_PREFIX}${part}`, occurrences: [...neighbours] })
        }
    }
    rigidGroups.push(...bridges)

    for (let index = rigidGroups.length - 1; index >= 0; index--) {
        const group = rigidGroups[index]
        if (!group.occurrences) continue

        const before = group.occurrences.length
        group.occurrences = group.occurrences.filter(guid => !parts.has(guid))
        if (group.occurrences.length === before) continue

        if (group.occurrences.length < 2) rigidGroups.splice(index, 1)
    }
}

/**
 * Makes each part in `parts` its own rigid node, holding exactly that one occurrence.
 *
 * Two things have to happen for that. A separator joint against the part's design parent forces the
 * parser to break there, and a separator joint against each of the part's design children peels
 * them back off (the parser rounds every unassigned part up into its nearest assigned ancestor).
 * Those children stay in their RigidGroups, so they re-bandage onto the chassis where they belong.
 *
 * Isolation is deliberately per-occurrence rather than per-subtree. The design hierarchy of an
 * Onshape URDF export is a chain of fastener relationships, not assembly containment: one swerve
 * module's base plate is an ancestor of another module's wheel, and the top-level ancestor of all
 * four wheels is a single bearing. A subtree there routinely spans the entire robot.
 */
function isolateParts(assembly: mirabuf.Assembly, parts: readonly string[]): void {
    const joints = assembly.data!.joints!
    const { parentOf, childrenOf } = buildDesignTree(assembly)
    const isolated = new Set(parts)
    const added = new Set<string>()

    const separate = (parentPart: string, childPart: string, name: string) => {
        const key = `${parentPart}->${childPart}`
        if (added.has(key)) return
        added.add(key)
        addSeparatorJoint(joints, parentPart, childPart, name)
    }

    for (const part of isolated) {
        const parent = parentOf.get(part)
        if (parent) separate(parent, part, `Isolate ${part}`)
        for (const child of childrenOf.get(part) ?? []) separate(part, child, `Peel ${child}`)
    }

    excludeFromRigidGroups(assembly, isolated)
}

function toCentimetres(v: { x: number; y: number; z: number }): mirabuf.IVector3 {
    return { x: v.x * 100, y: v.y * 100, z: v.z * 100 }
}

/** Adds an untagged vertical REVOLUTE hinge (chassis -> pod) per assignment. */
function addPodJoints(assembly: mirabuf.Assembly, assignments: readonly PodAssignment[]): void {
    const joints = assembly.data!.joints!

    assignments.forEach((assignment, i) => {
        const token = `manual_pod_${crypto.randomUUID()}`
        const name = `Manual Pod ${i + 1}`

        joints.jointDefinitions![token] = {
            info: { GUID: token, name, version: 1 },
            origin: toCentimetres(assignment.origin),
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
    })
}

/** Adds a wheel-tagged REVOLUTE joint (chassis or pod -> wheel) per assignment. */
function addWheelJoints(assembly: mirabuf.Assembly, assignments: readonly WheelAssignment[]): void {
    const joints = assembly.data!.joints!

    assignments.forEach((assignment, i) => {
        const token = `manual_wheel_${crypto.randomUUID()}`
        const name = `Manual Wheel ${i + 1}`

        joints.jointDefinitions![token] = {
            info: { GUID: token, name, version: 1 },
            origin: toCentimetres(assignment.axisFit.center),
            jointMotionType: mirabuf.joint.JointMotion.REVOLUTE,
            rotational: {
                rotationalFreedom: {
                    axis: {
                        x: assignment.axisFit.axis.x,
                        y: assignment.axisFit.axis.y,
                        z: assignment.axisFit.axis.z,
                    },
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
                    ...(assignment.steered ? { [STEERED_WHEEL_KEY]: "true" } : {}),
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
    })
}

/**
 * Mutates `assembly` in place, adding the joints for a batch of manual wheel and pod picks and
 * isolating every picked occurrence into its own rigid node.
 *
 * Wheels and pods are applied together because isolation has to see the whole set at once: bridging
 * a pod out of its RigidGroups must not re-bandage it to a wheel that is also being isolated.
 */
export function applyManualAssignments(
    assembly: mirabuf.Assembly,
    wheels: readonly WheelAssignment[],
    pods: readonly PodAssignment[]
): void {
    if (wheels.length === 0 && pods.length === 0) return

    const joints = assembly.data?.joints
    if (!joints) throw new Error("Assembly has no joints container")

    joints.jointDefinitions ??= {}
    joints.jointInstances ??= {}
    joints.rigidGroups ??= []

    addPodJoints(assembly, pods)
    addWheelJoints(assembly, wheels)

    isolateParts(assembly, [...pods.map(pod => pod.podPartGuid), ...wheels.map(wheel => wheel.wheelPartGuid)])
}
