import { Reader } from "protobufjs/minimal"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { mirabuf } from "@/proto/mirabuf"
import {
    THUMBNAIL_EXTENSION,
    THUMBNAIL_IS_TRANSPARENT,
    THUMBNAIL_SIZE,
    thumbnailMimeType,
} from "@/systems/scene/ThumbnailCapture"
import World from "@/systems/World"
import { unzipMira } from "@/util/Utility"

const ASSEMBLY_THUMBNAIL_TAG = Reader.create(
    mirabuf.Assembly.encode(new mirabuf.Assembly({ thumbnail: new mirabuf.Thumbnail() })).finish()
).uint32()

const thumbnailsByAssemblyHash = new Map<string, Promise<Blob | undefined>>()

export async function embedAssemblyThumbnail(target: MirabufSceneObject): Promise<void> {
    const blob = await World.sceneRenderer.captureAssemblyThumbnail(target)
    if (!blob) return

    const assembly = target.mirabufInstance.parser.assembly
    assembly.thumbnail = new mirabuf.Thumbnail({
        width: THUMBNAIL_SIZE,
        height: THUMBNAIL_SIZE,
        extension: THUMBNAIL_EXTENSION,
        transparent: THUMBNAIL_IS_TRANSPARENT,
        data: new Uint8Array(await blob.arrayBuffer()),
    })

    await recacheAssembly(target, blob)
}

async function recacheAssembly(target: MirabufSceneObject, thumbnail: Blob): Promise<void> {
    const previousHash = target.assemblyHash
    if (previousHash == null || !MirabufCachingService.has(previousHash)) return

    await MirabufCachingService.remove(previousHash)
    thumbnailsByAssemblyHash.delete(previousHash)

    const assembly = target.mirabufInstance.parser.assembly
    const info = await MirabufCachingService.storeAssemblyInCache(assembly, {
        miraType: assembly.dynamic ? MiraType.ROBOT : MiraType.FIELD,
    })
    if (!info) return

    target.assemblyHash = info.hash
    thumbnailsByAssemblyHash.set(info.hash, Promise.resolve(thumbnail))
}

export function getCachedThumbnail(hash: string): Promise<Blob | undefined> {
    if (!MirabufCachingService.has(hash)) return Promise.resolve(undefined)

    const cached = thumbnailsByAssemblyHash.get(hash)
    if (cached) return cached

    const pending = readCachedThumbnail(hash).catch((e: unknown) => {
        thumbnailsByAssemblyHash.delete(hash)
        throw e
    })
    thumbnailsByAssemblyHash.set(hash, pending)
    return pending
}

async function readCachedThumbnail(hash: string): Promise<Blob | undefined> {
    const encoded = await MirabufCachingService.getEncoded(hash)
    if (!encoded) return undefined

    const thumbnail = decodeThumbnailField(unzipMira(new Uint8Array(encoded.buffer)))
    if (!thumbnail) return undefined

    return new Blob([thumbnail.data as BlobPart], {
        type: thumbnailMimeType(thumbnail.extension || THUMBNAIL_EXTENSION),
    })
}

/** Pulls only the thumbnail out of an encoded assembly. */
function decodeThumbnailField(assemblyBuffer: Uint8Array): mirabuf.Thumbnail | undefined {
    const reader = Reader.create(assemblyBuffer)
    while (reader.pos < reader.len) {
        const tag = reader.uint32()
        if (tag === ASSEMBLY_THUMBNAIL_TAG) return mirabuf.Thumbnail.decode(reader, reader.uint32())
        reader.skipType(tag & 7)
    }
    return undefined
}
