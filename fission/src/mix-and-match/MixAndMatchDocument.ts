import type { mirabuf } from "@/proto/mirabuf"
import {
    createEmptySession,
    MIX_AND_MATCH_SESSION_VERSION,
    MIX_AND_MATCH_USER_DATA_KEY,
    type MixAndMatchComponent,
    type MixAndMatchSession,
    type TimelineEntry,
    type TransformArray,
} from "./MixAndMatchTypes"

/**
 * Reads and writes the session document to `Parts.user_data["mixAndMatchSession"]` as a JSON string.
 *
 * A typed proto message is deliberately not used here: the map field is forward compatible, so a
 * mira carrying this key is still an ordinary robot to every other consumer.
 */

const TRANSFORM_LENGTH = 16

function isTransform(value: unknown): value is TransformArray {
    return Array.isArray(value) && value.length === TRANSFORM_LENGTH && value.every(x => typeof x === "number")
}

function isRecord(value: unknown): value is Record<string, unknown> {
    return typeof value === "object" && value != null && !Array.isArray(value)
}

function parseEntry(value: unknown): TimelineEntry | undefined {
    if (!isRecord(value)) return undefined

    switch (value.type) {
        case "spawn":
            return typeof value.componentId === "string" &&
                typeof value.libraryPartRef === "string" &&
                isTransform(value.transform)
                ? {
                      type: "spawn",
                      componentId: value.componentId,
                      libraryPartRef: value.libraryPartRef,
                      transform: [...value.transform],
                  }
                : undefined
        case "move":
            return typeof value.componentId === "string" && isTransform(value.transform)
                ? { type: "move", componentId: value.componentId, transform: [...value.transform] }
                : undefined
        case "weld":
            return typeof value.componentA === "string" &&
                typeof value.componentB === "string" &&
                isTransform(value.relativeOffset)
                ? {
                      type: "weld",
                      componentA: value.componentA,
                      componentB: value.componentB,
                      relativeOffset: [...value.relativeOffset],
                  }
                : undefined
        case "resize":
            return typeof value.componentId === "string" && typeof value.sizeOption === "string"
                ? { type: "resize", componentId: value.componentId, sizeOption: value.sizeOption }
                : undefined
        case "delete":
            return typeof value.componentId === "string"
                ? { type: "delete", componentId: value.componentId }
                : undefined
        default:
            return undefined
    }
}

function parseComponent(value: unknown): MixAndMatchComponent | undefined {
    if (!isRecord(value)) return undefined
    if (typeof value.id !== "string" || typeof value.libraryPartRef !== "string") return undefined

    return { id: value.id, libraryPartRef: value.libraryPartRef }
}

export function serializeSession(session: MixAndMatchSession): string {
    return JSON.stringify(session)
}

/**
 * Parses a serialized session, dropping anything malformed rather than throwing. A partially
 * readable document is more useful than none: the user can still see and fix their build.
 *
 * @param   json    Raw JSON previously written by {@link serializeSession}.
 * @returns The session, or undefined if the document isn't a mix-and-match session of a known version.
 */
export function parseSession(json: string): MixAndMatchSession | undefined {
    let raw: unknown
    try {
        raw = JSON.parse(json)
    } catch (e) {
        console.warn("Malformed mix-and-match session", e)
        return undefined
    }

    if (!isRecord(raw)) return undefined
    if (raw.version !== MIX_AND_MATCH_SESSION_VERSION) {
        console.warn(`Unsupported mix-and-match session version: ${raw.version}`)
        return undefined
    }

    const session = createEmptySession()
    if (Array.isArray(raw.components)) {
        raw.components.forEach(x => {
            const component = parseComponent(x)
            if (component) session.components.push(component)
        })
    }
    if (Array.isArray(raw.timeline)) {
        raw.timeline.forEach(x => {
            const entry = parseEntry(x)
            if (entry) session.timeline.push(entry)
        })
    }

    return session
}

export function hasMixAndMatchSession(assembly: mirabuf.IAssembly): boolean {
    return assembly.data?.parts?.userData?.data?.[MIX_AND_MATCH_USER_DATA_KEY] != null
}

export function readSessionFromAssembly(assembly: mirabuf.IAssembly): MixAndMatchSession | undefined {
    const raw = assembly.data?.parts?.userData?.data?.[MIX_AND_MATCH_USER_DATA_KEY]
    return raw == null ? undefined : parseSession(raw)
}

/**
 * Stamps the session onto an assembly's part user data. Mutates `assembly` in place, creating the
 * `userData` map if the exporter left it off.
 */
export function writeSessionToAssembly(assembly: mirabuf.IAssembly, session: MixAndMatchSession): boolean {
    const parts = assembly.data?.parts
    if (!parts) {
        console.warn("Cannot write mix-and-match session: assembly has no parts")
        return false
    }

    parts.userData ??= { data: {} }
    parts.userData.data ??= {}
    parts.userData.data[MIX_AND_MATCH_USER_DATA_KEY] = serializeSession(session)

    return true
}
