import * as THREE from 'three'
import { OrbitControls as ThreeOrbitControls } from "three/examples/jsm/controls/OrbitControls.js"
import { FlyControls as ThreeFlyControls } from "three/examples/jsm/controls/FlyControls.js"

export enum CameraControlsType {
    OrbitFocus = 1, OrbitFree = 2
}

export abstract class CameraControls {
    private _controlsType;

    public abstract set enabled(val: boolean)
    public abstract get enabled(): boolean

    public get controlsType() { return this._controlsType }

    public constructor(controlsType: CameraControlsType) {
        this._controlsType = controlsType
    }

    public abstract update(deltaT: number): void

    public abstract setFocus(vec: THREE.Vector3): void;

    public abstract dispose(): void
}

export class OrbitControls extends CameraControls {
    private _orbit: ThreeOrbitControls

    public get settings() { return this._orbit }
    
    public set enabled(val: boolean) { this._orbit.enabled = val }
    public get enabled(): boolean { return this._orbit.enabled }

    private _allowFocus: boolean
    
    public constructor(mainCamera: THREE.Camera, domElement: HTMLElement, allowFocus: boolean) {
        super(allowFocus ? CameraControlsType.OrbitFocus : CameraControlsType.OrbitFree)

        this._allowFocus = allowFocus

        this._orbit = new ThreeOrbitControls(mainCamera, domElement)
        this._orbit.enablePan = !allowFocus
    }

    public update(deltaT: number): void {
        this._orbit.update(deltaT)
    }

    public setFocus(vec: THREE.Vector3): void {
        if (!this._allowFocus) {
            return
        }

        this._orbit.target = vec
    }

    public dispose(): void {
        this._orbit.dispose()
    }
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

const CO_MAX_ZOOM = 40.0
const CO_MIN_ZOOM = 0.1
const CO_MAX_PHI = Math.PI / 2.1
const CO_MIN_PHI = -Math.PI / 2.1

const CO_SENSITIVITY_ZOOM = 3.0
const CO_SENSITIVITY_PHI = 1.0
const CO_SENSITIVITY_THETA = 1.0

const CO_DEFAULT_ZOOM = 3.5
const CO_DEFAULT_PHI = -Math.PI / 6.0
const CO_DEFAULT_THETA = -Math.PI / 4.0

export class CustomOrbitControls extends CameraControls {
    private _enabled = true
    
    private _mainCamera: THREE.Camera
    private _domElement: HTMLElement

    private _pointerHandler: PointerHandler

    private _isPointerDown: boolean
    private _currentPos: SphericalCoords
    private _lastPos: SphericalCoords
    private _coords: SphericalCoords
    private _focus: THREE.Vector3

    public set enabled(val: boolean) {
        this._enabled = val
    }
    public get enabled(): boolean {
        return this._enabled
    }

    public constructor(mainCamera: THREE.Camera, domElement: HTMLElement) {
        super(CameraControlsType.OrbitFocus)

        this._mainCamera = mainCamera
        this._domElement = domElement

        this._currentPos = { theta: CO_DEFAULT_THETA, phi: CO_DEFAULT_PHI, r: CO_DEFAULT_ZOOM }
        this._lastPos = this._currentPos
        this._coords = { theta: CO_DEFAULT_THETA, phi: CO_DEFAULT_PHI, r: CO_DEFAULT_ZOOM }
        this._isPointerDown = false

        this._focus = new THREE.Vector3(0,0,0)

        this._pointerHandler = new PointerHandler(domElement,
            (ev) => this.pointerDown(ev),
            (ev) => this.pointerUp(ev),
            (ev) => this.pointerMove(ev),
            (ev) => this.wheelMove(ev)
        )
    }

    public wheelMove(ev: WheelEvent) {
        this._currentPos.r += ev.deltaY * 0.01
    }

    public pointerUp(ev: PointerEvent) {
        this._isPointerDown = false
    }

    public pointerDown(ev: PointerEvent) {
        this._isPointerDown = ev.button == 0
    }

    public pointerMove(ev: PointerEvent) {
        if (!this._isPointerDown) {
            return
        }

        this._currentPos.theta -= ev.movementX
        this._currentPos.phi -= ev.movementY

        console.debug(this._currentPos)
    }

    public update(deltaT: number): void {
        const omega: SphericalCoords = this.enabled ? {
            theta: this._currentPos.theta - this._lastPos.theta,
            phi: this._currentPos.phi - this._lastPos.phi,
            r: this._currentPos.r - this._lastPos.r
        } : { theta: 0, phi: 0, r: 0 }

        this._lastPos = { theta: this._currentPos.theta, phi: this._currentPos.phi, r: this._currentPos.r }

        this._coords.theta += omega.theta * deltaT * CO_SENSITIVITY_THETA
        this._coords.phi += omega.phi * deltaT * CO_SENSITIVITY_PHI
        this._coords.r += omega.r * deltaT * CO_SENSITIVITY_ZOOM * Math.pow(this._coords.r, 1.6)

        this._coords.phi = Math.min(CO_MAX_PHI, Math.max(CO_MIN_PHI, this._coords.phi))
        this._coords.r = Math.min(CO_MAX_ZOOM, Math.max(CO_MIN_ZOOM, this._coords.r))

        const deltaTransform = (new THREE.Matrix4()).makeTranslation(0, 0, this._coords.r)
            .premultiply((new THREE.Matrix4()).makeRotationFromEuler(new THREE.Euler(this._coords.phi, this._coords.theta, 0, "YXZ")))

        deltaTransform.premultiply((new THREE.Matrix4()).makeTranslation(this._focus))

        this._mainCamera.position.setFromMatrixPosition(deltaTransform)
        this._mainCamera.rotation.setFromRotationMatrix(deltaTransform)
    }

    public setFocus(vec: THREE.Vector3): void {
        if (this.enabled) {
            this._focus.copy(vec)
        }
    }

    public dispose(): void {
        this._pointerHandler.dispose()
    }

}