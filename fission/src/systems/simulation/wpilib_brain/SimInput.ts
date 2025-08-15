export abstract class SimInput {
    constructor(protected _device: string) {}

    public abstract update(deltaT: number): void

    public get device(): string {
        return this._device
    }
}
