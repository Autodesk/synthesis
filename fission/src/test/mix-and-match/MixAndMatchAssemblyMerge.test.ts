import { describe, expect, test } from "vitest"
import { mergeAssemblies } from "@/mix-and-match/MixAndMatchAssemblyMerge"
import type { ComponentState, TimelineState } from "@/mix-and-match/MixAndMatchTimeline"
import type { ComponentId, TransformArray } from "@/mix-and-match/MixAndMatchTypes"
import { mirabuf } from "@/proto/mirabuf"
import { convertMirabufTransformToThreeMatrix } from "@/util/TypeConversions"

const IDENTITY: TransformArray = [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1]

/** A mira row-major, centimeter-scaled identity-rotation transform placed at `(x, y, z)` meters. */
function miraTranslation(x: number, y: number, z: number): number[] {
    return [1, 0, 0, x * 100, 0, 1, 0, y * 100, 0, 0, 1, z * 100, 0, 0, 0, 1]
}

/** A `WeldLink.relativeOffset`-shaped (THREE.Matrix4.elements, meters) identity-rotation translation. */
function weldOffset(x: number, y: number, z: number): TransformArray {
    return [1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, x, y, z, 1]
}

/** A minimal, parseable single-part library assembly: one part, grounded, at `transform`. */
function fixtureAssembly(partGuid: string, transform: number[]): mirabuf.Assembly {
    const defGuid = `${partGuid}-def`
    return mirabuf.Assembly.create({
        dynamic: true,
        designHierarchy: mirabuf.GraphContainer.create({ nodes: [{ value: partGuid, children: [] }] }),
        data: mirabuf.AssemblyData.create({
            parts: mirabuf.Parts.create({
                partDefinitions: { [defGuid]: { physicalData: { mass: 1 } } },
                partInstances: {
                    [partGuid]: {
                        info: { GUID: partGuid },
                        partDefinitionReference: defGuid,
                        transform: { spatialMatrix: transform },
                    },
                },
            }),
            joints: mirabuf.joint.Joints.create({
                jointInstances: { grounded: { parts: { nodes: [{ value: partGuid }] } } },
                jointDefinitions: {},
                rigidGroups: [],
                motorDefinitions: {},
            }),
        }),
    })
}

function component(id: ComponentId, weld?: ComponentState["weld"], transform: TransformArray = IDENTITY): ComponentState {
    return { id, libraryPartRef: `lib-${id}`, transform, weld }
}

function stateOf(...components: ComponentState[]): TimelineState {
    return { components: new Map(components.map(c => [c.id, c])) }
}

