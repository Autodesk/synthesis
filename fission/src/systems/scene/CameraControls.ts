import * as THREE from 'three'
import { OrbitControls as ThreeOrbitControls } from "three/examples/jsm/controls/OrbitControls.js"
import { FlyControls as ThreeFlyControls } from "three/examples/jsm/controls/FlyControls.js"

export enum CameraControlsType {
    Orbit = 1, Fly = 2
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

    public abstract dispose(): void
}

export class OrbitControls extends CameraControls {
    private _orbit: ThreeOrbitControls

    public get settings() { return this._orbit }
    
    public set enabled(val: boolean) { this._orbit.enabled = val }
    public get enabled(): boolean { return this._orbit.enabled }
    
    public constructor(mainCamera: THREE.Camera, domElement: HTMLElement) {
        super(CameraControlsType.Orbit)

        this._orbit = new ThreeOrbitControls(mainCamera, domElement)
    }

    public update(_: number): void {
        this._orbit.update()
    }

    public dispose(): void {
        this._orbit.dispose()
    }
}

export class FlyControls extends CameraControls {
    private _fly: ThreeFlyControls

    public get settings() { return this._fly }
    
    public set enabled(val: boolean) { this._fly.enabled = val }
    public get enabled(): boolean { return this._fly.enabled }
    
    public constructor(mainCamera: THREE.Camera, domElement: HTMLElement) {
        super(CameraControlsType.Orbit)

        this._fly = new ThreeFlyControls(mainCamera, domElement)
    }

    public update(deltaT: number): void {
        this._fly.update(deltaT)
    }

    public dispose(): void {
        this._fly.dispose()
    }
}