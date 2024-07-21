import MirabufSceneObject, { RigidNodeAssociate } from '@/mirabuf/MirabufSceneObject'
import * as THREE from 'three'
import World from '../World'
import { ThreeVector3_JoltVec3 } from '@/util/TypeConversions'
import { MiraType } from '@/mirabuf/MirabufLoader'
import { MainHUD_AddToast } from '@/ui/components/MainHUD'

export type CameraControlsType =
    "Orbit"

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

class PointerHandler {

    private _pointerMove: (ev: PointerEvent) => void
    private _wheelMove: (ev: WheelEvent) => void
    private _pointerDown: (ev: PointerEvent) => void
    private _pointerUp: (ev: PointerEvent) => void
    private _contextMenu: (ev: MouseEvent) => unknown

    private _domElement: HTMLElement

    public constructor(
        domElement: HTMLElement,
        pointerDown: (ev: PointerEvent) => void,
        pointerUp: (ev: PointerEvent) => void,
        pointerMove: (ev: PointerEvent) => void,
        wheelMove: (ev: WheelEvent) => void,
        onContextMenu?: (ev: MouseEvent) => unknown
    ) {
        this._pointerMove = pointerMove
        this._wheelMove = wheelMove
        this._domElement = domElement

        this._pointerDown = pointerDown
        this._pointerUp = pointerUp

        this._contextMenu = onContextMenu ?? ((ev: MouseEvent) => {
            ev.preventDefault()
        }) 
        
        this._domElement.addEventListener("pointermove", this._pointerMove)
        this._domElement.addEventListener("wheel", this._wheelMove, { passive: false })
        this._domElement.addEventListener("contextmenu", this._contextMenu)
        
        this._domElement.addEventListener("pointerdown", this._pointerDown)
        this._domElement.addEventListener("pointerup", this._pointerUp)
        this._domElement.addEventListener("pointercancel", this._pointerUp)
        this._domElement.addEventListener("pointerleave", this._pointerUp)
    }

    public dispose() {
        this._domElement.removeEventListener("pointermove", this._pointerMove)
        this._domElement.removeEventListener("wheel", this._wheelMove)
        this._domElement.removeEventListener("contextmenu", this._contextMenu)

        this._domElement.removeEventListener("pointerdown", this._pointerDown)
        this._domElement.removeEventListener("pointerup", this._pointerUp)
        this._domElement.removeEventListener("pointercancel", this._pointerUp)
        this._domElement.removeEventListener("pointerleave", this._pointerUp)
    }
}

interface SphericalCoords {
    theta: number
    phi: number
    r: number
}

type PointerType = -1 | 0 | 1 | 2

const PRIMARY_POINTER_TYPE = 0
const MIDDLE_POINTER_TYPE = 1
const SECONDARY_POINTER_TYPE = 2

const CO_MAX_ZOOM = 40.0
const CO_MIN_ZOOM = 0.1
const CO_MAX_PHI = Math.PI / 2.1
const CO_MIN_PHI = -Math.PI / 2.1

const CO_SENSITIVITY_ZOOM = 5.0
const CO_SENSITIVITY_PHI = 1.0
const CO_SENSITIVITY_THETA = 1.0

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
function augmentMovement(camera: THREE.Camera, distanceFromFocus: number, originalMovement: [number, number]): [number, number] {
    const aspect = window.innerWidth / window.innerHeight
    const fov: number | undefined = (camera as THREE.PerspectiveCamera)?.fov
    if (fov) {
        return [
            (2 * distanceFromFocus * Math.tan(DEG2RAD * fov * aspect / 2) * originalMovement[0]) / window.innerWidth,
            (2 * distanceFromFocus * Math.tan(DEG2RAD * fov / 2) * originalMovement[1]) / window.innerHeight
        ]
    } else {
        return originalMovement
    }
}

export class CustomOrbitControls extends CameraControls {
    private _enabled = true
    
    private _mainCamera: THREE.Camera

    private _pointerHandler: PointerHandler

    private _activePointerType: PointerType
    private _nextCoords: SphericalCoords
    private _coords: SphericalCoords
    private _focus: THREE.Matrix4

    private _focusProvider: MirabufSceneObject | undefined
    public locked: boolean

    public set enabled(val: boolean) {
        this._enabled = val
    }
    public get enabled(): boolean {
        return this._enabled
    }

    public constructor(mainCamera: THREE.Camera, domElement: HTMLElement) {
        super("Orbit")

        this._mainCamera = mainCamera

        this.locked = false

        this._nextCoords = { theta: CO_DEFAULT_THETA, phi: CO_DEFAULT_PHI, r: CO_DEFAULT_ZOOM }
        this._coords = { theta: CO_DEFAULT_THETA, phi: CO_DEFAULT_PHI, r: CO_DEFAULT_ZOOM }
        this._activePointerType = -1

        // Identity
        this._focus = new THREE.Matrix4(1,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1)

        this._pointerHandler = new PointerHandler(domElement,
            (ev) => this.pointerDown(ev),
            (ev) => this.pointerUp(ev),
            (ev) => this.pointerMove(ev),
            (ev) => this.wheelMove(ev)
        )
    }

