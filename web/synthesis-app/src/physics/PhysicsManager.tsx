// import { ColliderDesc, RigidBody, RigidBodyDesc, Vector3, World } from "@dimforge/rapier3d";

import RAPIER from "@dimforge/rapier3d-compat";

const GRAVITY: RAPIER.Vector3 = new RAPIER.Vector3(0.0, -9.81, 0.0);

/**
 * Mostly stolen from here: https://rapier.rs/docs/user_guides/javascript/getting_started_js
 */
export class PhysicsManager {

    private _world: RAPIER.World;
    private _groundCollider: RAPIER.ColliderDesc;

    private _bodies: Array<RAPIER.RigidBody>;

    private constructor() {
        this._world = new RAPIER.World(GRAVITY);

        this._groundCollider = RAPIER.ColliderDesc.cuboid(10.0, 0.25, 10.0).setTranslation(0.0, -2.0, 0.0);
        this._world.createCollider(this._groundCollider);

        this._bodies = new Array<RAPIER.RigidBody>();
    }

    public makeBall(position: RAPIER.Vector3, radius: number): RAPIER.RigidBody {
        let rb = this._world.createRigidBody(
            RAPIER.RigidBodyDesc.dynamic().setTranslation(position.x, position.y, position.z)
        );
        // this._world.createCollider(
        //     RAPIER.ColliderDesc.ball(radius),
        //     rb
        // );
        this._world.createCollider(
            RAPIER.ColliderDesc.cuboid(0.5, 0.5, 0.5),
            rb
        );

        this._bodies.push(rb);

        return rb;
    }

    public step() {
        this._world.step();
    }

    public getBody(index: number): RAPIER.RigidBody {
        if (index >= this._bodies.length || index < 0) {
            throw new Error("Index out of bounds");
        }

        return this._bodies[index];
    }

    // Singleton Lifetime controls

    private static _instance: PhysicsManager | null;
    public static getInstance(): PhysicsManager {
        if (!PhysicsManager._instance) {
            PhysicsManager._instance = new PhysicsManager();
        }

        return PhysicsManager._instance;
    }

    public static killInstance(): boolean {
        if (PhysicsManager._instance) {
            PhysicsManager._instance = null;
            return true;
        }
        return false;
    }
}