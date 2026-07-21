import MirabufParser from "@/mirabuf/MirabufParser"
import { mirabuf } from "@/proto/mirabuf"
import { isWheel } from "@/systems/physics/ConstraintSettingsUtilities"
import type { WheelAxis } from "@/util/geometry/WheelAxisFit"

/**
 * Prefix for throwaway "separator" joints from addWheelSeparatorJoints. PhysicsSystem.createJointsFromParser
 * skips joints with this prefix -- they only steer MirabufParser's rigid-node topology, never real constraints.
 */
export const WHEEL_SEPARATOR_JOINT_PREFIX = "manual_wheel_separator_"

export interface WheelAssignment {
    /** Part-instance GUID of the occurrence the user picked as the wheel. */
    wheelPartGuid: string
    /** Part-instance GUID of the occurrence the user picked as the wheel's parent/chassis. */
    parentPartGuid: string
    /** World-space (assembly rest-pose) axis fit — center in metres, axis normalized. */
    axisFit: WheelAxis
}

/**
 * Runs a throwaway MirabufParser pass with RigidGroups temporarily emptied to learn exactly which parts
 * MirabufParser's ancestral-break split (independent of any RigidGroup bandaging) assigns to the wheel's
 * own rigid node now that `assembly.data.joints.jointInstances` already contains the new wheel joint.
 * This is the wheel's TRUE post-split subtree -- typically not just the wheel part itself but also
 * whatever sits between it and the design-tree's common ancestor with the parent (axle, hub, pulley,
 * bearings, ...). Those intermediate parts matter: on a "conservative" URDF import (see
 * applyConservativeURDFImport in URDFLoader.ts), every non-wheel joint along that same chain -- e.g.
 * wheel-to-axle, axle-to-bracket, bracket-to-chassis -- was already turned into its own RigidGroup, so an
 * unbroken chain of untouched groups can still bandage the wheel's new rigid node right back to the
 * parent (or, transitively, to the rest of the whole robot) even after the wheel's own GUID is removed
 * from its immediate neighbor's group.
 */
function computeWheelSubtreeParts(assembly: mirabuf.Assembly, wheelPartGuid: string): ReadonlySet<string> {
    const joints = assembly.data!.joints!
    const savedRigidGroups = joints.rigidGroups
    joints.rigidGroups = []
    try {
        const dryRunParser = new MirabufParser(assembly)
        const node = dryRunParser.partToNodeMap.get(wheelPartGuid)
        if (!node) {
            console.warn(`[WheelJointBuilder] Dry-run parse found no rigid node for wheel=${wheelPartGuid}.`)
            return new Set([wheelPartGuid])
        }
        return new Set(node.parts)
    } finally {
        joints.rigidGroups = savedRigidGroups
    }
}

/**
 * Strips every part in `wheelSubtree` (see computeWheelSubtreeParts) out of every RigidGroup's
 * occurrences, dropping any group left with fewer than 2 occurrences. Removing the wheel's whole real
 * subtree -- not just its own GUID -- is what actually breaks every conservative-import RigidGroup edge
 * along the chain from the wheel to wherever the user picked as its parent.
 */
function unbandageWheelSubtree(assembly: mirabuf.Assembly, wheelSubtree: ReadonlySet<string>): void {
    const rigidGroups = assembly.data?.joints?.rigidGroups
    if (!rigidGroups) {
        console.log(`[WheelJointBuilder] wheel subtree (${wheelSubtree.size} parts): assembly has no rigidGroups.`)
        return
    }

    let touchedGroups = 0
    let removedOccurrences = 0
    for (let index = rigidGroups.length - 1; index >= 0; index--) {
        const group = rigidGroups[index]
        if (!group.occurrences) continue

        const before = group.occurrences.length
        group.occurrences = group.occurrences.filter(guid => !wheelSubtree.has(guid))
        const removed = before - group.occurrences.length
        if (removed === 0) continue

        touchedGroups++
        removedOccurrences += removed
        if (group.occurrences.length < 2) rigidGroups.splice(index, 1)
    }

    console.log(
        `[WheelJointBuilder] wheel subtree (${wheelSubtree.size} parts): touched ${touchedGroups} RigidGroup(s), removed ${removedOccurrences} occurrence(s), ${rigidGroups.length} RigidGroup(s) remain.`
    )
    // Names, not just count -- distinguishes real wheel-module hardware from an ancestral-break
    // overshoot pulling in unrelated sibling subassemblies.
    const names = [...wheelSubtree].map(guid => assembly.data?.parts?.partInstances?.[guid]?.info?.name ?? guid).sort()
    console.log(`[WheelJointBuilder] wheel subtree parts: ${JSON.stringify(names)}`)
}

