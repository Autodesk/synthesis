import { describe, expect, test } from "vitest"
import MirabufCachingService, { MiraType } from "../../mirabuf/MirabufLoader.ts"
import MirabufParser, { type RigidNodeReadOnly } from "../../mirabuf/MirabufParser.ts"
import { mirabuf } from "../../proto/mirabuf"

describe("Mirabuf Parser Tests", () => {
    test("Generate Rigid Nodes (Dozer_v9.mira)", async () => {
        const spikeMira = await MirabufCachingService.cacheRemote(
            "/api/mira/robots/Dozer_v9.mira",
            MiraType.ROBOT
        ).then(x => MirabufCachingService.get(x!.id, MiraType.ROBOT))

        const t = new MirabufParser(spikeMira!)
        const rn = [...t.rigidNodes.values()]

        expect(filterNonPhysicsNodes(rn, spikeMira!).length).toBe(7)

        // Validate joints
        const jointValidation = validateJoints(spikeMira!)
        expect(jointValidation.isValid).toBe(true)
        expect(jointValidation.jointCount).toBe(6)
        expect(jointValidation.wheelJoints).toBe(6)
        expect(jointValidation.allJoints).toContain(mirabuf.joint.JointMotion.REVOLUTE) // Wheels are revolute joints
        expect(jointValidation.allJoints).not.toContain(mirabuf.joint.JointMotion.SLIDER) // Dozer has no slider joints
    })

    /*
     * Multi-Joint Wheels robot contains
     * - 4 wheels (4 revolute joints)
     * - 2 additional revolute joints
     * - 2 slider joints
     * Mira File: https://synthesis.autodesk.com/api/mira/private/Multi-Joint_Wheels_v0.mira
     */
    test("Generate Rigid Nodes (Multi-Joint Wheels)", async () => {
        const spikeMira = await MirabufCachingService.cacheRemote(
            "/api/mira/private/Multi-Joint_Wheels_v0.mira",
            MiraType.ROBOT
        ).then(x => MirabufCachingService.get(x!.id, MiraType.ROBOT))

        const t = new MirabufParser(spikeMira!)
        const rn = [...t.rigidNodes.values()]

        expect(filterNonPhysicsNodes(rn, spikeMira!).length).toBe(9)

        // Validate joints
        const jointValidation = validateJoints(spikeMira!)
        expect(jointValidation.isValid).toBe(true)
        expect(jointValidation.jointCount).toBe(8)

        // Validate joint type distribution
        const revoluteJoints = jointValidation.allJoints.filter(j => j === mirabuf.joint.JointMotion.REVOLUTE)
        const sliderJoints = jointValidation.allJoints.filter(j => j === mirabuf.joint.JointMotion.SLIDER)

        expect(revoluteJoints.length).toBe(6) // Should have 6 revolute joints (4 wheels + 2 additional)
        expect(sliderJoints.length).toBe(2) // Should have 2 slider joints
        expect(jointValidation.wheelJoints).toBe(4) // Should have 4 wheel joints
    })

    test("Generate Rigid Nodes (FRC Field 2018_v13.mira)", async () => {
        const field = await MirabufCachingService.cacheRemote(
            "/api/mira/Fields/FRC Field 2018_v13.mira",
            MiraType.FIELD
        ).then(x => MirabufCachingService.get(x!.id, MiraType.FIELD))
        const t = new MirabufParser(field!)

        expect(filterNonPhysicsNodes([...t.rigidNodes.values()], field!).length).toBe(34)
    })
})

function filterNonPhysicsNodes(nodes: RigidNodeReadOnly[], mira: mirabuf.Assembly): RigidNodeReadOnly[] {
    return nodes.filter(x => {
        for (const part of x.parts) {
            const inst = mira.data!.parts!.partInstances![part]!
            const def = mira.data!.parts!.partDefinitions![inst.partDefinitionReference!]!
            if (def.bodies && def.bodies.length > 0) {
                return true
            }
        }
        return false
    })
}

