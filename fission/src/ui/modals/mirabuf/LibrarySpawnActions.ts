import type { Data } from "@/aps/APSDataManagement"
import type { DefaultAssetInfo } from "@/mirabuf/DefaultAssetLoader.ts"
import MirabufCachingService, { type MirabufCacheInfo, MiraType } from "@/mirabuf/MirabufLoader"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import { embedAssemblyThumbnail } from "@/mirabuf/MirabufThumbnail"
import { mirabuf } from "@/proto/mirabuf"
import EventSystem from "@/systems/EventSystem"
import type { EncodedAssembly, Message } from "@/systems/multiplayer/MultiplayerTypes"
import { PAUSE_REF_ASSEMBLY_SPAWNING } from "@/systems/physics/PhysicsTypes"
import { getTargetControls } from "@/systems/scene/CameraControls"
import World from "@/systems/World"
import { globalAddToast, globalOpenPanel } from "@/ui/components/GlobalUIControls"
import { ProgressHandle } from "@/ui/components/ProgressNotificationData"
import InitialConfigPanel from "@/ui/panels/configuring/initial-config/InitialConfigPanel"

let pendingSpawns = 0

export const hasPendingSpawn = () => pendingSpawns > 0

function trackSpawn(delta: number) {
    const was = pendingSpawns > 0
    pendingSpawns += delta
    const now = pendingSpawns > 0
    if (was !== now) EventSystem.dispatch("SpawnPendingChangeEvent", now)
}

function broadcastSpawn(sceneObject: MirabufSceneObject, assembly: mirabuf.Assembly, info: MirabufCacheInfo) {
    const multiplayer = World.multiplayerSystem
    if (multiplayer == null) return

    const encodedAssembly =
        sceneObject.miraType !== MiraType.FIELD
            ? (mirabuf.Assembly.encode(assembly).finish() as EncodedAssembly)
            : undefined

    const message: Message = {
        type: "newObject",
        timestamp: Date.now(),
        data: {
            sceneObjectId: sceneObject.id,
            assembly: encodedAssembly,
            assemblyHash: info.hash,
            miraType: info.miraType,
            initialPreferences: sceneObject.getPreferenceData(),
        },
    }
    multiplayer.broadcast(message)
    multiplayer.registerOwnSceneObject(sceneObject.id)
}

// every library entry point funnels through here once its buffer is cached
export async function spawnCachedMira(info: MirabufCacheInfo, progressHandle = new ProgressHandle(info.name)) {
    if (info.miraType === MiraType.FIELD) {
        if (World.multiplayerSystem != null && World.sceneRenderer.mirabufSceneObjects.getField() != null) {
            globalAddToast("warning", "Cannot spawn a second field!")
            progressHandle.fail("Cannot spawn a second field")
            return
        }
        World.sceneRenderer.removeAllFields()
    }

    World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_SPAWNING)
    trackSpawn(1)
    try {
        const assembly = await MirabufCachingService.get(info.hash)
        if (!assembly) {
            progressHandle.fail()
            console.error(`Failed to load "${info.name}" from cache`)
            return
        }

        const sceneObject = await createMirabuf(info.hash, assembly, progressHandle)
        if (!sceneObject) {
            progressHandle.fail("No object!")
            return
        }

        World.sceneRenderer.registerSceneObject(sceneObject)

        const targetControls = getTargetControls()

        broadcastSpawn(sceneObject, assembly, info)

        if (targetControls && (info.miraType === MiraType.ROBOT || !targetControls.focusProvider)) {
            targetControls.focusProvider = sceneObject
        }

        progressHandle.done()
        World.physicsSystem.deactivateGamepieces()

        if (sceneObject.miraType === MiraType.ROBOT) {
            globalOpenPanel(InitialConfigPanel, undefined)
        }

        if (!info.remotePath && !assembly.thumbnail) embedAssemblyThumbnail(sceneObject).catch(console.error)
    } catch (e) {
        console.error(e)
        progressHandle.fail()
    } finally {
        trackSpawn(-1)
        setTimeout(() => World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_SPAWNING), 500)
    }
}

function cacheDefaultAsset(info: DefaultAssetInfo) {
    return MirabufCachingService.cacheRemote(info.remotePath, info.miraType, {
        name: info.name,
        expectedHash: info.hash,
        year: info.year,
        thumbnail: info.thumbnail,
    })
}

async function cacheAndSpawn(status: ProgressHandle, cache: () => Promise<MirabufCacheInfo | undefined>) {
    trackSpawn(1)
    try {
        const cacheInfo = await cache()
        if (cacheInfo) {
            await spawnCachedMira(cacheInfo, status)
        } else {
            status.fail("Failed to cache")
        }
    } catch (e) {
        console.error(e)
        status.fail()
    } finally {
        trackSpawn(-1)
    }
}

// fire and forget, progress goes out on the ProgressHandle
export function spawnRemote(info: DefaultAssetInfo) {
    const status = new ProgressHandle(info.name)
    status.update("Downloading from Synthesis...", 0.05)
    void cacheAndSpawn(status, () => cacheDefaultAsset(info))
}

// APS is the Autodesk Hub
export function spawnAPS(data: Data, miraType: MiraType) {
    const status = new ProgressHandle(data.attributes.displayName ?? data.id)
    status.update("Downloading from APS...", 0.05)
    void cacheAndSpawn(status, () => MirabufCachingService.cacheAPS(data, miraType))
}

// backs the "Download All" button, caches only and spawns nothing
export async function downloadAll(manifestAssets: DefaultAssetInfo[], cachedAssets: MirabufCacheInfo[]) {
    const cachedHashes = new Set(cachedAssets.map(info => info.hash))
    const toCache = manifestAssets.filter(asset => !cachedHashes.has(asset.hash))
    if (toCache.length === 0) return

    const status = new ProgressHandle("Caching Remote Assets")
    status.update(`Downloading... (0/${toCache.length})`, 0.05)

    let completeCount = 0
    const results = await Promise.all(
        toCache.map(async asset => {
            const cacheInfo = await cacheDefaultAsset(asset).catch(e => {
                console.error(e)
                return undefined
            })
            if (cacheInfo) {
                completeCount++
                status.update(`Downloading... (${completeCount}/${toCache.length})`, completeCount / toCache.length)
            }
            return cacheInfo
        })
    )

    const failedCount = results.filter(cacheInfo => !cacheInfo).length
    if (failedCount > 0) {
        status.fail(`Failed to cache ${failedCount} asset${failedCount === 1 ? "" : "s"}`)
    } else {
        status.done()
    }
}
