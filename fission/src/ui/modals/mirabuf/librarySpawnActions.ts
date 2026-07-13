import type { Data } from "@/aps/APSDataManagement"
import type { DefaultAssetInfo } from "@/mirabuf/DefaultAssetLoader.ts"
import MirabufCachingService, { type MirabufCacheInfo, MiraType } from "@/mirabuf/MirabufLoader"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import { mirabuf } from "@/proto/mirabuf"
import type { EncodedAssembly, LocalSceneObjectId, Message, RemoteSceneObjectId } from "@/systems/multiplayer/types"
import { PAUSE_REF_ASSEMBLY_SPAWNING } from "@/systems/physics/PhysicsTypes"
import { getTargetControls } from "@/systems/scene/CameraControls"
import World from "@/systems/World"
import { globalOpenPanel } from "@/ui/components/GlobalUIControls"
import { ProgressHandle } from "@/ui/components/ProgressNotificationData"
import InitialConfigPanel from "@/ui/panels/configuring/initial-config/InitialConfigPanel"

/**
 * Spawn a mirabuf assembly that already lives in the cache. Shared by every
 * entry point of the asset Library (cached, remote, and APS spawns all funnel
 * through here once their buffer is cached).
 */
export async function spawnCachedMira(info: MirabufCacheInfo, progressHandle?: ProgressHandle) {
    // If spawning a field, then remove all other fields
    if (info.miraType === MiraType.FIELD) {
        World.sceneRenderer.removeAllFields()
    }

    if (!progressHandle) {
        progressHandle = new ProgressHandle(info.name)
    }

    World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_SPAWNING)
    await MirabufCachingService.get(info.hash)
        .then(async assembly => {
            if (assembly) {
                await createMirabuf(info.hash, assembly, progressHandle).then(async mirabufSceneObject => {
                    if (mirabufSceneObject) {
                        World.sceneRenderer.registerSceneObject(mirabufSceneObject)

                        const targetControls = getTargetControls()

                        if (World.multiplayerSystem != null) {
                            const encodedAssembly =
                                mirabufSceneObject.miraType !== MiraType.FIELD
                                    ? (mirabuf.Assembly.encode(assembly).finish() as EncodedAssembly)
                                    : undefined

                            const message: Message = {
                                type: "newObject",
                                timestamp: Date.now(),
                                data: {
                                    sceneObjectKey: mirabufSceneObject.id as RemoteSceneObjectId,
                                    assembly: encodedAssembly,
                                    assemblyHash: info.hash,
                                    miraType: info.miraType,
                                    initialPreferences: mirabufSceneObject.getPreferenceData(),
                                    bodyIds: mirabufSceneObject
                                        .getAllBodyIds()
                                        .map(id => id.GetIndexAndSequenceNumber()),
                                },
                            }
                            await World.multiplayerSystem?.broadcast(message)
                            World.multiplayerSystem?.registerOwnSceneObject(mirabufSceneObject.id as LocalSceneObjectId)
                        }

                        if (targetControls && (info.miraType === MiraType.ROBOT || !targetControls.focusProvider)) {
                            targetControls.focusProvider = mirabufSceneObject
                        }

                        progressHandle.done()

                        if (mirabufSceneObject.miraType == MiraType.ROBOT) {
                            globalOpenPanel(InitialConfigPanel, undefined)
                        }
                    } else {
                        progressHandle.fail("No object!")
                    }
                })
            } else {
                progressHandle.fail()
                console.error("Failed to spawn robot")
            }
        })
        .catch(e => {
            console.error(e)
            progressHandle.fail()
        })
        .finally(() => {
            setTimeout(() => World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_SPAWNING), 500)
        })
}

/**
 * Download a default (remote) asset into the cache, carrying its year/thumbnail
 * metadata, then spawn it. Fire-and-forget: progress is surfaced via ProgressHandle.
 */
export function spawnRemote(info: DefaultAssetInfo) {
    const status = new ProgressHandle(info.name)
    status.update("Downloading from Synthesis...", 0.05)

    MirabufCachingService.cacheRemote(info.remotePath, info.miraType, info.name, info.hash, info.year, info.thumbnail)
        .then(async cacheInfo => {
            if (cacheInfo) {
                await spawnCachedMira(cacheInfo, status)
            } else {
                status.fail("Failed to cache")
            }
        })
        .catch(e => {
            console.error(e)
            status.fail()
        })
}

/**
 * Cache an APS (Autodesk Hub) file, then spawn it.
 */
export function spawnAPS(data: Data, miraType: MiraType) {
    const status = new ProgressHandle(data.attributes.displayName ?? data.id)
    status.update("Downloading from APS...", 0.05)

    MirabufCachingService.cacheAPS(data, miraType)
        .then(async cacheInfo => {
            if (cacheInfo) {
                await spawnCachedMira(cacheInfo, status)
            } else {
                status.fail("Failed to cache")
            }
        })
        .catch(e => {
            console.error(e)
            status.fail()
        })
}

/**
 * Download every remote asset not yet cached (no spawn). Used by "Download All".
 */
export function downloadAll(manifestAssets: DefaultAssetInfo[], cachedAssets: MirabufCacheInfo[]) {
    const status = new ProgressHandle("Caching Remote Assets")

    const toCache = manifestAssets.filter(asset => !cachedAssets.some(info => info.hash === asset.hash))

    let completeCount = 0
    const totalCount = toCache.length
    status.update(`Downloading... (0/${totalCount})`, 0.05)

    toCache.forEach(asset => {
        MirabufCachingService.cacheRemote(
            asset.remotePath,
            asset.miraType,
            asset.name,
            asset.hash,
            asset.year,
            asset.thumbnail
        )
            .then(cacheInfo => {
                if (cacheInfo) {
                    completeCount++
                    if (completeCount == totalCount) {
                        status.done()
                    } else {
                        status.update(`Downloading... (${completeCount}/${totalCount})`, completeCount / totalCount)
                    }
                } else {
                    status.fail("Failed to cache")
                }
            })
            .catch(e => {
                console.error(e)
                status.fail()
            })
    })
}
