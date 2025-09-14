import * as THREE from "three"
import { MiraType } from "@/mirabuf/MirabufLoader"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import World from "../World"
import type ScreenInteractionHandler from "./ScreenInteractionHandler"
import {
    type InteractionEnd,
    type InteractionMove,
    type InteractionStart,
    PRIMARY_MOUSE_INTERACTION,
    SECONDARY_MOUSE_INTERACTION,
} from "./ScreenInteractionHandler"

export type CameraControlsType = "Orbit"

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
    // const aspect = 1.0
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

export class CustomOrbitControls extends CameraControls {
    private _enabled = true

    private _mainCamera: THREE.Camera

    private _activePointerType: PointerType
    private _nextCoords: SphericalCoords
    private _coords: SphericalCoords
    private _focus: THREE.Matrix4

    private _focusProvider: MirabufSceneObject | undefined
    private _isExplicitlyUnfocused: boolean = false
    public locked: boolean

    private _interactionHandler: ScreenInteractionHandler

    public set enabled(val: boolean) {
        this._enabled = val
    }
    public get enabled(): boolean {
        return this._enabled
    }

    public set focusProvider(provider: MirabufSceneObject | undefined) {
        this._focusProvider = provider
        if (provider !== undefined) {
            this._isExplicitlyUnfocused = false
        }
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
    }

    public get coords(): SphericalCoords {
        return this._coords
    }

    public get focus(): THREE.Matrix4 {
        return this._focus
    }

    public set focus(matrix: THREE.Matrix4) {
        this._focus.copy(matrix)
    }

    public constructor(mainCamera: THREE.Camera, interactionHandler: ScreenInteractionHandler) {
        super("Orbit")

        this._mainCamera = mainCamera
        this._interactionHandler = interactionHandler

        this.locked = false

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
        if (!World.sceneRenderer?.sceneObjects || World.dragModeSystem.isTransitioning) {
            return
        }
        const mirabufObjects = World.sceneRenderer.mirabufSceneObjects.getAll()

        if (this._focusProvider) {
            if (!mirabufObjects.includes(this._focusProvider)) {
                this._focusProvider = this.findFallbackFocus(mirabufObjects)
                this._isExplicitlyUnfocused = false
            }
        } else if (!this._isExplicitlyUnfocused) {
            this._focusProvider = this.findFallbackFocus(mirabufObjects)
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
        if (move.movement) {
            if (this._activePointerType == PRIMARY_MOUSE_INTERACTION) {
                // Add the movement of the mouse to the _currentPos
                this._nextCoords.theta -= move.movement[0]
                this._nextCoords.phi -= move.movement[1]
            } else if (this._activePointerType == SECONDARY_MOUSE_INTERACTION && !this.locked) {
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

    public update(deltaT: number): void {
        deltaT = Math.max(1.0 / 60.0, Math.min(1 / 144.0, deltaT))

        this.validateFocusProvider()

        if (this.enabled) this._focusProvider?.loadFocusTransform(this._focus)

        // Generate delta of spherical coordinates
        const omega: SphericalCoords = this.enabled
            ? {
                  theta: this._nextCoords.theta - this._coords.theta,
                  phi: this._nextCoords.phi - this._coords.phi,
                  r: this._nextCoords.r - this._coords.r,
              }
            : { theta: 0, phi: 0, r: 0 }

        this._coords.theta += omega.theta * deltaT * PreferencesSystem.getGlobalPreference("SceneRotationSensitivity")
        this._coords.phi += omega.phi * deltaT * PreferencesSystem.getGlobalPreference("SceneRotationSensitivity")
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

        if (this.locked && this._focusProvider) {
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
