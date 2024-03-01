import { Type } from "typescript";
import Queue from "../../util/data/Queue";

class WorldSystem {

    private _systems: Array<System>;
    private _entities: Array<number>;
    private _removedEntityIndexes: Queue<number>;
    private _components: Map<Type, Component>;

    constructor() {
        this._systems = new Array();
        this._entities = new Array();
    }

    addComponent() {

    }

    removeComponent() {

    }

    getComponent<T extends Component>(entity: Entity) {

    }

}
export var worldSystem = new WorldSystem();

export abstract class System {
    protected abstract componentAdded(comp: Component);
}

export class Component {
        
    private _owner: Entity;

    public get owner(): Entity { return this._owner; }

    constructor(owner: Entity | undefined) {
        owner && (this._owner = owner);
    }
}

class Entity {
    private _kill = false;
    private _index: number;
    private _gen: number;

    constructor(index: number, gen: number) {
        this._index = index;
        this._gen = gen;
    }

    kill() {
        this._kill = true;
    }
}
