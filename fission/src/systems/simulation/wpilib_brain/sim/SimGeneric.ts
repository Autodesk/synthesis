import { getSimMap } from "../WPILibState"
import { FieldType, SimMapUpdateEvent, type SimType, worker } from "../WPILibTypes"

function getFieldType(field: string): FieldType {
    if (field.length < 2) {
        return FieldType.UNKNOWN
    }

    switch (field.charAt(0)) {
        case "<":
            return field.charAt(1) == ">" ? FieldType.BOTH : FieldType.READ
        case ">":
            return FieldType.WRITE
        default:
            return FieldType.UNKNOWN
    }
}

export default class SimGeneric {
    private constructor() {}

    public static getUnsafe<T>(simType: SimType, device: string, field: string): T | undefined
    public static getUnsafe<T>(simType: SimType, device: string, field: string, defaultValue: T): T
    public static getUnsafe<T>(simType: SimType, device: string, field: string, defaultValue?: T): T | undefined {
        const map = getSimMap()?.get(simType)
        if (!map) {
            // console.warn(`No '${simType}' devices found`)
            return undefined
        }

        const data = map.get(device)
        if (!data) {
            // console.warn(`No '${simType}' device '${device}' found`)
            return undefined
        }

        return (data.get(field) as T | undefined) ?? defaultValue
    }

    public static get<T>(simType: SimType, device: string, field: string): T | undefined
    public static get<T>(simType: SimType, device: string, field: string, defaultValue: T): T
    public static get<T>(simType: SimType, device: string, field: string, defaultValue?: T): T | undefined {
        const fieldType = getFieldType(field)
        if (fieldType != FieldType.READ && fieldType != FieldType.BOTH) {
            console.warn(`Field '${field}' is not a read or both field type`)
            return undefined
        }

        const map = getSimMap()?.get(simType)
        if (!map) {
            // console.warn(`No '${simType}' devices found`)
            return undefined
        }

        const data = map.get(device)
        if (!data) {
            // console.warn(`No '${simType}' device '${device}' found`)
            return undefined
        }

        return (data.get(field) as T | undefined) ?? defaultValue
    }

    public static set<T extends number | boolean | string>(
        simType: SimType,
        device: string,
        field: string,
        value: T
    ): boolean {
        const fieldType = getFieldType(field)
        if (fieldType != FieldType.WRITE && fieldType != FieldType.BOTH) {
            console.warn(`Field '${field}' is not a write or both field type`)
            return false
        }

        const map = getSimMap()?.get(simType)
        if (!map) {
            // console.warn(`No '${simType}' devices found`)
            return false
        }

        const data = map.get(device)
        if (!data) {
            // console.warn(`No '${simType}' device '${device}' found`)
            return false
        }

        const selectedData: { [key: string]: number | boolean | string } = {}
        selectedData[field] = value
        data.set(field, value)

        worker.getValue().postMessage({
            command: "update",
            data: {
                type: simType,
                device: device,
                data: selectedData,
            },
        })

        window.dispatchEvent(new SimMapUpdateEvent(true))
        return true
    }
}
