import RAPIER from "@dimforge/rapier3d-compat";

const GRAVITY: RAPIER.Vector3 = new RAPIER.Vector3(0.0, -9.81, 0.0);

/**
 * Mostly stolen from here: https://rapier.rs/docs/user_guides/javascript/getting_started_js
 */
export class PhysicsManager {

    private _world: RAPIER.World;
    private _groundCollider: RAPIER.ColliderDesc;

    private _bodies: Array<RAPIER.RigidBody>;
    private _joints: Array<RAPIER.MultibodyJoint>;

    private constructor() {
        this._world = new RAPIER.World(GRAVITY);

        this._groundCollider = RAPIER.ColliderDesc.cuboid(10.0, 0.25, 10.0).setTranslation(0.0, -2.0, 0.0);
        this._world.createCollider(this._groundCollider);

        this._bodies = new Array<RAPIER.RigidBody>();
        this._joints = new Array<RAPIER.MultibodyJoint>();
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

        this._bodies.push(rb);

        return rb;
    }

    public makeBox(position: RAPIER.Vector3, halfExtents: RAPIER.Vector3): RAPIER.RigidBody {
        let rb = this._world.createRigidBody(
            RAPIER.RigidBodyDesc.dynamic().setTranslation(position.x, position.y, position.z)
        );
        // this._world.createCollider(
        //     RAPIER.ColliderDesc.ball(radius),
        //     rb
        // );
        this._world.createCollider(
            RAPIER.ColliderDesc.cuboid(halfExtents.x, halfExtents.y, halfExtents.z),
            rb
        );

        this._bodies.push(rb);

        return rb;
    }

    public createJoint(data: RAPIER.JointData, body1: RAPIER.RigidBody, body2: RAPIER.RigidBody): RAPIER.MultibodyJoint {
        console.log("Creating multi joint");
        var joint = this._world.createMultibodyJoint(data, body1, body2, true);
        this._joints.push(joint);
        return joint;
    }

    public step() {
        this._world.step();
    }

    public getBody(index: number): RAPIER.RigidBody | undefined {
        if (index >= this._bodies.length || index < 0) {
            console.warn(`No bodies with index ${index}`);
            return undefined;
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

            // PhysicsManager._instance._world.free();

            PhysicsManager._instance = null;
            return true;
        }
        return false;
    }
}