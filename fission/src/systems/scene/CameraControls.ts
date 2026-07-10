import * as THREE from "three"
import { MiraType } from "@/mirabuf/MirabufLoader"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import EventSystem from "@/systems/EventSystem"
import World from "../World"
import type ScreenInteractionHandler from "./ScreenInteractionHandler"
import {
    type InteractionEnd,
    type InteractionMove,
    type InteractionStart,
    PRIMARY_MOUSE_INTERACTION,
    SECONDARY_MOUSE_INTERACTION,
} from "./ScreenInteractionHandler"

export type CameraControlsType = "Target"

export enum CameraMode {
    Follow = "Follow",
    Locked = "Locked",
    Face = "Face",
}

export abstract class CameraControls {
    private _controlsType: CameraControlsType

    public abstract set enabled(val: boolean)
    public abstract get enabled(): boolean

    public get controlsType() {
        return this._controlsType
    }

    public constructor(controlsType: CameraControlsType) {
        this._controlsType = controlsType
    }

    public abstract update(deltaT: number): void

    public abstract dispose(): void
}

export interface SphericalCoords {
    theta: number
    phi: number
    r: number
}

type PointerType = -1 | 0 | 1 | 2

const CO_MAX_ZOOM = 40.0
const CO_MIN_ZOOM = 0.1
const CO_MAX_PHI = Math.PI / 2.1
const CO_MIN_PHI = -Math.PI / 2.1

const CO_SENSITIVITY_ZOOM = 4.0
const CO_FACE_ZOOM_SENSITIVITY = 0.4

const CO_DEFAULT_ZOOM = 3.5
const CO_DEFAULT_PHI = -Math.PI / 6.0
const CO_DEFAULT_THETA = -Math.PI / 4.0

const DEG2RAD = Math.PI / 180.0

/**
 * Creates a pseudo frustum of the perspective camera to scale the mouse movement to something relative to the scenes dimensions and scale
 *
 * @param camera Main Camera
 * @param distanceFromFocus Distance from the focus point
 * @param originalMovement Original movement of the mouse across the screen
 * @returns Augmented movement to scale to the scenes relative dimensions
 */
function augmentMovement(
    camera: THREE.Camera,
    distanceFromFocus: number,
    originalMovement: [number, number]
): [number, number] {
    const aspect = (camera as THREE.PerspectiveCamera)?.aspect ?? 1.0
    const fov: number | undefined = (camera as THREE.PerspectiveCamera)?.getEffectiveFOV()
    if (fov) {
        const res: [number, number] = [
            (2 *
                distanceFromFocus *
                Math.tan(Math.min((Math.PI * 0.9) / 2, (DEG2RAD * fov * aspect) / 2)) *
                originalMovement[0]) /
                window.innerWidth,
            (2 * distanceFromFocus * Math.tan((DEG2RAD * fov) / 2) * originalMovement[1]) / window.innerHeight,
        ]
        return res
    } else {
        return originalMovement
    }
}

function easeInOutCubic(t: number): number {
    return t < 0.5 ? 4 * Math.pow(t, 3) : 1 - Math.pow(2 - 2 * t, 3) / 2
}

/**
 * Interpolates between two rotation/translation matrices
 * Position is lerped and rotation is slerped
 */
function blendTransforms(out: THREE.Matrix4, start: THREE.Matrix4, end: THREE.Matrix4, t: number): void {
    const startPos = new THREE.Vector3()
    const startRot = new THREE.Quaternion()
    const endPos = new THREE.Vector3()
    const endRot = new THREE.Quaternion()
    const scratch = new THREE.Vector3()

    start.decompose(startPos, startRot, scratch)
    end.decompose(endPos, endRot, scratch)

    out.compose(startPos.lerp(endPos, t), startRot.slerp(endRot, t), new THREE.Vector3(1, 1, 1))
}

/** Tracks an in-progress smooth re-settle of the camera onto its focus provider. */
interface FocusBlend {
    progress: number
    duration: number
    startFocus: THREE.Matrix4
}

export class CustomTargetControls extends CameraControls {
    private _enabled = true

    private _mainCamera: THREE.Camera

    private _activePointerType: PointerType
    private _nextCoords: SphericalCoords
    private _coords: SphericalCoords
    private _focus: THREE.Matrix4

    private _focusProvider: MirabufSceneObject | undefined
    private _isExplicitlyUnfocused: boolean = false
    private _pendingResync: THREE.Vector3 | undefined
    private _focusBlend: FocusBlend | undefined

    private _mode: CameraMode = CameraMode.Follow
    private _focusPosition: THREE.Vector3 = new THREE.Vector3()

    public get isFocusedOnRobot(): boolean {
        return this._focusProvider?.miraType === MiraType.ROBOT
    }

