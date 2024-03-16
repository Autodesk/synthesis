import { MirabufVector3_JoltVec3, MirabufVector3_ThreeVector3, ThreeVector3_JoltVec3, _JoltQuat } from "../../util/TypeConversions";
import JOLT from "../../util/loading/JoltSyncLoader";
import Jolt from "@barclah/jolt-physics";
import * as THREE from 'three';
import { mirabuf } from '../../proto/mirabuf';
import MirabufParser from "../../mirabuf/MirabufParser";

const LAYER_NOT_MOVING = 0;
const LAYER_MOVING = 1;
const COUNT_OBJECT_LAYERS = 2;

const STANDARD_TIME_STEP = 1.0 / 60.0;
const STANDARD_SUB_STEPS = 1;

/**
 * The PhysicsSystem handles all Jolt Phyiscs interactions within Synthesis.
 * This system can create physical representations of objects such as Robots,
 * Fields, and Game pieces, and simulate them.
 */
export class PhysicsSystem {
    
    private _joltInterface: Jolt.JoltInterface;
    private _joltPhysSystem: Jolt.PhysicsSystem;
    private _joltBodyInterface: Jolt.BodyInterface;
    private _bodies: Array<Jolt.Body>;

    /**
     * Creates a PhysicsSystem object.
     */
    constructor() {
        this._bodies = [];

        const joltSettings = new JOLT.JoltSettings();
        SetupCollisionFiltering(joltSettings);

        this._joltInterface = new JOLT.JoltInterface(joltSettings);
        JOLT.destroy(joltSettings);

        this._joltPhysSystem = this._joltInterface.GetPhysicsSystem();
        this._joltBodyInterface = this._joltPhysSystem.GetBodyInterface();
    }

    /**
     * TEMPORARY
     * Create a box.
     * 
     * @param   halfExtents The half extents of the Box.
     * @param   mass        Mass of the Box. Leave undefined to make Box static.
     * @param   position    Posiition of the Box (default: 0, 0, 0)
     * @param   rotation    Rotation of the Box (default 0, 0, 0, 1)
     * @returns Reference to Jolt Body
     */
    public CreateBox(
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

    public CreateBody(
    shape: Jolt.Shape,
    mass: number | undefined,
    position: THREE.Vector3 | undefined,
    rotation: THREE.Euler | THREE.Quaternion | undefined) {
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

    public CreateConvexHull(points: Float32Array, density: number = 1.0) {
        if (points.length % 3) {
            throw new Error(`Invalid size of points: ${points.length}`);
        }
        const settings = new JOLT.ConvexHullShapeSettings();
        settings.mPoints.clear();
        settings.mPoints.reserve(points.length / 3.0);
        for (let i = 0; i < points.length; i += 3) {
            settings.mPoints.push_back(new JOLT.Vec3(points[i], points[i + 1], points[i + 2]));
        }
        settings.mDensity = density;
        return settings.Create();
    }

    public CreateBodiesFromParser(parser: MirabufParser): Map<string, Jolt.Body> {
        const rnToBodies = new Map<string, Jolt.Body>();
        
        parser.rigidNodes.forEach(rn => {
            rn.parts.forEach(partId => {
                const partInstance = parser.assembly.data!.parts!.partInstances![partId]!;
                const partDefinition = parser.assembly.data!.parts!.partDefinitions![partInstance.partDefinitionReference!];
                
            });
        });

        return rnToBodies;
    }

    private CreateColliderFromPart(partDefinition: mirabuf.PartDefinition): Jolt.ShapeResult {
        const settings = new JOLT.ConvexHullShapeSettings();
        const points = settings.mPoints;
        partDefinition.bodies.forEach(body => {
            if (body.triangleMesh && body.triangleMesh.mesh && body.triangleMesh.mesh.verts) {
                const vertArr = body.triangleMesh.mesh.verts;
                for (let i = 0; i < body.triangleMesh.mesh.verts.length; i += 3) {
                    points.push_back(new JOLT.Vec3(vertArr[i], vertArr[i + 1], vertArr[i + 2]));
                }
            }
        });

        return settings.Create();
    }

    public Step() {
        this._joltInterface.Step(STANDARD_TIME_STEP, STANDARD_SUB_STEPS);
    }

    public Destroy() {
        // Destroy Jolt Bodies.
        this._bodies.forEach(x => JOLT.destroy(x));
        this._bodies = [];

        JOLT.destroy(this._joltInterface);
    }
}

function SetupCollisionFiltering(settings: Jolt.JoltSettings) {
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