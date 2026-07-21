import MirabufParser from "@/mirabuf/MirabufParser"
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
    // Names, not just count -- need this to tell "wheel module hardware" apart from an
    // ancestral-break overshoot that swept in unrelated sibling subassemblies.
    const names = [...wheelSubtree]
        .map(guid => assembly.data?.parts?.partInstances?.[guid]?.info?.name ?? guid)
        .sort()
    console.log(`[WheelJointBuilder] wheel subtree parts: ${JSON.stringify(names)}`)
}

/**
 * Mutates `assembly` in place: for each assignment, synthesizes a REVOLUTE Joint + JointInstance tagged
 * as a wheel (matching the exact userData convention PhysicsSystem.isWheel()/WheelDetector already
 * expect), then strips the wheel's whole post-split rigid-node subtree out of every RigidGroup so
 * MirabufParser's bandage pass can't re-fuse it back to the parent (see computeWheelSubtreeParts).
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

        // Must run AFTER the joint above is added -- the dry-run parse needs it present to correctly
        // ancestrally-break the wheel's subtree away from the parent.
        const wheelSubtree = computeWheelSubtreeParts(assembly, assignment.wheelPartGuid)
        unbandageWheelSubtree(assembly, wheelSubtree)
    })
}