    public wheelMove(ev: WheelEvent) {
        // Something to just scale the scrolling delta to something more reasonable.
        this._nextCoords.r += ev.deltaY * 0.01
    }

    public pointerUp(ev: PointerEvent) {
        /**
         * If Pointer is already down, and the button that is being
         * released is the primary button, make Pointer not be down
         */
        if (ev.button == this._activePointerType) {
            this._activePointerType = -1
        }
    }

    public pointerDown(ev: PointerEvent) {
        // If primary button, make Pointer be down
        if (this._activePointerType < 0) {
            switch (ev.button) {
                case PRIMARY_POINTER_TYPE:
                    this._activePointerType = PRIMARY_POINTER_TYPE
                    break
                case MIDDLE_POINTER_TYPE:
                    this._activePointerType = MIDDLE_POINTER_TYPE
                    break
                case SECONDARY_POINTER_TYPE:
                    this.tryFindFocusProvider(ev)
                    break
                default:
                    break
            }
        }
    }

    public pointerMove(ev: PointerEvent) {
        if (this._activePointerType == PRIMARY_POINTER_TYPE) {
            // Add the movement of the mouse to the _currentPos
            this._nextCoords.theta -= ev.movementX
            this._nextCoords.phi -= ev.movementY
        } else if (this._activePointerType == MIDDLE_POINTER_TYPE && !this.locked) {
            this._focusProvider = undefined

            const orientation = (new THREE.Quaternion()).setFromEuler(this._mainCamera.rotation)

            const augmentedMovement = augmentMovement(
                this._mainCamera,
                this._coords.r,
                [ev.movementX, ev.movementY]
            )

            const pan = (new THREE.Vector3(-augmentedMovement[0], augmentedMovement[1], 0)).applyQuaternion(orientation)
            const newPos = (new THREE.Vector3()).setFromMatrixPosition(this._focus)
            newPos.add(pan)
            this._focus.setPosition(newPos)
        }
    }

    public update(deltaT: number): void {

        this._focusProvider?.LoadFocusTransform(this._focus)

        // Generate delta of spherical coordinates
        const omega: SphericalCoords = this.enabled ? {
            theta: this._nextCoords.theta - this._coords.theta,
            phi: this._nextCoords.phi - this._coords.phi,
            r: this._nextCoords.r - this._coords.r
        } : { theta: 0, phi: 0, r: 0 }

        this._coords.theta += omega.theta * deltaT * CO_SENSITIVITY_THETA
        this._coords.phi += omega.phi * deltaT * CO_SENSITIVITY_PHI
        this._coords.r += omega.r * deltaT * CO_SENSITIVITY_ZOOM * Math.pow(this._coords.r, 1.4)

        this._coords.phi = Math.min(CO_MAX_PHI, Math.max(CO_MIN_PHI, this._coords.phi))
        this._coords.r = Math.min(CO_MAX_ZOOM, Math.max(CO_MIN_ZOOM, this._coords.r))

        const deltaTransform = (new THREE.Matrix4()).makeTranslation(0, 0, this._coords.r)
            .premultiply((new THREE.Matrix4()).makeRotationFromEuler(new THREE.Euler(this._coords.phi, this._coords.theta, 0, "YXZ")))

        if (this.locked && this._focusProvider) {
            deltaTransform.premultiply(this._focus)
        } else {
            const focusPosition = (new THREE.Matrix4()).copyPosition(this._focus)
            deltaTransform.premultiply(focusPosition)
        }
        

        this._mainCamera.position.setFromMatrixPosition(deltaTransform)
        this._mainCamera.rotation.setFromRotationMatrix(deltaTransform)

        this._nextCoords = { theta: this._coords.theta, phi: this._coords.phi, r: this._coords.r }
    }

    private tryFindFocusProvider(ev: PointerEvent) {
        const dir = World.SceneRenderer.PixelToWorldSpace(ev.clientX, ev.clientY, 1.0).normalize().multiplyScalar(40.0)
        const res = World.PhysicsSystem.RayCast(ThreeVector3_JoltVec3(this._mainCamera.position), ThreeVector3_JoltVec3(dir))
        if (res) {
            const assoc = World.PhysicsSystem.GetBodyAssociation(res.data.mBodyID) as RigidNodeAssociate
            // Cast and Exist check
            if (assoc?.sceneObject) {
                if (assoc.sceneObject.miraType == MiraType.ROBOT && assoc.sceneObject != this._focusProvider) {
                    this._focusProvider = assoc.sceneObject
                    MainHUD_AddToast("info", "Focus Changed", `Focusing on ${assoc.sceneObject.assemblyName}`)
                }
            }
        }
    }

    public dispose(): void {
        this._pointerHandler.dispose()
    }

}