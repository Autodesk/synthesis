/* eslint-disable  @typescript-eslint/no-explicit-any */

import { ThreeVector3_JoltVec3, _JoltQuat } from "../../util/conversions/JoltThreeConversions";
import JOLT from "../../util/loading/JoltSyncLoader";
import * as THREE from 'three';

const LAYER_NOT_MOVING = 0;
const LAYER_MOVING = 1;
const COUNT_OBJECT_LAYERS = 2;

export class PhysicsSystem {
    
    private _joltInterface;
    private _joltPhysSystem;
    private _joltBodyInterface;
    private _bodies: Array<any>;

    constructor() {
        this._bodies = [];

        const joltSettings = new JOLT.JoltSettings();
        setupCollisionFiltering(joltSettings);

        this._joltInterface = new JOLT.JoltInterface(joltSettings);
        JOLT.destroy(joltSettings);

        this._joltPhysSystem = this._joltInterface.GetPhysicsSystem();
        this._joltBodyInterface = this._joltPhysSystem.GetBodyInterface();
    }

    public createBox(
    halfExtents: THREE.Vector3,
    mass: number | undefined,
    position: THREE.Vector3 | undefined,
    rotation: THREE.Euler | THREE.Quaternion | undefined) {
        const size = ThreeVector3_JoltVec3(halfExtents);
        const shape = new JOLT.BoxShape(size, 0.1);
        JOLT.destroy(size);

        const pos = position ? ThreeVector3_JoltVec3(position) : new JOLT.Vec3(0.0, 0.0, 0.0);
        const rot = _JoltQuat(rotation);
        const creationSettings = new JOLT.BodyCreationSettings(
            shape,
            pos,
            rot,
            mass ? JOLT.EMotionType_Dynamic : JOLT.EMotionType_Static,
            LAYER_NOT_MOVING
        );
        if (mass) {
            creationSettings.mMassPropertiesOverride.mMass = mass;
        }
        const body = this._joltBodyInterface.CreateBody(creationSettings);
        JOLT.destroy(pos);
        JOLT.destroy(rot);
        JOLT.destroy(creationSettings);

        this._bodies.push(body);
        return body;
    }

    public createConvexHull() {
        const helper = new JOLT.SynthesisHelper();
        // TODO
    }
}

function setupCollisionFiltering(settings: any) {
    const objectFilter = new JOLT.ObjectLayerPairFilterTable(COUNT_OBJECT_LAYERS);
    objectFilter.EnableCollision(LAYER_NOT_MOVING, LAYER_MOVING);
    objectFilter.EnableCollision(LAYER_MOVING, LAYER_MOVING);

    const BP_LAYER_NOT_MOVING = new JOLT.BroadPhaseLayer(LAYER_NOT_MOVING);
    const BP_LAYER_MOVING = new JOLT.BroadPhaseLayer(LAYER_MOVING);
    const COUNT_BROAD_PHASE_LAYERS = 2;

    const bpInterface = new JOLT.BroadPhaseLayerInterfaceTable(COUNT_OBJECT_LAYERS, COUNT_BROAD_PHASE_LAYERS);
    bpInterface.MapObjectToBroadPhaseLayer(LAYER_NOT_MOVING, BP_LAYER_NOT_MOVING);
    bpInterface.MapObjectToBroadPhaseLayer(LAYER_MOVING, BP_LAYER_MOVING);

    settings.mObjectLayerPairFilter = objectFilter;
    settings.mBroadPhaseLayerInterface = bpInterface;
    settings.mObjectVsBroadPhaseLayerFilter = new JOLT.ObjectVsBroadPhaseLayerFilterTable(
        settings.mBroadPhaseLayerInterface,
        COUNT_BROAD_PHASE_LAYERS,
        settings.mObjectLayerPairFilter,
        COUNT_OBJECT_LAYERS
    );
}