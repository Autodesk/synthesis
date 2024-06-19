import Mechanism from "@/systems/physics/Mechanism";
import Brain from "../Brain";
import Behavior from "../behavior/Behavior";
import World from "@/systems/World";
import WheelDriver from "../driver/WheelDriver";
import WheelRotationStimulus from "../stimulus/WheelStimulus";
import ArcadeDriveBehavior from "../behavior/ArcadeDriveBehavior";
import { SimulationLayer } from "../SimulationSystem";
import Jolt from "@barclah/jolt-physics";
import JOLT from "@/util/loading/JoltSyncLoader";
import HingeDriver from "../driver/HingeDriver";
import HingeStimulus from "../stimulus/HingeStimulus";
import GenericArmBehavior from "../behavior/GenericArmBehavior";
import SliderDriver from "../driver/SliderDriver";
import SliderStimulus from "../stimulus/SliderStimulus";
import GenericElevatorBehavior from "../behavior/GenericElevatorBehavior";


class SynthesisBrain extends Brain {
    private _behaviors: Behavior[] = [];
    private _simLayer: SimulationLayer;

    _leftWheelIndices: number[] = [];

    // Tracks how many joins have been made for unique controls
    _currentJointIndex = 1;

    public constructor(mechanism: Mechanism) {
        super(mechanism);

        this._simLayer = World.SimulationSystem.GetSimulationLayer(mechanism)!;

        if (!this._simLayer) { 
            console.log("SimulationLayer is undefined");
            return;
        }

        this.configureArcadeDriveBehavior();
        this.configureArmBehaviors();
        this.configureElevatorBehaviors();
    }

    public Enable(): void { }

    public Update(deltaT: number): void { 
        this._behaviors.forEach((b) => b.Update(deltaT)); 
    }

    public Disable(): void {
        this._behaviors = [];
    }

    // Creates an instance of ArcadeDriveBehavior and automatically configures it
    configureArcadeDriveBehavior() {
        let wheelDrivers: WheelDriver[] =  this._simLayer.drivers.filter((driver) => driver instanceof WheelDriver) as WheelDriver[];
        let wheelStimuli: WheelRotationStimulus[] =  this._simLayer.stimuli.filter((stimulus) => stimulus instanceof WheelRotationStimulus) as WheelRotationStimulus[];

        // Two body constraints are part of wheels and are used to determine which way a wheel is facing
        let fixedConstraints: Jolt.TwoBodyConstraint[] = this._mechanism.constraints.filter((mechConstraint) => mechConstraint.constraint instanceof JOLT.TwoBodyConstraint).map((mechConstraint) => mechConstraint.constraint as Jolt.TwoBodyConstraint);

        let leftWheels: WheelDriver[] = [];
        let leftStimuli: WheelRotationStimulus[] = [];

        let rightWheels: WheelDriver[] = [];
        let rightStimuli: WheelRotationStimulus[] = [];

        // Determines which wheels and stimuli belong to which side of the robot
        for (let i = 0; i < wheelDrivers.length; i++) {
            let wheelPos = fixedConstraints[i].GetConstraintToBody1Matrix().GetTranslation();

            let robotCOM = World.PhysicsSystem.GetBody(this._mechanism.constraints[0].childBody).GetCenterOfMassPosition() as Jolt.Vec3;
            let rightVector = new JOLT.Vec3(1, 0, 0);

            let dotProduct = rightVector.Dot(wheelPos.Sub(robotCOM))

            if (dotProduct < 0) {
                rightWheels.push(wheelDrivers[i]);
                rightStimuli.push(wheelStimuli[i]);
            }
            else {
                leftWheels.push(wheelDrivers[i]);
                leftStimuli.push(wheelStimuli[i]);
            }
        }

        this._behaviors.push(new ArcadeDriveBehavior(leftWheels, rightWheels, leftStimuli, rightStimuli));
    }

    // Creates instances of ArmBehavior and automatically configures them
    configureArmBehaviors() {
        let hingeDrivers: HingeDriver[] =  this._simLayer.drivers.filter((driver) => driver instanceof HingeDriver) as HingeDriver[];
        let hingeStimuli: HingeStimulus[] =  this._simLayer.stimuli.filter((stimulus) => stimulus instanceof HingeStimulus) as HingeStimulus[];

        for (let i = 0; i < hingeDrivers.length; i++) {
            this._behaviors.push(new GenericArmBehavior(hingeDrivers[i], hingeStimuli[i], this._currentJointIndex));
            this._currentJointIndex++;
        }
    }

    // Creates instances of ElevatorBehavior and automatically configures them
    configureElevatorBehaviors() {
        let sliderDrivers: SliderDriver[] =  this._simLayer.drivers.filter((driver) => driver instanceof SliderDriver) as SliderDriver[];
        let sliderStimuli: SliderStimulus[] =  this._simLayer.stimuli.filter((stimulus) => stimulus instanceof SliderStimulus) as SliderStimulus[];

        for (let i = 0; i < sliderDrivers.length; i++) {
            this._behaviors.push(new GenericElevatorBehavior(sliderDrivers[i], sliderStimuli[i], this._currentJointIndex));
            this._currentJointIndex++;
        }
    }
}

export default SynthesisBrain;32