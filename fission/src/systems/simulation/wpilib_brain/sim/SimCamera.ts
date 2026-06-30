import { getSimMap } from "../WPILibState"
import { CAMERA_CONNECTED, CAMERA_FPS, CAMERA_HEIGHT, CAMERA_WIDTH, SimType } from "../WPILibTypes"
import SimGeneric from "./SimGeneric"

/**
 * Accessor for simulated USB cameras over HALSim.
 *
 * The robot code configures the desired resolution / fps / connected state, which
 * Synthesis reads back here to size and throttle its render, and to discover that the
 * camera exists. The rendered frame itself is streamed separately (see CameraFrameSocket)
 * because SimDevice fields only carry numbers/booleans.
 */
export default class SimCamera {
    private constructor() {}

    /** Resolution width requested by the robot code (falls back to `defaultValue`). */
    public static getWidth(device: string, defaultValue: number): number {
        return SimGeneric.get<number>(SimType.CAMERA, device, CAMERA_WIDTH, defaultValue)
    }

    /** Resolution height requested by the robot code (falls back to `defaultValue`). */
    public static getHeight(device: string, defaultValue: number): number {
        return SimGeneric.get<number>(SimType.CAMERA, device, CAMERA_HEIGHT, defaultValue)
    }

    /** Frame rate requested by the robot code (falls back to `defaultValue`). */
    public static getFps(device: string, defaultValue: number): number {
        return SimGeneric.get<number>(SimType.CAMERA, device, CAMERA_FPS, defaultValue)
    }

    /** Whether the robot code has marked the camera as connected. */
    public static getConnected(device: string): boolean {
        return SimGeneric.get<boolean>(SimType.CAMERA, device, CAMERA_CONNECTED, false)
    }

    /**
     * Whether the robot code has created this camera device yet. Frames are only worth
     * encoding/sending once the device is present in the sim map.
     */
    public static isPresent(device: string): boolean {
        return !!getSimMap()?.get(SimType.CAMERA)?.get(device)
    }
}
