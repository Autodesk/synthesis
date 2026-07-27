import * as THREE from "three"
import { MiraType } from "@/mirabuf/MirabufLoader"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import type { CameraPoint } from "@/systems/preferences/PreferenceTypes"
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

export type CameraControlsType = "Target" | "FieldView"

export enum CameraMode {
    FOLLOW = "Follow",
    LOCKED = "Locked",
    FACE = "Face",
}

type PointerType = -1 | 0 | 1 | 2

export abstract class CameraControls {
    private _controlsType: CameraControlsType

    protected _mainCamera: THREE.Camera
    protected _interactionHandler: ScreenInteractionHandler
    protected _enabled = true
    protected _activePointerType: PointerType = -1

    public get controlsType() {
        return this._controlsType
    }

    public set enabled(val: boolean) {
        this._enabled = val
    }
    public get enabled(): boolean {
        return this._enabled
    }

    public constructor(
        controlsType: CameraControlsType,
        mainCamera: THREE.Camera,
        interactionHandler: ScreenInteractionHandler
    ) {
        this._controlsType = controlsType
        this._mainCamera = mainCamera
        this._interactionHandler = interactionHandler

        this._interactionHandler.interactionStart = e => this.interactionStart(e)
        this._interactionHandler.interactionEnd = e => this.interactionEnd(e)
        this._interactionHandler.interactionMove = e => this.interactionMove(e)
    }

    /** Tracks which pointer button (primary/secondary) initiated the active gesture. */
    public interactionStart(start: InteractionStart): void {
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

    /** Clears the active pointer once its button is released. */
    public interactionEnd(end: InteractionEnd): void {
        if (end.interactionType === this._activePointerType) {
            this._activePointerType = -1
        }
    }

    public abstract interactionMove(move: InteractionMove): void

    public abstract update(deltaT: number): void

    /** Unbinds this control's interaction callbacks. Override to release additional resources. */
    public dispose(): void {
        this._interactionHandler.interactionStart = undefined
        this._interactionHandler.interactionEnd = undefined
        this._interactionHandler.interactionMove = undefined
    }
}

export interface SphericalCoords {
    theta: number
    phi: number
    r: number
}

const CO_MAX_ZOOM = 40.0
const CO_MIN_ZOOM = 0.1
const CO_MAX_PHI = Math.PI / 2.1
const CO_MIN_PHI = -Math.PI / 2.1

const CO_SENSITIVITY_ZOOM = 4.0
const CO_FIXED_DOLLY_SENSITIVITY = 0.3

const CO_DEFAULT_ZOOM = 3.5
const CO_DEFAULT_PHI = -Math.PI / 6.0
const CO_DEFAULT_THETA = -Math.PI / 4.0

const DEG2RAD = Math.PI / 180.0

function clampPhi(phi: number): number {
    return THREE.MathUtils.clamp(phi, CO_MIN_PHI, CO_MAX_PHI)
}

function clampZome(r: number): number {
    return THREE.MathUtils.clamp(r, CO_MIN_ZOOM, CO_MAX_ZOOM)
}

/** Eases a zoom distance one frame toward a target, decelerating as it approaches CO_MIN_ZOOM */
function easeZoomDistance(current: number, target: number, deltaT: number): number {
    const eased = current + (target - current) * deltaT * CO_SENSITIVITY_ZOOM * Math.pow(current, 1.4)
    return clampZome(eased)
}

/** Accumulates raw scroll/pinch zoom input into a target distance and eases toward it over time (via {@link easeZoomDistance}). */
class ZoomEase {
    private _current: number
    private _target: number

    public constructor(initial: number) {
        this._current = initial
        this._target = initial
    }

    /** The current eased distance. */
    public get current(): number {
        return this._current
    }

    /** Resets to a known-good distance, discarding any zoom input in flight. */
    public reset(distance: number): void {
        this._current = distance
        this._target = distance
    }

    /** Accumulates raw scroll/pinch input onto the target distance. */
    public addInput(scale: number): void {
        this._target += scale
    }

