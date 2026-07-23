import * as THREE from "three"

export const THUMBNAIL_SIZE = 512
const THUMBNAIL_MIME_TYPE = "image/webp"
const THUMBNAIL_QUALITY = 0.85

const CAPTURE_SUPERSAMPLE = 2

// thumbnail angle constants
export const THUMBNAIL_FOV_Y_DEGREES = 45
export const THUMBNAIL_THETA = -Math.PI / 4
export const THUMBNAIL_PHI = -Math.PI / 6
export const THUMBNAIL_FILL = { x: 0.9, y: 0.7 } as const

/* Computing thumbnail bounds */

/** unit vector from origin (relative) toward camera */
export function canonicalCameraOffset(): THREE.Vector3 {
    return new THREE.Vector3(0, 0, 1).applyEuler(new THREE.Euler(THUMBNAIL_PHI, THUMBNAIL_THETA, 0, "YXZ"))
}

function boxCorners(box: THREE.Box3): THREE.Vector3[] {
    const corners: THREE.Vector3[] = []
    for (let i = 0; i < 8; i++) {
        corners.push(
            new THREE.Vector3(
                i & 1 ? box.max.x : box.min.x,
                i & 2 ? box.max.y : box.min.y,
                i & 4 ? box.max.z : box.min.z
            )
        )
    }
    return corners
}

export interface ThumbnailFraming {
    position: THREE.Vector3
    lookAt: THREE.Vector3
}

function computeThumbnailFraming(bounds: THREE.Box3 | readonly THREE.Vector3[]): ThumbnailFraming | undefined {
    const points = bounds instanceof THREE.Box3 ? boxCorners(bounds) : bounds
    if (points.length === 0) return undefined

    const pointBounds = new THREE.Box3().setFromPoints([...points])
    const toCamera: THREE.Vector3 = canonicalCameraOffset()

    const center = pointBounds.getCenter(new THREE.Vector3())
    const basis = new THREE.Matrix4().lookAt(toCamera, new THREE.Vector3(), THREE.Object3D.DEFAULT_UP)
    const right = new THREE.Vector3().setFromMatrixColumn(basis, 0)
    const up = new THREE.Vector3().setFromMatrixColumn(basis, 1)

    const halfFovY = THREE.MathUtils.degToRad(THUMBNAIL_FOV_Y_DEGREES) / 2
    // square frame
    const tanX = Math.tan(halfFovY) * THUMBNAIL_FILL.x
    const tanY = Math.tan(halfFovY) * THUMBNAIL_FILL.y

    /* camera distance D from the center must satisfy `D >= p.toCamera + l / tan` for every point */
    let distance = 0
    const relative = new THREE.Vector3()
    for (const point of points) {
        relative.copy(point).sub(center)
        const lateral = Math.max(Math.abs(relative.dot(right)) / tanX, Math.abs(relative.dot(up)) / tanY)
        distance = Math.max(distance, relative.dot(toCamera) + lateral)
    }
    if (distance <= 0) return undefined

    return { position: toCamera.multiplyScalar(distance).add(center), lookAt: center }
}

function computeTargetBounds(targets: readonly THREE.Object3D[]): THREE.Box3 {
    const bounds = new THREE.Box3()
    const targetBox = new THREE.Box3()
    for (const target of targets) {
        if (target instanceof THREE.BatchedMesh) {
            target.computeBoundingBox()
            target.computeBoundingSphere()
            if (!target.boundingBox) continue
            target.updateWorldMatrix(true, false)
            targetBox.copy(target.boundingBox).applyMatrix4(target.matrixWorld)
        } else {
            targetBox.setFromObject(target)
        }
        bounds.union(targetBox)
    }
    return bounds
}

/* rendering thumbnail */

export interface ThumbnailCaptureProps {
    renderer: THREE.WebGLRenderer
    scene: THREE.Scene
    skybox: THREE.Object3D // skybox stays visible
    targets: readonly THREE.Object3D[]
    framingPoints?: readonly THREE.Vector3[] // world-space points to frame
}

/** renders the targets to an off-screen render target */
export async function captureSceneThumbnail(props: ThumbnailCaptureProps): Promise<Blob | undefined> {
    const { renderer, scene, skybox, targets, framingPoints } = props

    const framing = computeThumbnailFraming(framingPoints?.length ? framingPoints : computeTargetBounds(targets))
    if (!framing) return undefined

    const cameraDistance = framing.position.distanceTo(framing.lookAt)
    const camera = new THREE.PerspectiveCamera(THUMBNAIL_FOV_Y_DEGREES, 1, Math.min(0.1, cameraDistance / 10), 2000)
    camera.position.copy(framing.position)
    camera.lookAt(framing.lookAt)
    camera.updateMatrixWorld()

    const renderSize = THUMBNAIL_SIZE * CAPTURE_SUPERSAMPLE
    const pixels = new Uint8Array(renderSize * renderSize * 4)

    encodePixels(pixels, renderSize)

    return undefined
}

function createSquareCanvas(size: number): OffscreenCanvas | HTMLCanvasElement {
    if (typeof OffscreenCanvas !== "undefined") return new OffscreenCanvas(size, size)
    const canvas = document.createElement("canvas")
    canvas.width = size
    canvas.height = size
    return canvas
}

/** encoding rendered offscreen scene */
async function encodePixels(pixels: Uint8Array, renderSize: number): Promise<Blob | undefined> {
    const flipped = new Uint8ClampedArray(pixels.length)

    const rowBytes = renderSize * 4
    for (let y = 0; y < renderSize; y++) {
        flipped.set(pixels.subarray(y * rowBytes, (y + 1) * rowBytes), (renderSize - 1 - y) * rowBytes)
    }

    const full = createSquareCanvas(renderSize)
    const fullContext = full.getContext("2d") as OffscreenCanvasRenderingContext2D | null
    if (!fullContext) return undefined
    fullContext.putImageData(new ImageData(flipped, renderSize, renderSize), 0, 0)

    const scaled = createSquareCanvas(THUMBNAIL_SIZE)
    const scaledContext = scaled.getContext("2d") as OffscreenCanvasRenderingContext2D | null
    if (!scaledContext) return undefined
    scaledContext.imageSmoothingEnabled = true
    scaledContext.imageSmoothingQuality = "high"
    scaledContext.drawImage(full, 0, 0, THUMBNAIL_SIZE, THUMBNAIL_SIZE)

    // converting canvas to blob
    if (scaled instanceof HTMLCanvasElement) {
        return new Promise(resolve =>
            scaled.toBlob(blob => resolve(blob ?? undefined), THUMBNAIL_MIME_TYPE, THUMBNAIL_QUALITY)
        )
    }
    return scaled.convertToBlob({ type: THUMBNAIL_MIME_TYPE, quality: THUMBNAIL_QUALITY })
}
