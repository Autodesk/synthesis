import { Type } from "typescript";
import Queue from "../../util/data/Queue";
import { PhysicsSystem, RenderSystem } from "../Systems";

class WorldSystem {

    private _lastFrameTime: number;
    private _updateFrame?: number;

    private _physics: PhysicsSystem;
    private _renderer: RenderSystem;

    constructor() {
        this._physics = new PhysicsSystem();
        this._renderer = new RenderSystem();

        // Create Robot
    }

    public startLoop() {
        this.stopLoop();

        
    }

    private update() {
        var _ = Date.now() - this._lastFrameTime;
        this._lastFrameTime = Date.now();

        this._updateFrame = requestAnimationFrame(this.update);


    }

    public stopLoop() {

    }

}
export var worldSystem = new WorldSystem();

export abstract class System {

    private _priority: number;
    
    protected get priority() { return this._priority; }

    protected constructor(priority: number) {

    }
}
