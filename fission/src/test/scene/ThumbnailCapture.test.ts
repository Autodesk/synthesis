import type * as THREE from "three"
import { Box3, Vector3 } from "three"
import { describe, expect, test, vi } from "vitest"
import {
    boxCorners,
    collectInstanceBoundsPoints,
    computeThumbnailFraming,
    createThumbnailCamera,
    THUMBNAIL_FILL,
} from "@/systems/scene/ThumbnailCapture"
import { getMiraInstance } from "@/test/GetAssets.ts"

vi.mock("@/systems/World", () => ({
    default: {
        sceneRenderer: {
            setupMaterial: vi.fn(),
        },
    },
}))

// testing shapes for thumbnail generation
const SHAPES: ReadonlyArray<[string, Box3]> = [
    ["robot", new Box3(new Vector3(-0.4, 0, -0.5), new Vector3(0.4, 1.4, 0.5))],
    ["field", new Box3(new Vector3(-11, 0, -4.6), new Vector3(11, 3, 4.6))],
    ["off-origin", new Box3(new Vector3(20, 5, -30), new Vector3(21, 6, -29))],
]

const ASSEMBLIES = ["DOZER", 2018] as const

function fillRatio(points: readonly THREE.Vector3[]): number {
    const framing = computeThumbnailFraming(points)
    if (!framing) throw new Error("expected a framing")

    const camera = createThumbnailCamera(framing)
    let ratio = 0
    const projected = new Vector3()
    for (const point of points) {
        projected.copy(point).project(camera)
        ratio = Math.max(ratio, Math.abs(projected.x) / THUMBNAIL_FILL.x, Math.abs(projected.y) / THUMBNAIL_FILL.y)
    }
    return ratio
}

function rounded(vector: THREE.Vector3): number[] {
    return vector.toArray().map(component => Number(component.toFixed(4)))
}

describe("computeThumbnailFraming", () => {
    test("returns undefined when there is nothing to frame", () => {
        expect(computeThumbnailFraming([])).toBeUndefined()
    })

    test.each(SHAPES)("frames the %s tightly, with every corner in view", (_name, box) => {
        expect(fillRatio(boxCorners(box))).toBeCloseTo(1)
    })

    test.each(ASSEMBLIES)("frames %s the same way it always has", async name => {
        const instance = await getMiraInstance(name)
        if (!instance) throw new Error(`could not load ${name}`)

        const points = collectInstanceBoundsPoints([...instance.meshes.values()].flat())
        expect(points.length).toBeGreaterThan(0)
        expect(fillRatio(points)).toBeCloseTo(1)

        const framing = computeThumbnailFraming(points)
        if (!framing) throw new Error("expected a framing")
        expect({ position: rounded(framing.position), lookAt: rounded(framing.lookAt) }).toMatchSnapshot()
    })
})
