import type { VariantType } from "notistack"
import { tryParse } from "@/util/Utility.ts"
import { globalAddToast } from "@/components/GlobalUIControls.ts"

interface SessionStorageData {
    autoOpenTo: "singleplayer" | "multiplayer"
    autoToast: { type: VariantType; lines: string[] }
}

interface SessionStorageEntry<K extends keyof SessionStorageData> {
    clearAfterLoad: boolean
    data: SessionStorageData[K]
}

export function applyAutoToast() {
    const toast = SessionStorage.load("autoToast")
    if (toast) {
        globalAddToast(toast.type, ...toast.lines)
    }
}

class SessionStorage {
    public static saveOnce<Key extends keyof SessionStorageData>(key: Key, value: SessionStorageData[Key]) {
        this.save(key, {
            clearAfterLoad: true,
            data: value,
        })
    }
    public static saveForSession<Key extends keyof SessionStorageData>(key: Key, value: SessionStorageData[Key]) {
        this.save(key, {
            clearAfterLoad: false,
            data: value,
        })
    }

    private static save<Key extends keyof SessionStorageData>(key: Key, value: SessionStorageEntry<Key>) {
        sessionStorage.setItem(key, JSON.stringify(value))
    }

    public static load<Key extends keyof SessionStorageData>(key: Key): SessionStorageData[Key] | undefined {
        const stringEntry = sessionStorage.getItem(key)
        if (stringEntry == null) return
        const entry = tryParse(stringEntry) as SessionStorageEntry<Key> | undefined
        if (entry == null) return
        if (entry.clearAfterLoad) {
            sessionStorage.removeItem(key)
        }
        return entry.data
    }

    public static clear() {
        sessionStorage.clear()
    }
}

export default SessionStorage