    public get mode(): CameraMode {
        return this._mode
    }

    public set mode(val: CameraMode) {
        if (val === this._mode) return

        if (val === CameraMode.Face && this._focusProvider?.miraType === MiraType.FIELD) return

        this._mode = val
        EventSystem.dispatch("CameraModeChangedEvent", { mode: val })

        if (val === CameraMode.Face) {
            // Face mode drives the camera directly and ignores target coords
            this._pendingResync = undefined
            this._focusPosition.copy(this._mainCamera.position)
        } else {
            this.syncCoordsFromWorldPos(this._mainCamera.position)
        }
    }

    /**
     * Recalculates target coords so the camera stays at worldPos after the focus changes.
     * In Locked mode uses robot-local space. in Follow/Face uses world-space offset from focus.
     */
    private syncCoordsFromWorldPos(worldPos: THREE.Vector3): void {
        const ref =
            this._mode === CameraMode.Locked && this._focusProvider
                ? worldPos.clone().applyMatrix4(new THREE.Matrix4().copy(this._focus).invert())
                : worldPos.clone().sub(new THREE.Vector3().setFromMatrixPosition(this._focus))

        const r = ref.length()
        if (r < 0.01) return

        this.setImmediateCoordinates({
            theta: Math.atan2(ref.x, ref.z),
            phi: -Math.asin(THREE.MathUtils.clamp(ref.y / r, -1, 1)),
            r,
        })
    }

    private onFocusProviderChanged(): void {
        EventSystem.dispatch("CameraFocusChangedEvent", { focusProvider: this._focusProvider })

        if (!this._focusProvider) return

        if (this._focusProvider.miraType === MiraType.FIELD && this._mode === CameraMode.Face) {
            // Don't allow Face mode for fields, default back to Follow mode
            this.mode = CameraMode.Follow
        }

        if (this._mode !== CameraMode.Face) {
            // Capture the camera's current world position.
            // The coord re-sync is deferred to update() so it runs after _focus is refreshed
            this._pendingResync = this._mainCamera.position.clone()
        }
    }

    private _interactionHandler: ScreenInteractionHandler

    /*
     * NOTE
     * These getter and setters and necessary for adhering to the `CameraControls` interface
     */
    public set enabled(val: boolean) {
        this._enabled = val
    }
    public get enabled(): boolean {
        return this._enabled
    }

    public set focusProvider(provider: MirabufSceneObject | undefined) {
        if (provider === this._focusProvider) return
        this._focusProvider = provider
        if (provider !== undefined) {
            this._isExplicitlyUnfocused = false
        }
        this.onFocusProviderChanged()
    }
    public get focusProvider() {
        return this._focusProvider
    }

    /**
     * Explicitly unfocus the camera (user-initiated action)
     */
    public unfocus(): void {
        this._focusProvider = undefined
        this._isExplicitlyUnfocused = true
        this.onFocusProviderChanged()

        if (this._mode !== CameraMode.Follow) {
            const worldPos =
                this._mode === CameraMode.Face ? this._focusPosition.clone() : this._mainCamera.position.clone()
            this.syncCoordsFromWorldPos(worldPos)
        }
    }

    public get coords(): SphericalCoords {
        return this._coords
    }

    public get isBlendingFocus(): boolean {
        return this._focusBlend !== undefined
    }

    /**
     * Smoothly re-settles the camera onto a focus provider after that object has moved
     *  - Follow: the focus point pans to the object's new position.
     *  - Locked: position and rotation blend together, so the camera ends locked at the
     *    same relative orientation it had before.
     *  - Face: no blend is needed because the camera already tracks the object every frame.
     */
    public settleOntoFocus(target: MirabufSceneObject | undefined, duration: number = 1.0): void {
        if (!target) return

        // Attach directly (rather than via the focusProvider setter) so we skip the coord
        // resync that would otherwise snap the camera and fight the blend.
        this._focusProvider = target
        this._isExplicitlyUnfocused = false
        EventSystem.dispatch("CameraFocusChangedEvent", { focusProvider: target })

        if (this._mode === CameraMode.Face) return

        this._focusBlend = { progress: 0, duration, startFocus: this._focus.clone() }
    }

    public get focus(): THREE.Matrix4 {
        return this._focus
    }

    public set focus(matrix: THREE.Matrix4) {
        this._focus.copy(matrix)
    }

    public constructor(mainCamera: THREE.Camera, interactionHandler: ScreenInteractionHandler) {
        super("Target")

        this._mainCamera = mainCamera
        this._interactionHandler = interactionHandler

        this._nextCoords = {
            theta: CO_DEFAULT_THETA,
            phi: CO_DEFAULT_PHI,
            r: CO_DEFAULT_ZOOM,
        }
        this._coords = {
            theta: CO_DEFAULT_THETA,
            phi: CO_DEFAULT_PHI,
            r: CO_DEFAULT_ZOOM,
        }
        this._activePointerType = -1

        // Identity
        this._focus = new THREE.Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)