/**
 * Adds a throwaway separator joint between every PAIR of picked wheels. Purely structural -- PhysicsSystem
 * skips these by prefix (WHEEL_SEPARATOR_JOINT_PREFIX). Works around a MirabufParser quirk: when several
 * wheels are flat siblings under one branch well above where each diverges from its (distant) parent pick,
 * every wheel-vs-parent joint resolves the same ancestral-break node for that shared branch. MirabufParser
 * processes joints one at a time and the last write wins, so the wheels collapse into one shared rigid
 * node -- skid-steer drive then fights itself since "left" and "right" wheels are the same body.
 *
 * A joint between SIBLING wheels breaks at their own leaf parts directly (equally deep, no shared branch to
 * overshoot), so pre-claiming each wheel this way before the coarse round-up keeps each wheel's own
 * hardware in its own rigid node.
 *
 * Full pairwise, not just a chain: a chain only protects each wheel transitively, and one coarse
 * intermediate break can still let two non-adjacent wheels share a node. Full pairwise needs no knowledge
 * of the design tree's shape.
 *
 * Also chains in pre-existing wheel joints, not just this batch -- otherwise applying wheels one Apply at
 * a time (batch of 1, no sibling to pair) would skip this protection entirely.
 */
function addWheelSeparatorJoints(assembly: mirabuf.Assembly, assignments: WheelAssignment[]): void {
    const joints = assembly.data!.joints!

    const existingWheelParts = Object.values(joints.jointInstances!)
        .filter(inst => {
            const jDef = joints.jointDefinitions?.[inst.jointReference!] as mirabuf.joint.Joint | undefined
            return jDef && isWheel(jDef)
        })
        .map(inst => inst.childPart!)
    const wheelParts = [...existingWheelParts, ...assignments.map(a => a.wheelPartGuid)]
    console.log(
        `[WheelJointBuilder] separator pairs over (${wheelParts.length} wheels, ${existingWheelParts.length} pre-existing): ${JSON.stringify(wheelParts)}`
    )

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

/**
 * Mutates `assembly` in place: for each assignment, synthesizes a REVOLUTE Joint + JointInstance tagged
 * as a wheel (matching the exact userData convention PhysicsSystem.isWheel()/WheelDetector already
 * expect), then strips the wheel's whole post-split rigid-node subtree out of every RigidGroup so
 * MirabufParser's bandage pass can't re-fuse it back to the parent (see computeWheelSubtreeParts).
 *
 * Radius and width come from the wheel pick's circle fit, stored on the joint's userData (PhysicsSystem
 * prefers these over AABB-based inference -- see getExplicitWheelRadius/Width). AABB inference reads the
 * bounds of whatever rigid body the wheel ends up in -- for a manually-assigned wheel that can be a whole
 * fused belt-driven wheel train, giving wrong radius AND wrong (often oversized) width. Circle fit reads
 * straight off the clicked mesh's own geometry instead, unaffected by shared rigid-body hardware.
 *
 * Joint.origin is centimetres, Y-up, ASSEMBLY space (`parser.globalTransforms`) NOT a live scene
 * matrix, which bakes in the physics body's current world transform.
 */
export function applyWheelAssignments(assembly: mirabuf.Assembly, assignments: WheelAssignment[]): void {
    const joints = assembly.data?.joints
    if (!joints) throw new Error("Assembly has no joints container")

    joints.jointDefinitions ??= {}
    joints.jointInstances ??= {}

    // Must run BEFORE the per-assignment loop below (see addWheelSeparatorJoints) so each wheel's rigid
    // node stays isolated from siblings, and the dry-run subtree computation below sees final topology.
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
            // Basis inference keys off isURDFImport(assembly), not a per-joint tag. wheelRadius/wheelWidth
            // are centimetres, matching origin's convention (see getExplicitWheelRadius in PhysicsSystem.ts).
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

        // Must run AFTER the joint above is added -- the dry-run parse needs it present to correctly
        // ancestrally-break the wheel's subtree away from the parent.
        const wheelSubtree = computeWheelSubtreeParts(assembly, assignment.wheelPartGuid)
        unbandageWheelSubtree(assembly, wheelSubtree)
    })
}
