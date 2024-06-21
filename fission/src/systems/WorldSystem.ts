abstract class WorldSystem {
    public abstract Update(deltaT: number): void;
    public abstract Destroy(): void;
}
export default WorldSystem;