import { getSimMap } from "../WPILibState"
import type { DeviceData, SimType } from "../WPILibTypes"

export default class SimCAN {
    private constructor() {}

    public static getDeviceWithID(id: number, type: SimType): DeviceData | undefined {
        const idExp = /SYN.*\[(\d+)\]/g
        const map = getSimMap()
        if (!map) return undefined
        const entries = [...map.entries()].filter(([simType, _data]) => simType == type)
        for (const [_simType, data] of entries) {
            for (const key of data.keys()) {
                const result = [...key.matchAll(idExp)]
                if (result?.length <= 0 || result[0].length <= 1) continue
                const parsedId = parseInt(result[0][1])
                if (parsedId != id) continue
                return data.get(key)
            }
        }
        return undefined
    }
}
