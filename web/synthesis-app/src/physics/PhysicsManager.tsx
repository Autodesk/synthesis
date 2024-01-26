import RAPIER from "@dimforge/rapier3d-compat";

const GRAVITY: RAPIER.Vector3 = new RAPIER.Vector3(0.0, -9.81, 0.0);

/**
 * Mostly stolen from here: https://rapier.rs/docs/user_guides/javascript/getting_started_js
 */
export class PhysicsManager {

    private _world: RAPIER.World;

    private _bodies: Array<number>;

    public ballCount: number;

    private constructor() {
        this._world = new RAPIER.World(GRAVITY);
        this._world.integrationParameters.dt = 1.0 / 60.0;

        var [X, Y] = [10.0, 10.0];

        this._world.createCollider(RAPIER.ColliderDesc.cuboid(X / 2.0, 0.25, Y / 2.0).setTranslation(0.0, -0.25, 0.0));
        this._world.createCollider(RAPIER.ColliderDesc.cuboid(0.25, 10.0, Y / 2.0).setTranslation((-X / 2.0) + 0.25, 10.0, 0.0));
        this._world.createCollider(RAPIER.ColliderDesc.cuboid(0.25, 10.0, Y / 2.0).setTranslation((X / 2.0) - 0.25, 10.0, 0.0));
        this._world.createCollider(RAPIER.ColliderDesc.cuboid(X / 2.0, 10.0, 0.25).setTranslation(0.0, 10.0, (-Y / 2.0) + 0.25));
        this._world.createCollider(RAPIER.ColliderDesc.cuboid(X / 2.0, 10.0, 0.25).setTranslation(0.0, 10.0, (Y / 2.0) - 0.25));

        this._bodies = new Array<number>();
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
            RAPIER.ColliderDesc.ball(radius),
            rb
        );

        this._bodies.push(rb.handle);

        this.ballCount++;

        return rb;
    }

    public makeBox(position: RAPIER.Vector3, halfExtents: RAPIER.Vector3): RAPIER.RigidBody {
        let rb = this._world.createRigidBody(
            RAPIER.RigidBodyDesc.dynamic().setTranslation(position.x, position.y, position.z)
        );
        this._world.createCollider(
            RAPIER.ColliderDesc.cuboid(halfExtents.x, halfExtents.y, halfExtents.z),
            rb
        );

        this._bodies.push(rb.handle);

        return rb;
    }

    public step(deltaT: number) {
        // this._world.integrationParameters.dt = Math.min(1.0 / 60.0, Math.max(1.0 / 140.0, deltaT));
        this._world.step();
    }

    public getBody(handle: number): RAPIER.RigidBody | null {
        return this._world.bodies.get(handle);
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

            // PhysicsManager._instance._world.free();

            PhysicsManager._instance = null;
            return true;
        }
        return false;
    }
}