interface JointValidationResult {
    isValid: boolean
    jointCount: number
    allJoints: mirabuf.joint.JointMotion[]
    wheelJoints: number
    errors: string[]
    warnings: string[]
}

function validateJoints(assembly: mirabuf.Assembly): JointValidationResult {
    const result: JointValidationResult = {
        isValid: true,
        jointCount: 0,
        allJoints: [],
        wheelJoints: 0,
        errors: [],
        warnings: [],
    }

    const jointData = assembly.data?.joints
    if (!jointData) {
        result.errors.push("No joint data found in assembly")
        result.isValid = false
        return result
    }

    // Validate joint definitions and instances
    const jointDefinitions = jointData.jointDefinitions || {}
    const jointInstances = jointData.jointInstances || {}

    // Count non-grounded joints
    const nonGroundedJoints = Object.entries(jointInstances).filter(([key]) => key !== "grounded")
    result.jointCount = nonGroundedJoints.length

    // Validate each joint
    for (const [jointId, jointInstance] of nonGroundedJoints) {
        try {
            // Check if joint definition exists
            const jointDef = jointDefinitions[jointInstance.jointReference!]
            if (!jointDef) {
                result.errors.push(
                    `Joint instance '${jointId}' references missing definition '${jointInstance.jointReference}'`
                )
                result.isValid = false
                continue
            }

            // Get all joints
            if (jointDef.jointMotionType !== null && jointDef.jointMotionType !== undefined)
                result.allJoints.push(jointDef.jointMotionType)

            // Check for wheel joints
            if (
                jointDef.userData?.data?.wheel === "true" ||
                (jointDef.jointMotionType === mirabuf.joint.JointMotion.REVOLUTE &&
                    jointDef.userData?.data?.wheelType !== undefined)
            ) {
                result.wheelJoints++
            }

            // Validate joint motion type specific properties
            switch (jointDef.jointMotionType) {
                case mirabuf.joint.JointMotion.REVOLUTE:
                    if (!jointDef.rotational) {
                        result.errors.push(`Revolute joint '${jointId}' missing rotational definition`)
                        result.isValid = false
                    }
                    break
                case mirabuf.joint.JointMotion.SLIDER:
                    if (!jointDef.prismatic) {
                        result.errors.push(`Slider joint '${jointId}' missing prismatic definition`)
                        result.isValid = false
                    }
                    break
                case mirabuf.joint.JointMotion.BALL:
                    // Note: Ball joint properties are validated differently in the mirabuf format
                    if (!jointDef.custom) {
                        result.warnings.push(`Ball joint '${jointId}' may be missing ball-specific configuration`)
                    }
                    break
                case mirabuf.joint.JointMotion.CUSTOM:
                    if (!jointDef.custom) {
                        result.errors.push(`Custom joint '${jointId}' missing custom definition`)
                        result.isValid = false
                    }
                    break
            }

            // Validate joint has an origin
            if (!jointDef.origin) {
                result.warnings.push(`Joint '${jointId}' has no origin defined`)
            }
        } catch (error) {
            result.errors.push(`Error validating joint '${jointId}': ${error}`)
            result.isValid = false
        }
    }

    // Validate rigid groups if they exist
    if (jointData.rigidGroups) {
        for (const rigidGroup of jointData.rigidGroups) {
            if (!rigidGroup.occurrences || rigidGroup.occurrences.length < 2) {
                result.warnings.push(`Rigid group '${rigidGroup.name}' has fewer than 2 occurrences`)
            }
        }
    }

    return result
}

// function printRigidNodeParts(nodes: RigidNodeReadOnly[], mira: mirabuf.Assembly) {
//     nodes.forEach(x => {
//         console.log(`[ ${x.name} ]:`);
//         x.parts.forEach(y => console.log(`-> '${mira.data!.parts!.partInstances![y]!.info!.name!}'`));
//         console.log('');
//     });
// }
