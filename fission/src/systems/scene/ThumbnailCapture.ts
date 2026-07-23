import * as THREE from "three"

export const THUMBNAIL_SIZE = 512
// const THUMBNAIL_MIME_TYPE = "image/webp"
// const THUMBNAIL_QUALITY = 0.85
//
// const CAPTURE_SUPERSAMPLE = 2

// thumbnail angle constants
export const THUMBNAIL_FOV_Y_DEGREES = 45
export const THUMBNAIL_THETA = -Math.PI / 4
export const THUMBNAIL_PHI = -Math.PI / 6
export const THUMBNAIL_FILL = { x: 0.9, y: 0.7 } as const

export interface ThumbnailCaptureProps {
    renderer: THREE.WebGLRenderer
    scene: THREE.Scene
    skybox: THREE.Object3D // skybox stays visible
    targets: readonly THREE.Object3D[]
    framingPoints?: readonly THREE.Vector3[] // world-space points to frame
}

/** renders the targets to an off-screen render target */
export async function captureSceneThumbnail(props: ThumbnailCaptureProps): Promise<Blob | undefined> {
    const { renderer, scene, skybox, targets, framingPoints, size = THUMBNAIL_SIZE } = props

    return undefined
}
