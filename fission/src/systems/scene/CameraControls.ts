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

    private _domElement: HTMLElement

    private _isPointerDown: boolean = false
    public get isPointerDown() { return this._isPointerDown }

    public constructor(domElement: HTMLElement, pointerMove: (ev: PointerEvent) => void, wheelMove: (ev: WheelEvent) => void) {
        this._pointerMove = pointerMove
        this._wheelMove = wheelMove
        this._domElement = domElement

        this._pointerDown = (ev) => { this._isPointerDown = true }
        this._pointerUp = (ev) => { this._isPointerDown = false }
        
        this._domElement.addEventListener("pointermove", this._pointerMove)
        this._domElement.addEventListener("wheel", this._wheelMove)
        
        this._domElement.addEventListener("pointerdown", this._pointerDown)
        this._domElement.addEventListener("pointerup", this._pointerUp)
        this._domElement.addEventListener("pointercancel", this._pointerUp)
        this._domElement.addEventListener("pointerleave", this._pointerUp)
    }

    public dispose() {
        this._domElement.removeEventListener("pointermove", this._pointerMove)
        this._domElement.removeEventListener("wheel", this._wheelMove)

        this._domElement.removeEventListener("pointerdown", this._pointerDown)
        this._domElement.removeEventListener("pointerup", this._pointerUp)
        this._domElement.removeEventListener("pointercancel", this._pointerUp)
        this._domElement.removeEventListener("pointerleave", this._pointerUp)
    }
}

export class CustomOrbitControls extends CameraControls {
    private _enabled = true
    
    private _mainCamera: THREE.Camera
    private _domElement: HTMLElement

    private _pointerHandler: PointerHandler

    private _omega: THREE.Vec2

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

        this._omega = { x: 0, y: 0 }

        this._pointerHandler = new PointerHandler(domElement,
            (ev) => {
                this._omega = { x: ev.movementX, y: ev.movementY }
            },
            (ev) => {

            }
        )
    }

    public update(deltaT: number): void {
        if (this._pointerHandler.isPointerDown)
        console.debug(this._omega)
    }

    public setFocus(vec: THREE.Vector3): void {
    
    }

    public dispose(): void {
        
    }

}