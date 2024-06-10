import Mechanism from "@/systems/physics/Mechanism";
import Brain from "../Brain";

class WPILibBrain extends Brain {

    constructor(mech: Mechanism) {
        super(mech)
    }

    public Update(deltaT: number): void { }
    public Enable(): void { }
    public Disable(): void { }

}

export default WPILibBrain