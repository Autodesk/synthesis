import * as THREE from "three"
import { describe, expect, test } from "vitest"
import { mirabuf } from "@/proto/mirabuf"
import { applyWheelAssignments } from "@/mirabuf/WheelJointBuilder"

const assignment = (wheelPartGuid: string) => ({
    wheelPartGuid,
    parentPartGuid: "chassis",
    name: wheelPartGuid,
    axisFit: {
        center: new THREE.Vector3(),
        axis: new THREE.Vector3(1, 0, 0),
        radius: 0.05,
        width: 0.02,
    },
})

function makeAssembly(): mirabuf.Assembly {
    const parts = ["root", "chassis", "wheel-a", "wheel-b"]
    const identity = { spatialMatrix: [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1] }
    return new mirabuf.Assembly({
        info: { GUID: "assembly" },
        dynamic: true,
        designHierarchy: {
            nodes: [
                {
                    value: "root",
                    children: [
                        { value: "chassis", children: [] },
                        { value: "wheel-a", children: [] },
                        { value: "wheel-b", children: [] },
                    ],
                },
            ],
        },
        data: {
            parts: {
                partInstances: Object.fromEntries(
                    parts.map(guid => [
                        guid,
                        { info: { GUID: guid }, partDefinitionReference: "definition", transform: identity },
                    ])
                ),
                partDefinitions: {
                    definition: { info: { GUID: "definition" }, physicalData: { mass: 1 } },
                },
            },
            joints: {
                jointInstances: {
                    grounded: {
                        info: { GUID: "grounded" },
                        parts: { nodes: [{ value: "root" }] },
                    },
                },
                jointDefinitions: {},
                rigidGroups: [],
            },
        },
    })
}

describe("applyWheelAssignments", () => {
    test("does not create self-joints when reapplying existing wheel assignments", () => {
        const assembly = makeAssembly()
        const assignments = [assignment("wheel-a"), assignment("wheel-b")]

        applyWheelAssignments(assembly, assignments)
        expect(() => applyWheelAssignments(assembly, assignments)).not.toThrow()

        const separatorInstances = Object.values(assembly.data!.joints!.jointInstances!).filter(instance =>
            instance.info?.GUID?.startsWith("manual_wheel_separator_")
        )
        expect(separatorInstances.every(instance => instance.parentPart !== instance.childPart)).toBe(true)
    })
})
