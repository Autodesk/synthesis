import Mechanism, { MechanismConstraint } from "@/systems/physics/Mechanism";
import Brain from "../Brain";
import Behavior from "../behavior/Behavior";
import World from "@/systems/World";
import WheelDriver from "../driver/WheelDriver";
import WheelRotationStimulus from "../stimulus/WheelStimulus";
import ArcadeDriveBehavior from "../behavior/ArcadeDriveBehavior";
import { SimulationLayer } from "../SimulationSystem";
import * as THREE from 'three';
import { JoltMat44_ThreeMatrix4 } from "@/util/TypeConversions";
import Jolt from "@barclah/jolt-physics";
import { JOLT_TYPES } from "@/util/loading/JoltAsyncLoader";
import JOLT from "@/util/loading/JoltSyncLoader";


class SynthesisBrain extends Brain {
    private _behaviors: Behavior[] = [];
    private _simLayer: SimulationLayer;

    private _debugBodies: Map<Jolt.TwoBodyConstraint, THREE.Mesh>;

    public constructor(mechanism: Mechanism) {
        super(mechanism);

        this._simLayer = World.SimulationSystem.GetSimulationLayer(mechanism)!;

        this._debugBodies = new Map();

        if (!this._simLayer) { 
            console.log("Simulation Layer is undefined");
            return;
        }

        console.log("config arcade drive");
        this.configureArcadeDriveBehavior();

        // const comMesh = World.SceneRenderer.CreateSphere(0.05);
        // World.SceneRenderer.scene.add(comMesh);

        // (comMesh.material as THREE.Material).depthTest = false;

        // this._debugMeshes.push(comMesh);

        // console.log(mechanism.nodeToBody);
                //this._debugBodies!.set(rnName, { colliderMesh: colliderMesh, comMesh: comMesh });
        //comMesh.position = new THREE.Vector3(0, 0, 0);

        this._mechanism.constraints.forEach((c) => {
            if (!(c.constraint instanceof JOLT.TwoBodyConstraint))
                return;
        
            const comMesh = World.SceneRenderer.CreateSphere(0.05);
            (comMesh.material as THREE.Material).depthTest = false;

            World.SceneRenderer.scene.add(comMesh);

            this._debugBodies.set(c.constraint as Jolt.TwoBodyConstraint, comMesh);
        });
        console.log("Number of constraints: " + this._debugBodies.size);
    }

    public Enable(): void { }
    public Update(deltaT: number): void { 
        this._behaviors.forEach((b) => b.Update(deltaT)); 
let i = 0;
        this._debugBodies.forEach((value, key) => {
            if (i != 0)return;
            const transform = JoltMat44_ThreeMatrix4(key.GetConstraintToBody1Matrix());            
            value!.position.setFromMatrixPosition(transform);
            i++;
        });
    }
    public Disable(): void {
        this._behaviors = [];
    }

    configureArcadeDriveBehavior() {
        let wheelDrivers: WheelDriver[] =  this._simLayer.drivers.filter((driver) => driver instanceof WheelDriver) as WheelDriver[];
        let wheelStimuli: WheelRotationStimulus[] =  this._simLayer.stimuli.filter((stimulus) => stimulus instanceof WheelRotationStimulus) as WheelRotationStimulus[];

        let fixedConstraints: Jolt.TwoBodyConstraint[] = this._mechanism.constraints.filter((mechConstraint) => mechConstraint.constraint instanceof JOLT.TwoBodyConstraint).map((mechConstraint) => mechConstraint.constraint as Jolt.TwoBodyConstraint);
        console.log(fixedConstraints.length);

        //wheelDrivers.forEach((w) => console.log(w.constraint))

        this._behaviors.push(new ArcadeDriveBehavior([wheelDrivers[0], wheelDrivers[1], wheelDrivers[2]], [wheelDrivers[3], wheelDrivers[4], wheelDrivers[5]], [wheelStimuli[0], wheelStimuli[1], wheelStimuli[2]], [wheelStimuli[3], wheelStimuli[4], wheelStimuli[5]]));
    }

    getLeftRightWheels() {
        throw new Error("Method not implemented.");
    }
}

export default SynthesisBrain;