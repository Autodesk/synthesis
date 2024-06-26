abstract class Driver {
    public abstract Update(deltaT: number): void
}

export enum DriverControlMode {
    Velocity = 0,
    Position = 1
}

export default Driver;