        this._interactionHandler.interactionStart = e => this.interactionStart(e)
        this._interactionHandler.interactionEnd = e => this.interactionEnd(e)
        this._interactionHandler.interactionMove = e => this.interactionMove(e)
    }

    /**
     * Finds a suitable fallback focus target when the current focus is no longer available.
     * Prioritizes robots first, then fields, then any other MirabufSceneObject.
     */
    private findFallbackFocus(mirabufObjects?: MirabufSceneObject[]): MirabufSceneObject | undefined {
        mirabufObjects ??= World.getOwnObjects()

        const robots = mirabufObjects.filter(obj => obj.miraType === MiraType.ROBOT)
        const fields = mirabufObjects.filter(obj => obj.miraType === MiraType.FIELD)

        return robots[0] ?? fields[0] ?? mirabufObjects[0]
    }

    /**
     * Validates that the current focus provider still exists in the scene.
     * If not, automatically finds a suitable replacement.
     */
    private validateFocusProvider(): void {
        if (!World.sceneRenderer?.sceneObjects || this.isBlendingFocus) {
            return
        }
        const mirabufObjects = World.sceneRenderer.mirabufSceneObjects.getAll()

        const currentProviderMissing = this._focusProvider && !mirabufObjects.includes(this._focusProvider)
        const needsFallback = currentProviderMissing || (!this._focusProvider && !this._isExplicitlyUnfocused)

        if (needsFallback) {
            const newProvider = this.findFallbackFocus(mirabufObjects)
            if (newProvider !== undefined) {
                this._focusProvider = newProvider
                this._isExplicitlyUnfocused = false

                this.onFocusProviderChanged()
            }
        }
    }

    public interactionEnd(end: InteractionEnd) {
        /**
         * If Pointer is already down, and the button that is being
         * released is the primary button, make Pointer not be down
         */
        if (end.interactionType == this._activePointerType) {
            this._activePointerType = -1
        }
    }

    public interactionStart(start: InteractionStart) {
        // If primary button, make Pointer be down
        if (this._activePointerType < start.interactionType) {
            switch (start.interactionType) {
                case PRIMARY_MOUSE_INTERACTION:
                    this._activePointerType = PRIMARY_MOUSE_INTERACTION
                    break
                case SECONDARY_MOUSE_INTERACTION:
                    this._activePointerType = SECONDARY_MOUSE_INTERACTION
                    break
                default:
                    break
            }
        }
    }

    public interactionMove(move: InteractionMove) {
        if (this._mode === CameraMode.Face) {
            // Face mode drives the camera directly, so only zoom is allowed
            if (move.scale) this.zoomFaceMode(move.scale)
            return
        }

        if (move.movement) {
            if (this._activePointerType == PRIMARY_MOUSE_INTERACTION) {
                // Add the movement of the mouse to the _currentPos
                this._nextCoords.theta -= move.movement[0]
                this._nextCoords.phi -= move.movement[1]
            } else if (this._activePointerType == SECONDARY_MOUSE_INTERACTION && this._mode !== CameraMode.Locked) {
                this._focusProvider = undefined

                const orientation = new THREE.Quaternion().setFromEuler(this._mainCamera.rotation)

                const augmentedMovement = augmentMovement(this._mainCamera, this._coords.r, [
                    move.movement[0],
                    move.movement[1],
                ])

                const pan = new THREE.Vector3(-augmentedMovement[0], augmentedMovement[1], 0).applyQuaternion(
                    orientation
                )
                const newPos = new THREE.Vector3().setFromMatrixPosition(this._focus)
                newPos.add(pan)
                this._focus.setPosition(newPos)
            }
        }

        if (move.scale) {
            this._nextCoords.r += move.scale
        }
    }

    /**
     * Zooms the fixed Face mode camera by moving it along its view axis toward or away from the robot.
     * Sensitivity scales with distance so zoom feels consistent at any range.
     */
    private zoomFaceMode(scale: number): void {
        const robotPos = new THREE.Vector3().setFromMatrixPosition(this._focus)
        const offset = this._focusPosition.clone().sub(robotPos)
        const distance = offset.length()
        if (distance < 0.01) return

        const newDistance = THREE.MathUtils.clamp(
            distance * (1 + scale * CO_FACE_ZOOM_SENSITIVITY),
            CO_MIN_ZOOM,
            CO_MAX_ZOOM
        )
        this._focusPosition.copy(robotPos).addScaledVector(offset.divideScalar(distance), newDistance)
    }

    // Fixed camera position, always faces towards robot
    private focusMode() {
        const robotPos = new THREE.Vector3().setFromMatrixPosition(this._focus)
        this._mainCamera.position.copy(this._focusPosition)
        this._mainCamera.lookAt(robotPos)
    }

    public getCurrentCoordinates(): SphericalCoords {
        return { ...this._coords }
    }

    public setImmediateCoordinates(coords: Partial<SphericalCoords>) {
        if (coords.theta !== undefined) {
            this._coords.theta = coords.theta
            this._nextCoords.theta = coords.theta
        }
        if (coords.phi !== undefined) {
            this._coords.phi = Math.min(CO_MAX_PHI, Math.max(CO_MIN_PHI, coords.phi))
            this._nextCoords.phi = this._coords.phi
        }
        if (coords.r !== undefined) {
            this._coords.r = Math.min(CO_MAX_ZOOM, Math.max(CO_MIN_ZOOM, coords.r))
            this._nextCoords.r = this._coords.r
        }
    }

    public animateToOrientation(theta: number, phi: number, duration: number = 500) {
        const startCoords = { ...this._coords }
        const targetCoords = { theta, phi, r: this._coords.r }

        let startTime: number | null = null

        const animate = (timestamp: number) => {
            if (!startTime) startTime = timestamp

            const elapsed = timestamp - startTime
            const progress = Math.min(elapsed / duration, 1)

            const easeOut = 1 - Math.pow(1 - progress, 3)

            this._coords.theta = startCoords.theta + (targetCoords.theta - startCoords.theta) * easeOut
            this._coords.phi = startCoords.phi + (targetCoords.phi - startCoords.phi) * easeOut

            if (progress < 1) {
                requestAnimationFrame(animate)
            }
        }

        requestAnimationFrame(animate)
    }

    private updateFocusTransform(deltaT: number): void {
        if (!this._focusProvider) return

        if (!this._focusBlend) {
            this._focusProvider.loadFocusTransform(this._focus)
            return
        }

        const target = new THREE.Matrix4()
        this._focusProvider.loadFocusTransform(target)

        this._focusBlend.progress += deltaT / this._focusBlend.duration
        const t = easeInOutCubic(Math.min(this._focusBlend.progress, 1))
        blendTransforms(this._focus, this._focusBlend.startFocus, target, t)

        if (this._focusBlend.progress >= 1) this._focusBlend = undefined
    }

    public update(deltaT: number): void {
        deltaT = Math.max(1.0 / 60.0, Math.min(1 / 144.0, deltaT))

        this.validateFocusProvider()

        if (this.enabled) this.updateFocusTransform(deltaT)

        if (this._pendingResync) {
            this.syncCoordsFromWorldPos(this._pendingResync)
            this._pendingResync = undefined
        }

        if (this._mode === CameraMode.Face && this._focusProvider) {
            this.focusMode()
            return
        }

        // Generate delta of spherical coordinates
        const omega: SphericalCoords = this.enabled
            ? {
                  theta: this._nextCoords.theta - this._coords.theta,
                  phi: this._nextCoords.phi - this._coords.phi,
                  r: this._nextCoords.r - this._coords.r,
              }
            : { theta: 0, phi: 0, r: 0 }

        this._coords.theta += omega.theta * deltaT * PreferencesSystem.getUserPreference("SceneRotationSensitivity")
        this._coords.phi += omega.phi * deltaT * PreferencesSystem.getUserPreference("SceneRotationSensitivity")
        this._coords.r += omega.r * deltaT * CO_SENSITIVITY_ZOOM * Math.pow(this._coords.r, 1.4)

        this._coords.phi = Math.min(CO_MAX_PHI, Math.max(CO_MIN_PHI, this._coords.phi))
        this._coords.r = Math.min(CO_MAX_ZOOM, Math.max(CO_MIN_ZOOM, this._coords.r))

        const deltaTransform = new THREE.Matrix4()
            .makeTranslation(0, 0, this._coords.r)
            .premultiply(
                new THREE.Matrix4().makeRotationFromEuler(
                    new THREE.Euler(this._coords.phi, this._coords.theta, 0, "YXZ")
                )
            )

        if (this._mode === CameraMode.Locked && this._focusProvider) {
            deltaTransform.premultiply(this._focus)
        } else {
            const focusPosition = new THREE.Matrix4().copyPosition(this._focus)
            deltaTransform.premultiply(focusPosition)
        }

        this._mainCamera.position.setFromMatrixPosition(deltaTransform)
        this._mainCamera.rotation.setFromRotationMatrix(deltaTransform)

        this._nextCoords = {
            theta: this._coords.theta,
            phi: this._coords.phi,
            r: this._coords.r,
        }
    }

    public dispose(): void {}
}