describe("Mix and Match Assembly Merge", () => {
    test("A Build With No Welds Produces One Assembly Per Component", () => {
        const state = stateOf(component("a"), component("b"))
        const assemblies = new Map([
            ["a", fixtureAssembly("part-a", miraTranslation(0, 0, 0))],
            ["b", fixtureAssembly("part-b", miraTranslation(1, 0, 0))],
        ])

        const merged = mergeAssemblies(state, assemblies)

        expect(merged).toHaveLength(2)
    })

    test("Grafts A Welded Child Under The Root, Baking Its Pose From Its Live Transform", () => {
        const state = stateOf(
            component("a"),
            component("b", { parentId: "a", relativeOffset: weldOffset(2, 0, 0) }, weldOffset(2, 0, 0))
        )
        const assemblies = new Map([
            ["a", fixtureAssembly("part-a", miraTranslation(10, 0, 0))],
            // Where "part-b" originally sat is irrelevant - its live transform overrides it entirely.
            ["b", fixtureAssembly("part-b", miraTranslation(999, -50, 3))],
        ])

        const [merged] = mergeAssemblies(state, assemblies)

        const partInstances = merged.data!.parts!.partInstances!
        expect(Object.keys(partInstances)).toHaveLength(2)
        expect(partInstances["part-a"]).toBeDefined()

        const namespacedPartB = Object.keys(partInstances).find(key => key !== "part-a")!
        const bakedGlobal = convertMirabufTransformToThreeMatrix(partInstances[namespacedPartB].transform!)
        // Root sits at x=10m, child's live transform is +2m along the root's frame, so it lands at x=12m.
        expect(bakedGlobal.elements[12]).toBeCloseTo(12, 4)
        expect(bakedGlobal.elements[13]).toBeCloseTo(0, 4)
        expect(bakedGlobal.elements[14]).toBeCloseTo(0, 4)
    })

    test("Bakes The Pose From The Live Transform, Not A Stale Weld Offset Left Behind By A Later Move", () => {
        // "b" was welded to "a" at +2m, recording that in relativeOffset - then dragged to +5m.
        // applyMove keeps b.transform current but never touches the weld's own relativeOffset.
        const state = stateOf(
            component("a"),
            component("b", { parentId: "a", relativeOffset: weldOffset(2, 0, 0) }, weldOffset(5, 0, 0))
        )
        const assemblies = new Map([
            ["a", fixtureAssembly("part-a", miraTranslation(0, 0, 0))],
            ["b", fixtureAssembly("part-b", miraTranslation(0, 0, 0))],
        ])

        const [merged] = mergeAssemblies(state, assemblies)

        const partInstances = merged.data!.parts!.partInstances!
        const namespacedPartB = Object.keys(partInstances).find(key => key !== "part-a")!
        const bakedGlobal = convertMirabufTransformToThreeMatrix(partInstances[namespacedPartB].transform!)
        // Must land at the post-move x=5m, not the stale weld-time x=2m.
        expect(bakedGlobal.elements[12]).toBeCloseTo(5, 4)
    })

    test("Folds The Weld Into One Shared RigidGroup Anchored On The Parent's Root Part", () => {
        const state = stateOf(component("a"), component("b", { parentId: "a", relativeOffset: weldOffset(2, 0, 0) }))
        const assemblies = new Map([
            ["a", fixtureAssembly("part-a", miraTranslation(0, 0, 0))],
            ["b", fixtureAssembly("part-b", miraTranslation(0, 0, 0))],
        ])

        const [merged] = mergeAssemblies(state, assemblies)

        const rigidGroups = merged.data!.joints!.rigidGroups!
        expect(rigidGroups).toHaveLength(1)
        expect(rigidGroups[0].occurrences).toContain("part-a")
        expect(rigidGroups[0].occurrences).toHaveLength(2)
    })

    test("Namespaces Two Spawns Of The Same Library Part So Their GUIDs Never Collide", () => {
        const state = stateOf(
            component("a"),
            component("b", { parentId: "a", relativeOffset: weldOffset(1, 0, 0) }),
            component("c", { parentId: "a", relativeOffset: weldOffset(-1, 0, 0) })
        )
        // "b" and "c" are two spawns of the exact same library part, so they share every GUID.
        const sharedPart = fixtureAssembly("part-shared", miraTranslation(0, 0, 0))
        const assemblies = new Map([
            ["a", fixtureAssembly("part-a", miraTranslation(0, 0, 0))],
            ["b", sharedPart],
            ["c", sharedPart],
        ])

        const [merged] = mergeAssemblies(state, assemblies)

        const partInstances = merged.data!.parts!.partInstances!
        expect(Object.keys(partInstances)).toHaveLength(3)
        expect(partInstances["part-a"]).toBeDefined()
        expect(Object.keys(partInstances).filter(key => key.endsWith(":part-shared"))).toHaveLength(2)
    })

    test("Rejects Nothing: An Unwelded Component Alongside A Welded Pair Stays Its Own Assembly", () => {
        const state = stateOf(
            component("a"),
            component("b", { parentId: "a", relativeOffset: weldOffset(1, 0, 0) }),
            component("c")
        )
        const assemblies = new Map([
            ["a", fixtureAssembly("part-a", miraTranslation(0, 0, 0))],
            ["b", fixtureAssembly("part-b", miraTranslation(0, 0, 0))],
            ["c", fixtureAssembly("part-c", miraTranslation(5, 0, 0))],
        ])

        const merged = mergeAssemblies(state, assemblies)

        expect(merged).toHaveLength(2)
        const soloAssembly = merged.find(assembly => Object.keys(assembly.data!.parts!.partInstances!).length === 1)!
        expect(soloAssembly.data!.parts!.partInstances!["part-c"]).toBeDefined()
    })
})
