import { Type } from "typescript";

export class WorldSystem {

    private systems: Array<System>;
    private entities: Array<number>;
    private components: Map<Type, Component>;

    constructor() {
        this.systems = new Array();
        this.entities = new Array();

    }

}

export abstract class System {
    protected abstract componentAdded(comp: Component);
}

export class Component {
        
    private owner: number;

    constructor(owner: number | undefined) {
        owner && (this.owner = owner);
    }
}
