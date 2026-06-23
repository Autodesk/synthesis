import { type AllianceStation, RobotSimMode, SimType } from "../WPILibTypes"
import SimGeneric from "./SimGeneric"

export default class SimDriverStation {
    private constructor() {}

    public static setMatchTime(time: number) {
        SimGeneric.set<number>(SimType.DRIVERS_STATION, "", ">match_time", time)
    }

    public static setGameData(gameData: string) {
        SimGeneric.set<string>(SimType.DRIVERS_STATION, "", ">match_time", gameData)
    }

    public static isEnabled(): boolean {
        return SimGeneric.getUnsafe<boolean>(SimType.DRIVERS_STATION, "", ">enabled", false)
    }

    public static setMode(mode: RobotSimMode) {
        const enabled = mode != RobotSimMode.DISABLED
        const autonomous = mode == RobotSimMode.AUTO
        SimGeneric.set<boolean>(SimType.DRIVERS_STATION, "", ">ds", true)
        SimGeneric.set<boolean>(SimType.DRIVERS_STATION, "", ">enabled", enabled)
        SimGeneric.set<boolean>(SimType.DRIVERS_STATION, "", ">autonomous", autonomous)
    }

    public static setStation(station: AllianceStation) {
        SimGeneric.set<string>(SimType.DRIVERS_STATION, "", ">station", station)
    }

    public static setDsAttached(attached: boolean) {
        SimGeneric.set<boolean>(SimType.DRIVERS_STATION, "", ">ds", attached)
    }
}
