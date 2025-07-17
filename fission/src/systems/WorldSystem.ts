abstract class WorldSystem {
    public abstract update(deltaT: number): void
    public abstract destroy(): void
}
export default WorldSystem
