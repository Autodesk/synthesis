import DefaultAssetLoader from "@/mirabuf/DefaultAssetLoader"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import type { mirabuf } from "@/proto/mirabuf"
import type { LibraryPartRef } from "./MixAndMatchTypes"

/**
 * The catalog of parts a build can be assembled from.
 *
 * Library parts are of type `MiraType.COMPONENT`.
 */

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
        const cached = MirabufCachingService.getAll(MiraType.COMPONENT).map<LibraryPart>(info => ({
            ref: info.hash,
            name: stripExtension(info.name || "Unnamed"),
            cached: true,
            remotePath: info.remotePath,
        }))

        const cachedRefs = new Set(cached.map(part => part.ref))
        const remote = DefaultAssetLoader.components
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
            const info = await MirabufCachingService.cacheRemote(
                part.remotePath,
                MiraType.COMPONENT,
                part.name,
                part.ref
            )
            if (!info) {
                console.error(`Failed to download library part ${part.name}`)
                return undefined
            }
        }

        return await MirabufCachingService.get(ref)
    }
}

export default PartLibrary
