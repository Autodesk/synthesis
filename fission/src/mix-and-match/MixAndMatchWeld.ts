import type Jolt from "@synthesis.adsk/jolt-physics"
import World from "@/systems/World"
import JOLT from "@/util/loading/JoltSyncLoader"

/**
 * Rigidly joins two bodies where they currently sit.
 *
 * Auto-detecting the point in world space locks whatever relative pose the pair is already in, which
 * is what a finished build wants: the user placed the parts, so the weld holds them exactly there.
 *
 * @param   parentBody Body to weld onto. Not destroyed by this function.
 * @param   childBody  Body being attached. Not destroyed by this function.
 * @returns The constraint, owned by the physics system.
 */
export function weldBodies(parentBody: Jolt.Body, childBody: Jolt.Body): Jolt.Constraint {
    const settings = new JOLT.FixedConstraintSettings()
    settings.mSpace = JOLT.EConstraintSpace_WorldSpace
    settings.mAutoDetectPoint = true

    // createConstraint takes ownership of the settings.
    return World.physicsSystem.createConstraint(settings, parentBody, childBody)
}
