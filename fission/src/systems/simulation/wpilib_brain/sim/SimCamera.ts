import { getSimMap } from "../WPILibState"
import { CAMERA_FPS, CAMERA_HEIGHT, CAMERA_WIDTH, SimType } from "../WPILibTypes"
import SimGeneric from "./SimGeneric"

// HALSim only carries camera config (resolution/fps/connected) and existence; the frame is
// streamed separately (see CameraFrameSocket)
export default class SimCamera {
    private constructor() {}

    public static getWidth(device: string, defaultValue: number): number {
        return SimGeneric.get<number>(SimType.CAMERA, device, CAMERA_WIDTH, defaultValue)
    }

    public static getHeight(device: string, defaultValue: number): number {
        return SimGeneric.get<number>(SimType.CAMERA, device, CAMERA_HEIGHT, defaultValue)
    }

    public static getFps(device: string, defaultValue: number): number {
        return SimGeneric.get<number>(SimType.CAMERA, device, CAMERA_FPS, defaultValue)
    }

    public static isPresent(device: string): boolean {
        return !!getSimMap()?.get(SimType.CAMERA)?.get(device)
    }
}