    /**
     * Eases the current distance one frame toward the accumulated target and returns the signed change
     * applied this frame.
     */
    public step(deltaT: number): number {
        const eased = easeZoomDistance(this._current, this._target, deltaT)
        const delta = eased - this._current
        this._current = eased
        this._target = eased
        return delta
    }
}

const DEFAULT_COORDS: SphericalCoords = {
    theta: CO_DEFAULT_THETA,
    phi: CO_DEFAULT_PHI,
    r: CO_DEFAULT_ZOOM,
}

/** World-space forward (view) direction of a camera. */
function cameraForward(camera: THREE.Camera): THREE.Vector3 {
    return new THREE.Vector3(0, 0, -1).applyQuaternion(camera.quaternion)
}

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
    private _nextTheta = CO_DEFAULT_THETA
    private _nextPhi = CO_DEFAULT_PHI
    private _orbitZoom = new ZoomEase(CO_DEFAULT_ZOOM)
    private _coords: SphericalCoords
    private _focus: THREE.Matrix4

    private _focusProvider: MirabufSceneObject | undefined
    private _isExplicitlyUnfocused: boolean = false
    private _pendingResync: THREE.Vector3 | undefined
    private _focusBlend: FocusBlend | undefined

    private _mode: CameraMode = CameraMode.FOLLOW
    private _focusPosition: THREE.Vector3 = new THREE.Vector3()
    private _faceZoom = new ZoomEase(CO_DEFAULT_ZOOM)

    public get mode(): CameraMode {
        return this._mode
    }

