import DefaultAssetLoader from "@/mirabuf/DefaultAssetLoader"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import type { mirabuf } from "@/proto/mirabuf"
import type { LibraryPartRef } from "./MixAndMatchTypes"

/**
 * The catalog of parts a build can be assembled from.
 *
 * A library part is a whole assembly — a chassis frame, a swerve pod, an elevator — not a raw
 * sub-part, so v1 draws straight from the assemblies already available to the user rather than
 * introducing a second asset pipeline.
 */

/**
 * Optional key a part author can set in their own mira's `Parts.user_data` to declare the discrete
 * sizes that part ships in. JSON array of {@link PartSizeOption}. Absent on parts that aren't
 * resizable, which is every bought-whole vendor assembly.
 */
export const MIX_AND_MATCH_SIZES_KEY = "mixAndMatchSizes"

export interface PartSizeOption {
    id: string
    label: string
    /** The library part to swap in for this size. May be the declaring part itself. */
    partRef: LibraryPartRef
}

export interface LibraryPart {
    ref: LibraryPartRef
    name: string
    /** False when the part still has to be downloaded before it can be spawned. */
    cached: boolean
    remotePath?: string
}

function stripExtension(name: string): string {
    return name.replace(/\.mira$/, "")
}

class PartLibrary {
    /** Everything spawnable right now, cached assets first. */
    public static list(): LibraryPart[] {
        const cached = MirabufCachingService.getAll(MiraType.ROBOT).map<LibraryPart>(info => ({
            ref: info.hash,
            name: stripExtension(info.name || "Unnamed"),
            cached: true,
            remotePath: info.remotePath,
        }))

        const cachedRefs = new Set(cached.map(part => part.ref))
        const remote = DefaultAssetLoader.robots
            .filter(asset => !cachedRefs.has(asset.hash))
            .map<LibraryPart>(asset => ({
                ref: asset.hash,
                name: stripExtension(asset.name),
                cached: false,
                remotePath: asset.remotePath,
            }))

        return [...cached, ...remote].sort((a, b) => a.name.localeCompare(b.name))
    }

    public static find(ref: LibraryPartRef): LibraryPart | undefined {
        return this.list().find(part => part.ref === ref)
    }

    /**
     * Decodes a fresh assembly for a library part, downloading it first if it isn't cached yet.
     *
     * Each call returns its own assembly, so the same library part can be spawned many times in one
     * build without the copies sharing a parser.
     */
    public static async load(ref: LibraryPartRef): Promise<mirabuf.Assembly | undefined> {
        const part = this.find(ref)

        if (part && !part.cached && part.remotePath) {
            const info = await MirabufCachingService.cacheRemote(part.remotePath, MiraType.ROBOT, part.name, part.ref)
            if (!info) {
                console.error(`Failed to download library part ${part.name}`)
                return undefined
            }
        }

        return await MirabufCachingService.get(ref)
    }

    /** @returns The discrete sizes a part declares, or an empty list when it isn't resizable. */
    public static sizesOf(assembly: mirabuf.IAssembly): PartSizeOption[] {
        const raw = assembly.data?.parts?.userData?.data?.[MIX_AND_MATCH_SIZES_KEY]
        if (!raw) return []

        try {
            const parsed: unknown = JSON.parse(raw)
            if (!Array.isArray(parsed)) return []

            return parsed.filter(
                (option): option is PartSizeOption =>
                    typeof option === "object" &&
                    option != null &&
                    typeof option.id === "string" &&
                    typeof option.label === "string" &&
                    typeof option.partRef === "string"
            )
        } catch (e) {
            console.warn(`Malformed ${MIX_AND_MATCH_SIZES_KEY} on ${assembly.info?.name}`, e)
            return []
        }
    }
}

export default PartLibrary
