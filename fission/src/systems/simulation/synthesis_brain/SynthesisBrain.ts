import Mechanism from "@/systems/physics/Mechanism";
import Brain from "../Brain";
import Behavior from "./Behavior";

class SynthesisBrain extends Brain {

    public _behaviors: Behavior[] = [];

    public constructor(mechanism: Mechanism) {
        super(mechanism);
    }

    public Update(_: number): void { }
    public Enable(): void { }
    public Disable(): void {
        this._behaviors = [];
    }

}

export default SynthesisBrain;