    public set mode(val: CameraMode) {
        if (val === this._mode) return

        if (val === CameraMode.FACE && this._focusProvider?.miraType === MiraType.FIELD) return

        this._mode = val
        EventSystem.dispatch("CameraModeChangedEvent", { mode: val })

        if (val === CameraMode.FACE) {
            // Face mode drives the camera directly and ignores target coords
            this._pendingResync = undefined
            this._focusPosition.copy(this._mainCamera.position)
            this._faceZoom.reset(this._focusPosition.distanceTo(this.focusWorldPosition()))
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
            this._mode === CameraMode.LOCKED && this._focusProvider
                ? worldPos.clone().applyMatrix4(new THREE.Matrix4().copy(this._focus).invert())
                : worldPos.clone().sub(this.focusWorldPosition())

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

        if (this._focusProvider.miraType === MiraType.FIELD && this._mode === CameraMode.FACE) {
            // Don't allow Face mode for fields, default back to Follow mode
            this.mode = CameraMode.FOLLOW
        }

        if (this._mode !== CameraMode.FACE) {
            // Capture the camera's current world position.
            // The coord re-sync is deferred to update() so it runs after _focus is refreshed
            this._pendingResync = this._mainCamera.position.clone()
        } else {
            this._focusProvider.loadFocusTransform(this._focus)
            this._faceZoom.reset(this._focusPosition.distanceTo(this.focusWorldPosition()))
        }
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

        if (this._mode !== CameraMode.FOLLOW) {
            const worldPos =
                this._mode === CameraMode.FACE ? this._focusPosition.clone() : this._mainCamera.position.clone()
            this.syncCoordsFromWorldPos(worldPos)
        }
    }

    /**
     * Re-seeds these controls as a free (unfocused) Follow camera that stays at the camera's current
     * world pose. Used when handing off from another control scheme (e.g. Field View) so the view doesn't jump.
     */
    public adoptCurrentView(): void {
        this._mode = CameraMode.FOLLOW
        this._focusProvider = undefined
        this._isExplicitlyUnfocused = true

        const forward = cameraForward(this._mainCamera)
        const focusPoint = this._mainCamera.position.clone().addScaledVector(forward, CO_DEFAULT_ZOOM)
        this._focus.identity().setPosition(focusPoint)
        this.syncCoordsFromWorldPos(this._mainCamera.position)

        EventSystem.dispatch("CameraFocusChangedEvent", { focusProvider: undefined })
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

        if (this._mode === CameraMode.FACE) return

        this._focusBlend = { progress: 0, duration, startFocus: this._focus.clone() }
    }

    /** World-space position of the current focus point. */
    private focusWorldPosition(): THREE.Vector3 {
        return new THREE.Vector3().setFromMatrixPosition(this._focus)
    }

    public get focus(): THREE.Matrix4 {
        return this._focus
    }

    public set focus(matrix: THREE.Matrix4) {
        this._focus.copy(matrix)
    }

    public constructor(mainCamera: THREE.Camera, interactionHandler: ScreenInteractionHandler) {
        super("Target", mainCamera, interactionHandler)

        this._coords = DEFAULT_COORDS

        // Identity
        this._focus = new THREE.Matrix4(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)
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

    public interactionMove(move: InteractionMove) {
        // A secondary (right) drag always drops focus and pans the camera, regardless of the current mode
        if (move.movement && this._activePointerType === SECONDARY_MOUSE_INTERACTION) {
            this.panAndUnfocus(move.movement)
            return
        }

        if (this._mode === CameraMode.FACE) {
            // Face mode drives the camera directly, so only zoom is allowed
            if (move.scale) this.zoomFaceMode(move.scale)
            return
        }

        if (move.movement && this._activePointerType === PRIMARY_MOUSE_INTERACTION) {
            // Orbit: add the movement of the mouse to the target coords
            this._nextTheta -= move.movement[0]
            this._nextPhi -= move.movement[1]
        }

        if (move.scale) {
            this._orbitZoom.addInput(move.scale)
        }
    }

    /** Drops any focus target and pans the free-orbit point by the given screen movement. */
    private panAndUnfocus(movement: [number, number]): void {
        if (this._mode !== CameraMode.FOLLOW) {
            this.mode = CameraMode.FOLLOW
        }

        // Clear focus so validateFocusProvider() does not re-snap the camera on the next frame.
        if (this._focusProvider !== undefined || !this._isExplicitlyUnfocused) {
            this._focusProvider = undefined
            this._isExplicitlyUnfocused = true
            EventSystem.dispatch("CameraFocusChangedEvent", { focusProvider: undefined })
        }

        const orientation = new THREE.Quaternion().setFromEuler(this._mainCamera.rotation)
        const augmentedMovement = augmentMovement(this._mainCamera, this._coords.r, [movement[0], movement[1]])
        const pan = new THREE.Vector3(-augmentedMovement[0], augmentedMovement[1], 0)
            .applyQuaternion(orientation)
            .multiplyScalar(PreferencesSystem.getUserPreference("ScenePanSensitivity"))
        const newPos = this.focusWorldPosition()
        newPos.add(pan)
        this._focus.setPosition(newPos)
    }

    /** Accumulates raw scroll/pinch zoom input for Face mode. Eased toward every frame in updateFaceModeZoom(). */
    private zoomFaceMode(scale: number): void {
        this._faceZoom.addInput(scale)
    }

    /**
     * Eases the fixed Face mode camera's distance from the robot toward the accumulated scroll/pinch
     * target, moving its anchor along the view axis.
     */
    private updateFaceModeZoom(deltaT: number): void {
        const delta = this._faceZoom.step(deltaT)
        if (delta === 0) return

        const robotPos = this.focusWorldPosition()
        const offset = this._focusPosition.clone().sub(robotPos)
        const distance = offset.length()
        if (distance < 0.01) return

        this._focusPosition.addScaledVector(offset.divideScalar(distance), delta)
    }

    // Fixed camera position, always faces towards robot
    private focusMode() {
        const robotPos = this.focusWorldPosition()
        this._mainCamera.position.copy(this._focusPosition)
        this._mainCamera.lookAt(robotPos)
    }

    public getCurrentCoordinates(): SphericalCoords {
        return { ...this._coords }
    }

    public setImmediateCoordinates(coords: Partial<SphericalCoords>) {
        if (coords.theta !== undefined) {
            this._coords.theta = coords.theta
            this._nextTheta = coords.theta
        }
        if (coords.phi !== undefined) {
            this._coords.phi = clampPhi(coords.phi)
            this._nextPhi = this._coords.phi
        }
        if (coords.r !== undefined) {
            this._coords.r = clampZome(coords.r)
            this._orbitZoom.reset(this._coords.r)
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

        if (this._mode === CameraMode.FACE && this._focusProvider) {
            this.updateFaceModeZoom(deltaT)
            this.focusMode()
            return
        }

        if (this.enabled) {
            const rotationSensitivity = PreferencesSystem.getUserPreference("SceneRotationSensitivity")
            this._coords.theta += (this._nextTheta - this._coords.theta) * deltaT * rotationSensitivity
            this._coords.phi += (this._nextPhi - this._coords.phi) * deltaT * rotationSensitivity
            this._orbitZoom.step(deltaT)
            this._coords.r = this._orbitZoom.current
        }

        this._coords.phi = clampPhi(this._coords.phi)

        const deltaTransform = new THREE.Matrix4()
            .makeTranslation(0, 0, this._coords.r)
            .premultiply(
                new THREE.Matrix4().makeRotationFromEuler(
                    new THREE.Euler(this._coords.phi, this._coords.theta, 0, "YXZ")
                )
            )

        if (this._mode === CameraMode.LOCKED && this._focusProvider) {
            deltaTransform.premultiply(this._focus)
        } else {
            const focusPosition = new THREE.Matrix4().copyPosition(this._focus)
            deltaTransform.premultiply(focusPosition)
        }

        this._mainCamera.position.setFromMatrixPosition(deltaTransform)
        this._mainCamera.rotation.setFromRotationMatrix(deltaTransform)

        this._nextTheta = this._coords.theta
        this._nextPhi = this._coords.phi
    }
}

/** Distance below which a look direction is treated as vertical, requiring a non-default up vector. */
const FV_VERTICAL_THRESHOLD = 0.01

/**
 * Camera controls for the Field View control type. The camera is anchored to a pre-authored
 * {@link CameraPoint} on the field and faces the point's configured target, or field center.
 * Allows panning and zooming within the view, and optionally focusing on a specific robot.
 */
export class CustomFieldViewControls extends CameraControls {
    private _field: MirabufSceneObject | undefined
    private _pointIndex = -1
    private _focusRobot: MirabufSceneObject | undefined

    /** Accumulated zoom (dolly) offset from the station anchor, in world space. */
    private _viewOffset = new THREE.Vector3()

    /** Eases the dolly distance toward the look target; unused for fixed-rotation points (no target). */
    private _zoom = new ZoomEase(CO_DEFAULT_ZOOM)

    private get _point(): CameraPoint | undefined {
        return this._field?.fieldPreferences?.cameraPoints?.[this._pointIndex]
    }

    public get selectedPoint(): CameraPoint | undefined {
        return this._point
    }

    public get focusedRobot(): MirabufSceneObject | undefined {
        return this._focusRobot
    }

    public constructor(mainCamera: THREE.Camera, interactionHandler: ScreenInteractionHandler) {
        super("FieldView", mainCamera, interactionHandler)
    }

    /** Anchor the camera to the field's camera point at the given index. */
    public selectPoint(field: MirabufSceneObject, index: number): void {
        this._field = field
        this._pointIndex = index
        this._viewOffset.set(0, 0, 0)
        this.resetDollyZoom()

        EventSystem.dispatch("CameraViewChangedEvent", { point: this._point, focusedRobotId: this._focusRobot?.id })
    }

    /** Face a specific robot, or pass undefined to return to the point's default aim. */
    public focusRobot(robot: MirabufSceneObject | undefined): void {
        this._focusRobot = robot
        this.resetDollyZoom()
        EventSystem.dispatch("CameraViewChangedEvent", { point: this._point, focusedRobotId: robot?.id })
    }

    /**
     * Resyncs the dolly zoom target to the current look-target distance, discarding any input in
     * flight. Needed whenever the look target changes for reasons other than a scroll (new point,
     * newly focused robot) so the eased zoom doesn't mistake the jump in distance for a dolly.
     */
    private resetDollyZoom(): void {
        const target = this.resolveLookTarget()
        if (!this._field || !this._point || !target) return
        const anchor = this.anchorPosition(this._field, this._point)
        this._zoom.reset(anchor.add(this._viewOffset).distanceTo(target))
    }

    public interactionMove(move: InteractionMove): void {
        // Panning hands control back to the free Follow camera rather than panning within this view.
        if (move.movement && this._activePointerType === SECONDARY_MOUSE_INTERACTION) {
            this.handoffToFollowControls(move)
            return
        }
        if (move.scale) this.dolly(move.scale)
    }

    /**
     * Hands control to the standard Follow controls with no focus, seeding them with the current
     * view so it doesn't jump, then forwards the in-progress drag so the pan continues seamlessly.
     */
    private handoffToFollowControls(move: InteractionMove): void {
        World.sceneRenderer.setCameraControls("Target")
        const controls = World.sceneRenderer.currentCameraControls as CustomTargetControls

        controls.adoptCurrentView()
        // Register the held button on the new controls so this drag (and the rest) pans.
        controls.interactionStart({ interactionType: SECONDARY_MOUSE_INTERACTION, position: [0, 0] })
        controls.interactionMove(move)
    }

    /** World position of the point's authored anchor, before the accumulated zoom offset. */
    private anchorPosition(field: MirabufSceneObject, point: CameraPoint): THREE.Vector3 {
        const fieldRef = field.getXZPositionTransform()
        return fieldRef.add(new THREE.Vector3(...point.pos))
    }

    /** Dollies the camera along its view axis (scroll to zoom toward/away from the target). */
    private dolly(scale: number): void {
        const field = this._field
        const point = this._point
        if (!field || !point) return

        if (!this.resolveLookTarget()) {
            const forward = cameraForward(this._mainCamera)
            this._viewOffset
                .addScaledVector(forward, -scale * CO_FIXED_DOLLY_SENSITIVITY * CO_DEFAULT_ZOOM)
                .clampLength(CO_MIN_ZOOM, CO_MAX_ZOOM)
            return
        }

        this._zoom.addInput(scale)
    }

    /**
     * Eases the dolly distance from the look target toward the accumulated scroll/pinch target,
     * decelerating as it approaches CO_MIN_ZOOM to match the orbit camera's zoom feel.å
     */
    private updateDollyZoom(deltaT: number): void {
        const delta = this._zoom.step(deltaT)
        if (delta === 0) return

        const field = this._field
        const point = this._point
        const target = this.resolveLookTarget()
        if (!field || !point || !target) return

        const anchor = this.anchorPosition(field, point)
        const toCamera = anchor.clone().add(this._viewOffset).sub(target)
        const distance = toCamera.length()
        if (distance < 0.01) return

        this._viewOffset.addScaledVector(toCamera.divideScalar(distance), delta)
    }

    /** Resolves the world point the camera should face, or undefined for a fixed-rotation point. */
    private resolveLookTarget(): THREE.Vector3 | undefined {
        if (!this._field || !this._point) return undefined
        if (this._focusRobot) return this._focusRobot.getXZPositionTransform()

        const look = this._point.look
        switch (look.type) {
            case "field":
                return this._field.getXZPositionTransform()
            case "rotation":
                return undefined
        }
    }

    public update(deltaT: number): void {
        if (!this._enabled || !this._point || !this._field) return

        // Recover gracefully if the field was unloaded while this view was active.
        if (!World.sceneRenderer.mirabufSceneObjects.getAll().includes(this._field)) return

        this.updateDollyZoom(deltaT)

        const fieldRef = this._field.getXZPositionTransform()
        this._mainCamera.position.set(
            fieldRef.x + this._point.pos[0] + this._viewOffset.x,
            fieldRef.y + this._point.pos[1] + this._viewOffset.y,
            fieldRef.z + this._point.pos[2] + this._viewOffset.z
        )

        // A fixed-rotation point with no robot focus uses its authored orientation directly.
        if (!this._focusRobot && this._point.look.type === "rotation") {
            this._mainCamera.up.set(0, 1, 0)
            this._mainCamera.rotation.set(this._point.look.pitch, this._point.look.yaw, 0, "YXZ")
            return
        }

        const target = this.resolveLookTarget()
        if (!target) return

        // A near-vertical view needs a horizontal up vector to avoid gimbal flip.
        const horizontal = Math.hypot(target.x - this._mainCamera.position.x, target.z - this._mainCamera.position.z)
        if (horizontal < FV_VERTICAL_THRESHOLD) {
            this._mainCamera.up.set(0, 0, -1)
        } else {
            this._mainCamera.up.set(0, 1, 0)
        }
        this._mainCamera.lookAt(target)
    }
}

/** Returns the active camera controls only when they are {@link CustomTargetControls}, else undefined. */
export function getTargetControls(): CustomTargetControls | undefined {
    const controls = World.sceneRenderer?.currentCameraControls
    return controls instanceof CustomTargetControls ? controls : undefined
}
