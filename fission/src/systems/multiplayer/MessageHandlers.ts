import { ProgressHandle } from "@/components/ProgressNotificationData.ts"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import MirabufSceneObject, { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import type { mirabuf } from "@/proto/mirabuf"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import JOLT from "@/util/loading/JoltSyncLoader"
import MatchMode from "../match_mode/MatchMode"
import World from "../World"
import type {
    ConfigureObjectBody,
    InfoBody,
    LatencyInfoBody,
    MatchModePenaltyBody,
    MatchModeStateBody,
    MessageType,
    NeedAssemblyBody,
    NewObjectBody,
    UpdateObjectData,
    UpdatePhysicsBodyData,
} from "@/systems/multiplayer/MultiplayerMessageTypes.ts"
import type MultiplayerSystem from "@/systems/multiplayer/MultiplayerSystem.ts"
import { multiplayerLogger as console } from "@/systems/multiplayer/MultiplayerSystem.ts"
import type { SceneObjectId } from "@/systems/scene/SceneRenderer.ts"
import { isDefined } from "@/util/Utility"
import EventSystem from "@/systems/EventSystem.ts"
import type { EncodedAssembly } from "./MultiplayerTypes"
import { applyPhysicsBodyData, handleUpdateObjectPhysics } from "./UpdatePhysicsData"

export const peerMessageHandlers = {
    info: handleInfoMessage,
    update: handleUpdateMessage,
    updatePhysicsBody: handleUpdatePhysicsBody,
    collision: handleCollisionMessage,
    newObject: handleNewObjectMessage,
    needAssembly: handleNeedAssemblyMessage,
    deleteObject: handleDeleteObjectMessage,
    configureObject: handleConfigureObjectMessage,
    disableObjectPhysics: handleDisableObjectPhysicsMessage,
    enableObjectPhysics: handleEnableObjectPhysicsMessage,
    matchModeState: handleMatchModeStateMessage,
    matchModePenalty: handleMatchModePenaltyMessage,
    latencyInfo: handleLatencyInfoMessage,
} as const satisfies {
    [K in keyof MessageType]: (data: MessageType[K], peerId: string, timestamp: number) => Promise<void> | void
}

const pendingOperations: (() => void)[] = []
const progressHandles: Map<SceneObjectId, ProgressHandle> = new Map()

async function handleMatchModeStateMessage(data: MatchModeStateBody) {
    const matchMode = MatchMode.getInstance()
    switch (data.event) {
        case "start": {
            matchMode.setMatchModeConfig(data.config)
            await matchMode.start(World.multiplayerSystem!.fromServerTime(data.startTime), false, data.moveRobots)
            break
        }
        case "ongoing": {
            matchMode.setMatchModeConfig(data.config)
            // - No need to move the robots, since if the client just joined an ongoing match
            // then they've been / will be sent the current transform of every robot already
            // - The mode will automatically be set by the match mode system
            await matchMode.start(World.multiplayerSystem!.fromServerTime(data.startTime), false, false)
            break
        }
        case "cancel": {
            matchMode.sandboxModeStart()
            globalAddToast("info", "Match Mode Cancelled")
            break
        }
    }
}

async function handleInfoMessage(this: MultiplayerSystem, { info, introduceSelf }: InfoBody) {
    this.clientToObjectMap.set(info.clientId, [])
    this.clientToInfoMap.set(info.clientId, info)

    if (introduceSelf) await this.introduceSelf(false, info.clientId)
    if (this.isHost) this.sendOngoingMatchModeInfo()

    globalAddToast("success", "Multiplayer Peer Connected", info.displayName)
    EventSystem.dispatch("MultiplayerStatePeerChange")
}

const clientToUpdateMap = new Map<string, number>()

function handleUpdateMessage(data: UpdateObjectData[], peerId: string, timestamp: number) {
    // const bodyMap = World.multiplayerSystem?.clientToBodyMap.get(peerId)!

    const lastTimestamp = clientToUpdateMap.get(peerId)
    if (lastTimestamp != null && lastTimestamp > timestamp) {
        console.warn("ignoring old update")
        return
    }
    clientToUpdateMap.set(peerId, timestamp)

    data.forEach(({ sceneObjectKey, gamePiecesControlled, bodies }) => {
        const sceneObject = World.sceneRenderer.sceneObjects.get(sceneObjectKey)
        if (sceneObject == null) {
            console.warn(
                `Multiplayer SceneObject: ${sceneObjectKey} not found in sceneObjects map. Multiplayer SceneObjects must be initialized before being updated.`
            )
            return
        } else if (!(sceneObject instanceof MirabufSceneObject)) {
            console.error(`Multiplayer SceneObject: ${sceneObjectKey} not MirabufSceneObject`)
            return
        }

        const fieldSceneObject = World.sceneRenderer.mirabufSceneObjects.getField()
        if (fieldSceneObject) {
            // Add all the ejectables that are in activeEjectables but not gamePiecesControlled
            const gamePiecesControlledBodies = gamePiecesControlled
                .map(rnId => fieldSceneObject.mechanism.getBodyByNodeId(rnId)?.GetIndexAndSequenceNumber())
                .filter(isDefined)

            const activeEjectables = sceneObject.activeEjectables.map(id => id.GetIndexAndSequenceNumber())

            activeEjectables
                .filter(idx => !gamePiecesControlledBodies.includes(idx))
                // We're not ejecting the specific game piece here, but the robots should be configured to eject in the same order so it's fine
                .forEach(_ => sceneObject.eject())

            // Add all the ejectables that are in gamePiecesControlled but not activeEjectables
            gamePiecesControlledBodies
                .filter(idx => !activeEjectables.includes(idx))
                .forEach(idx => {
                    const bodyId = new JOLT.BodyID(idx)
                    return sceneObject.setEjectable(bodyId)
                })
        }

        handleUpdateObjectPhysics(sceneObject, bodies, peerId)
    })
}

function handleUpdatePhysicsBody(data: UpdatePhysicsBodyData, peerId: string, _timestamp: number) {
    // We only want to send it through the mapping if it's not a game piece we own

    const sceneObject = World.sceneRenderer.sceneObjects.get(data.sceneObjectId) as MirabufSceneObject
    // undefined
    console.log(`SceneObject ${sceneObject}`)
    console.log(`mechanism ${sceneObject.mechanism}`)
    const bodyId = sceneObject.mechanism.getBodyByNodeId(data.rigidNodeId)
    if (bodyId == null) {
        console.error(`BodyId: ${bodyId} sent by ${peerId} does not exist in bodyMap`)
        return
    }

    applyPhysicsBodyData(bodyId, data)
}

function handleCollisionMessage() {
    return // TODO Expand on this logic
}

async function handleNewObjectMessage(data: NewObjectBody, peerId: string, ts: number) {
    if (data.miraType == MiraType.FIELD && World.multiplayerSystem?.fieldTransferLock != null) {
        if (World.multiplayerSystem.fieldTransferLock.ts < ts) {
            console.warn("Ignoring assembly", { peerId, sceneObjectKey: data.sceneObjectId })
            return
        } else {
            const object = World.multiplayerSystem.fieldTransferLock.id
            const deleteObject = () => {
                if (!World.sceneRenderer.sceneObjects.has(object)) {
                    pendingOperations.push(() => deleteObject())
                } else {
                    World.sceneRenderer.removeSceneObject(object)
                }
            }
            deleteObject()
        }
    }

    const handle =
        progressHandles.get(data.sceneObjectId) ??
        new ProgressHandle(
            "Asset from " + (World.multiplayerSystem?.clientToInfoMap.get(peerId)?.displayName ?? peerId)
        )
    handle.update("Finding Assembly", 0.05)
    progressHandles.set(data.sceneObjectId, handle)

    let assembly: mirabuf.Assembly | undefined
    if (data.assembly) {
        handle.update("Loading Assembly", 0.2)
        const returnedInfo = await MirabufCachingService.cacheLocalAndReturn(
            data.assembly.buffer.slice(
                data.assembly.byteOffset,
                data.assembly.byteOffset + data.assembly.byteLength
            ) as ArrayBuffer,
            data.miraType
        )
        if (!returnedInfo) {
            console.warn("nothing returned from caching function")
            return
        }
        assembly = returnedInfo?.assembly
    } else {
        assembly = await MirabufCachingService.get(data.assemblyHash)
    }

    if (!assembly) {
        console.log("needAssembly")
        handle.update("Requesting Assembly", 0.05)
        World.multiplayerSystem?.send(
            {
                type: "needAssembly",
                data: { assemblyHash: data.assemblyHash, sceneObjectId: data.sceneObjectId },
            },
            peerId
        )
        return
    }

    const object = await createMirabuf(data.assemblyHash, assembly, handle, peerId)
    if (object == null) return

    const clientToObjectMap = World.multiplayerSystem?.clientToObjectMap
    const clientToInfoMap = World.multiplayerSystem?.clientToInfoMap
    if (clientToInfoMap == null || clientToObjectMap == null) return

    // Use the same UUID as the peer
    object.id = data.sceneObjectId
    object.setPreferenceData(data.initialPreferences)
    object.nameOverride = clientToInfoMap.get(peerId)?.displayName ?? peerId

    console.log("Registering object", object, data)
    World.sceneRenderer.registerSceneObject(object, data.sceneObjectId)

    clientToObjectMap.get(peerId)?.push(object.id) || clientToObjectMap.set(peerId, [object.id])

    // Update all bodies to their present configuration
    if (data.initialPhysicsData) {
        console.log("initialPhysics:" + JSON.stringify(data.initialPhysicsData))

        handleUpdateObjectPhysics(object, data.initialPhysicsData, peerId)
    }

    handle.done("Loaded")
    // Run all messages that arrived before the assembly fully spawned
    const len = pendingOperations.length
    pendingOperations.forEach(op => {
        op()
    })
    pendingOperations.splice(0, len)
}

async function handleNeedAssemblyMessage(data: NeedAssemblyBody, peerId: string) {
    const sceneObjectKey = data.sceneObjectId

    const assembly = await MirabufCachingService.getEncoded(data.assemblyHash)
    if (!assembly) {
        console.error(`Failed to get assembly: ${data.assemblyHash} from cache`)
        return
    }
    const { buffer, info } = assembly

    const encodedAssembly = new Uint8Array(buffer) as EncodedAssembly

    const sceneObject = World.sceneRenderer.sceneObjects.get(sceneObjectKey)! as MirabufSceneObject
    World.multiplayerSystem?.send(
        {
            type: "newObject",
            data: {
                sceneObjectId: sceneObjectKey,
                assembly: encodedAssembly,
                assemblyHash: info!.hash,
                miraType: info!.miraType,
                initialPreferences: sceneObject.getPreferenceData(),
            },
        },
        peerId
    )
}

export function handleDeleteObjectMessage(sceneObjectKey: SceneObjectId, peerId: string) {
    if (!World.multiplayerSystem) return
    const clientToObjectMap = World.multiplayerSystem.clientToObjectMap

    const peerClient = [...clientToObjectMap.entries()].find(([_id, keys]) => keys.includes(sceneObjectKey))
    if (peerClient != null) {
        const keys = clientToObjectMap.get(peerClient[0])
        const index = keys?.indexOf(sceneObjectKey) ?? -1
        if (index != -1) {
            keys?.splice(index)
        }
    }

    if (!World.sceneRenderer.sceneObjects.has(sceneObjectKey)) {
        pendingOperations.push(() => handleDeleteObjectMessage(sceneObjectKey, peerId))
    }

    World.sceneRenderer.removeSceneObject(sceneObjectKey)
}

function handleConfigureObjectMessage(data: ConfigureObjectBody, peerId: string) {
    const sceneObject = World.sceneRenderer.sceneObjects.get(data.sceneObjectId)
    if (sceneObject instanceof MirabufSceneObject) {
        if (sceneObject.isOwnObject) {
            console.warn("received config for own object")
            return
        }
        sceneObject.setPreferenceData(data.objectConfigurationData)
        sceneObject.savePreferences()
    } else {
        pendingOperations.push(() => handleConfigureObjectMessage(data, peerId))
    }
}

function handleDisableObjectPhysicsMessage(sceneObjectKey: SceneObjectId, peerId: string) {
    const sceneObject = World.sceneRenderer.sceneObjects.get(sceneObjectKey)
    if (sceneObject instanceof MirabufSceneObject) {
        if (sceneObject.isOwnObject) {
            console.warn("received disable for own object")
            return
        }
        sceneObject.disablePhysics()
    } else {
        pendingOperations.push(() => handleDisableObjectPhysicsMessage(sceneObjectKey, peerId))
    }
}

function handleEnableObjectPhysicsMessage(sceneObjectKey: SceneObjectId, peerId: string) {
    const sceneObject = World.sceneRenderer.sceneObjects.get(sceneObjectKey)
    if (sceneObject instanceof MirabufSceneObject) {
        if (sceneObject.isOwnObject) {
            console.warn("received enablephysics for own object")
            return
        }
        sceneObject.enablePhysics()
    } else {
        pendingOperations.push(() => handleEnableObjectPhysicsMessage(sceneObjectKey, peerId))
    }
}

function handleMatchModePenaltyMessage(data: MatchModePenaltyBody, peerId: string) {
    const obj = World.sceneRenderer.sceneObjects.get(data.objectId)
    if (!(obj instanceof MirabufSceneObject)) {
        console.warn("Can't handle penalty for object", data.objectId, obj)
        pendingOperations.push(() => handleMatchModePenaltyMessage(data, peerId))
        return
    }

    World.scoreTracker.robotPenalty(obj, data.points, data.description, false)
}

function handleLatencyInfoMessage(data: LatencyInfoBody, peerId: string, timestamp: number) {
    const entry = World.multiplayerSystem?.clientToInfoMap.get(peerId)
    if (!entry) return
    entry.lastUpdateTime = timestamp
    entry.latency = data.latencyMS
}
