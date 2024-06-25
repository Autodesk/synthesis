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
import InputSystem from "@/systems/input/InputSystem";

class SynthesisBrain extends Brain {
    private _behaviors: Behavior[] = [];
    private _simLayer: SimulationLayer;

    // Tracks how many joins have been made for unique controls
    private _currentJointIndex = 1;

    // Tracks how many robots are spawned for control identification
    private _assemblyName: string

    public constructor(mechanism: Mechanism, assemblyName: string) {
        super(mechanism);

        this._simLayer = World.SimulationSystem.GetSimulationLayer(mechanism)!;
        this._assemblyName = assemblyName;

        if (!this._simLayer) { 
            console.log("SimulationLayer is undefined");
            return;
        }

        // TODO: maybe configure all the controls here instead of in the individual behaviors
        InputSystem.allInputs.set(this._assemblyName, []);

        // TODO: might need to recreate these whenever the robot is enabled, not when it's first created
        this.ConfigureArcadeDriveBehavior();
        this.ConfigureArmBehaviors();
        this.ConfigureElevatorBehaviors();
    }

    public Enable(): void { }

    public Update(deltaT: number): void { 
        this._behaviors.forEach((b) => b.Update(deltaT)); 
    }

    public Disable(): void {
        this._behaviors = [];
    }

    public clearControls(): void {
        InputSystem.allInputs.delete(this._assemblyName);
    }

    // Creates an instance of ArcadeDriveBehavior and automatically configures it
    public ConfigureArcadeDriveBehavior() {
        const wheelDrivers: WheelDriver[] =  this._simLayer.drivers.filter((driver) => driver instanceof WheelDriver) as WheelDriver[];
        const wheelStimuli: WheelRotationStimulus[] =  this._simLayer.stimuli.filter((stimulus) => stimulus instanceof WheelRotationStimulus) as WheelRotationStimulus[];

        // Two body constraints are part of wheels and are used to determine which way a wheel is facing
        const fixedConstraints: Jolt.TwoBodyConstraint[] = this._mechanism.constraints.filter((mechConstraint) => mechConstraint.constraint instanceof JOLT.TwoBodyConstraint).map((mechConstraint) => mechConstraint.constraint as Jolt.TwoBodyConstraint);

        const leftWheels: WheelDriver[] = [];
        const leftStimuli: WheelRotationStimulus[] = [];

        const rightWheels: WheelDriver[] = [];
        const rightStimuli: WheelRotationStimulus[] = [];

        // Determines which wheels and stimuli belong to which side of the robot
        for (let i = 0; i < wheelDrivers.length; i++) {
            const wheelPos = fixedConstraints[i].GetConstraintToBody1Matrix().GetTranslation();

            const robotCOM = World.PhysicsSystem.GetBody(this._mechanism.constraints[0].childBody).GetCenterOfMassPosition() as Jolt.Vec3;
            const rightVector = new JOLT.Vec3(1, 0, 0);

            const dotProduct = rightVector.Dot(wheelPos.Sub(robotCOM))

            if (dotProduct < 0) {
                rightWheels.push(wheelDrivers[i]);
                rightStimuli.push(wheelStimuli[i]);
            }
            else {
                leftWheels.push(wheelDrivers[i]);
                leftStimuli.push(wheelStimuli[i]);
            }
        }

        this._behaviors.push(new ArcadeDriveBehavior(leftWheels, rightWheels, leftStimuli, rightStimuli, this._assemblyName));
    }

    // Creates instances of ArmBehavior and automatically configures them
    public ConfigureArmBehaviors() {
        const hingeDrivers: HingeDriver[] =  this._simLayer.drivers.filter((driver) => driver instanceof HingeDriver) as HingeDriver[];
        const hingeStimuli: HingeStimulus[] =  this._simLayer.stimuli.filter((stimulus) => stimulus instanceof HingeStimulus) as HingeStimulus[];

        for (let i = 0; i < hingeDrivers.length; i++) {
            this._behaviors.push(new GenericArmBehavior(hingeDrivers[i], hingeStimuli[i], this._currentJointIndex, this._assemblyName));
            this._currentJointIndex++;
        }
    }

    // Creates instances of ElevatorBehavior and automatically configures them
    public ConfigureElevatorBehaviors() {
        const sliderDrivers: SliderDriver[] =  this._simLayer.drivers.filter((driver) => driver instanceof SliderDriver) as SliderDriver[];
        const sliderStimuli: SliderStimulus[] =  this._simLayer.stimuli.filter((stimulus) => stimulus instanceof SliderStimulus) as SliderStimulus[];

        for (let i = 0; i < sliderDrivers.length; i++) {
            this._behaviors.push(new GenericElevatorBehavior(sliderDrivers[i], sliderStimuli[i], this._currentJointIndex, this._assemblyName));
            this._currentJointIndex++;
        }
    }
}

export default SynthesisBrain;