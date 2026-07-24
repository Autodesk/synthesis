import * as THREE from "three"
import { describe, expect, test } from "vitest"
import { computeThumbnailFraming, THUMBNAIL_FILL, THUMBNAIL_FOV_Y_DEGREES } from "@/systems/scene/ThumbnailCapture"

// testing shapes for thumbnail generation
const SHAPES: ReadonlyArray<[string, THREE.Box3]> = [
    ["robot", new THREE.Box3(new THREE.Vector3(-0.4, 0, -0.5), new THREE.Vector3(0.4, 1.4, 0.5))],
    ["field", new THREE.Box3(new THREE.Vector3(-11, 0, -4.6), new THREE.Vector3(11, 3, 4.6))],
    ["off-origin", new THREE.Box3(new THREE.Vector3(20, 5, -30), new THREE.Vector3(21, 6, -29))],
]

/** tests the framing depending on different sizes of mira object */
function fillRatio(box: THREE.Box3): number {
    const framing = computeThumbnailFraming(box)
    if (!framing) throw new Error("expected a framing")

    const camera = new THREE.PerspectiveCamera(THUMBNAIL_FOV_Y_DEGREES, 1, 0.01, 5000)
    camera.position.copy(framing.position)
    camera.lookAt(framing.lookAt)
    camera.updateMatrixWorld()

    const corner = new THREE.Vector3()
    let ratio = 0
    for (let i = 0; i < 8; i++) {
        corner
            .set(i & 1 ? box.max.x : box.min.x, i & 2 ? box.max.y : box.min.y, i & 4 ? box.max.z : box.min.z)
            .project(camera)
        ratio = Math.max(ratio, Math.abs(corner.x) / THUMBNAIL_FILL.x, Math.abs(corner.y) / THUMBNAIL_FILL.y)
    }
    return ratio
}

describe("computeThumbnailFraming", () => {
    test("returns undefined when there is nothing to frame", () => {
        expect(computeThumbnailFraming([])).toBeUndefined()
    })

    test.each(SHAPES)("frames the %s tightly, with every corner in view", (_name, box) => {
        expect(fillRatio(box)).toBeCloseTo(1)
    })
})
