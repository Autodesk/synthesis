import { ProgressHandle } from "@/components/ProgressNotificationData.ts"
import MirabufCachingService from "@/mirabuf/MirabufLoader"
import MirabufSceneObject, { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import type { mirabuf } from "@/proto/mirabuf"
import ScoreTracker from "@/systems/match_mode/ScoreTracker"
import { globalAddToast } from "@/ui/components/GlobalUIControls"
import JOLT from "@/util/loading/JoltSyncLoader"
import MatchMode from "../match_mode/MatchMode"
import World from "../World"
import { MultiplayerStateEvent, MultiplayerStateEventType } from "./MultiplayerSystem"
import type {
    AssemblyRequestData,
    ClientInfo,
    EncodedAssembly,
    InitObjectData,
    LocalSceneObjectId,
    MatchModePenalty,
    MatchModeStateData,
    MessageType,
    ObjectPreferences,
    RemoteSceneObjectId,
    UpdateObjectData,
} from "./types"
import PreferencesSystem from "../preferences/PreferencesSystem"

export const peerMessageHandlers = {
    info: handlePeerInfo,
    update: handlePeerUpdate,
    collision: handleCollision,
    newObject: handleNewObject,
    needAssembly: handleAssemblyRequest,
    deleteObject: handleDeleteObject,
    configureObject: handleObjectConfiguration,
    disableObjectPhysics: disableObjectPhysics,
    enableObjectPhysics: enableObjectPhysics,
    matchModeState: handleMatchModeState,
    matchModePenalty: handleMatchModePenalty,
    ping: () => {
        console.warn("unhandled event")
    },
    pong: () => {
        console.warn("unhandled event")
    },
} as const satisfies {
    [K in keyof MessageType]: (data: MessageType[K], peerId: string, timestamp: number) => Promise<void> | void
}

const pendingOperations: (() => void)[] = []
const progressHandles: Map<number, ProgressHandle> = new Map()

async function handleMatchModeState(data: MatchModeStateData) {
    console.log(data)
    if (data.event == "start") {
        MatchMode.getInstance().setMatchModeConfig(data.config)
        await MatchMode.getInstance().start(false, data.moveRobots)
    }
    if (data.event == "cancel") {
        MatchMode.getInstance().sandboxModeStart()
        globalAddToast("info", "Match Mode Cancelled")
    }
}

function handlePeerInfo(data: ClientInfo) {
    World.multiplayerSystem?._clientToObjectMap.set(data.clientId, [])
    World.multiplayerSystem?._clientToInfoMap.set(data.clientId, data)
    globalAddToast("success", "Multiplayer Peer Connected", data.displayName)
    MultiplayerStateEvent.dispatch(MultiplayerStateEventType.PEER_CHANGE)
}
const clientToUpdateMap = new Map<string, number>()
function handlePeerUpdate(data: UpdateObjectData[], peerId: string, timestamp: number) {
    const bodyMap = World.multiplayerSystem?._clientToBodyMap.get(peerId)!

    const lastTimestamp = clientToUpdateMap.get(peerId)
    if (lastTimestamp != null && lastTimestamp > timestamp) {
        console.warn("ignoring old update")
        return
    }
    clientToUpdateMap.set(peerId, timestamp)

    data.forEach(({ sceneObjectKey, gamePiecesControlled, bodies }) => {
        const sceneObject = World.sceneRenderer.sceneObjects.get(
            World.multiplayerSystem!.convertSceneObjectId(peerId, sceneObjectKey)
        )
        if (sceneObject == null) {
            console.warn(
                `Multiplayer SceneObject: ${sceneObjectKey} not found in sceneObjects map. Multiplayer SceneObjects must be initialized before being updated.`
            )
            return
        } else if (!(sceneObject instanceof MirabufSceneObject)) {
            console.error(`Multiplayer SceneObject: ${sceneObjectKey} not MirabufSceneObject`)
            return
        }

        // Add all the ejectables that are in activeEjectables but not gamePiecesControlled
        sceneObject.activeEjectables
            .filter(id => !gamePiecesControlled.includes(id.GetIndexAndSequenceNumber()))
            // We're not ejecting the actual game piece here, but the robots should be configured to eject in the same order so it's fine
            .forEach(_ => sceneObject.eject())

        // Add all the ejectables that are in gamePiecesControlled but not activeEjectables
        gamePiecesControlled
            .filter(id => !sceneObject.activeEjectables.map(n => n.GetIndexAndSequenceNumber()).includes(id))
            .forEach(id => {
                const bodyId = new JOLT.BodyID(id)
                return sceneObject.setEjectable(bodyId)
            })

        // Sets the physics data for each body in the assembly
        bodies
            .map(({ bodyId, linearVelocityStr, angularVelocityStr, positionStr, rotationStr }) => {
                const newBodyId = bodyMap.get(bodyId)
                if (newBodyId == null) {
                    console.error(`BodyId: ${bodyId} sent by ${peerId} does not exist in bodyMap`)
                    return
                }
                return {
                    bodyId: newBodyId,
                    linearVelocityStr,
                    angularVelocityStr,
                    positionStr,
                    rotationStr,
                }
            })
            .filter(data => data != null)
            .forEach(({ bodyId, linearVelocityStr, angularVelocityStr, positionStr, rotationStr }) => {
                const lin: { x: number; y: number; z: number } = JSON.parse(linearVelocityStr)
                const ang: { x: number; y: number; z: number } = JSON.parse(angularVelocityStr)
                const pos: { x: number; y: number; z: number } = JSON.parse(positionStr)
                const rot: { x: number; y: number; z: number; w: number } = JSON.parse(rotationStr)

                const linearVelocity = new JOLT.Vec3(lin.x, lin.y, lin.z)
                const angularVelocity = new JOLT.Vec3(ang.x, ang.y, ang.z)
                const position = new JOLT.RVec3(pos.x, pos.y, pos.z)
                const rotation = new JOLT.Quat(rot.x, rot.y, rot.z, rot.w)

                const clientBody = World.physicsSystem.getBody(bodyId)
                if (!clientBody) {
                    console.error(`Body ${bodyId} on Scene Object ${sceneObject.assemblyName} not found`)
                    return
                }

                clientBody.SetLinearVelocity(linearVelocity)
                clientBody.SetAngularVelocity(angularVelocity)
                World.physicsSystem.setBodyPosition(bodyId, position)
                World.physicsSystem.setBodyRotation(bodyId, rotation)
            })
    })
}

function handleCollision() {
    return // TODO Expand on this logic
}

async function handleNewObject(data: InitObjectData, peerId: string) {
    const handle =
        progressHandles.get(data.sceneObjectKey) ??
        new ProgressHandle(
            "Asset from " + (World.multiplayerSystem?._clientToInfoMap.get(peerId)?.displayName ?? peerId)
        )
    handle.update("Finding Assembly", 0.05)
    progressHandles.set(data.sceneObjectKey, handle)
    let assembly: mirabuf.Assembly | undefined
    if (data.assembly) {
        handle.update("Loading Assembly", 0.2)
        const returnedInfo = await MirabufCachingService.cacheLocalAndReturn(
            data.assembly.buffer as ArrayBuffer,
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
        await World.multiplayerSystem?.send(peerId, {
            type: "needAssembly",
            data: { assemblyHash: data.assemblyHash, sceneObjectKey: data.sceneObjectKey },
        })
        return
    }

    const object = await createMirabuf(assembly, handle, peerId)
    if (object == null) return

    const clientToObjectMap = World.multiplayerSystem?._clientToObjectMap
    const clientToInfoMap = World.multiplayerSystem?._clientToInfoMap
    let bodyMap = World.multiplayerSystem?._clientToBodyMap.get(peerId)
    if (clientToInfoMap == null || clientToObjectMap == null) return
    // Initialize bodyMap for this peer if it doesn't exist
    if (bodyMap == null) {
        World.multiplayerSystem?._clientToBodyMap.set(peerId, new Map())
        bodyMap = World.multiplayerSystem?._clientToBodyMap.get(peerId)!
    }

    object.setPreferenceData(data.initialPreferences)
    object.nameOverride = clientToInfoMap.get(peerId)?.displayName ?? peerId

    console.log("Registering object", object, data)
    const localSceneObjectKey = World.sceneRenderer.registerSceneObject(object)
    console.log("linking object", data.sceneObjectKey, "->", localSceneObjectKey)

    World.multiplayerSystem?.setSceneObjectIdMapping(peerId, data.sceneObjectKey, localSceneObjectKey)

    clientToObjectMap.get(peerId)?.push(object.id as LocalSceneObjectId) ||
        clientToObjectMap.set(peerId, [object.id as LocalSceneObjectId])

    // Sets bodyMap
    const clientBodyIds = object.getAllBodyIds()
    console.assert(data.bodyIds.length === clientBodyIds.length)
    data.bodyIds.forEach((id, i) => bodyMap.set(id, clientBodyIds[i]))

    handle.done("Loaded")

    // Run all messages that arrived before the assembly fully spawned
    const len = pendingOperations.length
    pendingOperations.forEach(op => {
        op()
    })
    pendingOperations.splice(0, len)
}

async function handleAssemblyRequest(data: AssemblyRequestData, peerId: string) {
    const sceneObjectKey = data.sceneObjectKey

    const assembly = await MirabufCachingService.getEncoded(data.assemblyHash)
    if (!assembly) {
        console.error(`Failed to get assembly: ${data.assemblyHash} from cache`)
        return
    }
    const { buffer, info } = assembly

    const encodedAssembly = new Uint8Array(buffer) as EncodedAssembly

    const sceneObject = World.sceneRenderer.sceneObjects.get(data.sceneObjectKey)! as MirabufSceneObject
    await World.multiplayerSystem?.send(peerId, {
        type: "newObject",
        data: {
            sceneObjectKey,
            assembly: encodedAssembly,
            assemblyHash: info!.hash,
            miraType: info!.miraType,
            initialPreferences: sceneObject.getPreferenceData(),
            bodyIds: sceneObject.getAllBodyIds().map(id => id.GetIndexAndSequenceNumber()),
        },
    })
}

function handleDeleteObject(sceneObjectKey: RemoteSceneObjectId, peerId: string) {
    if (!World.multiplayerSystem) return
    const clientToObjectMap = World.multiplayerSystem._clientToObjectMap
    const localKey = World.multiplayerSystem!.convertSceneObjectId(peerId, sceneObjectKey)

    const peerClient = [...clientToObjectMap.entries()].find(([_id, keys]) => keys.includes(localKey))
    if (peerClient != null) {
        const keys = clientToObjectMap.get(peerClient[0])
        const index = keys?.indexOf(World.multiplayerSystem.convertSceneObjectId(peerId, sceneObjectKey)) ?? -1
        if (index != -1) {
            keys?.splice(index)
        }
    }

    if (!World.sceneRenderer.sceneObjects.has(localKey)) {
        pendingOperations.push(() => handleDeleteObject(sceneObjectKey, peerId))
    }

    World.sceneRenderer.removeSceneObject(localKey)
}

function handleObjectConfiguration(data: ObjectPreferences, peerId: string) {
    const sceneObject = World.sceneRenderer.sceneObjects.get(
        World.multiplayerSystem!.convertSceneObjectId(peerId, data.sceneObjectKey)
    )
    if (sceneObject instanceof MirabufSceneObject) {
        if (sceneObject.isOwnObject) {
            console.warn("received config for own object")
            return
        }
        sceneObject.setPreferenceData(data.objectConfigurationData)
        PreferencesSystem.savePreferences()
    } else {
        pendingOperations.push(() => handleObjectConfiguration(data, peerId))
    }
}

function disableObjectPhysics(sceneObjectKey: RemoteSceneObjectId, peerId: string) {
    const sceneObject = World.sceneRenderer.sceneObjects.get(
        World.multiplayerSystem!.convertSceneObjectId(peerId, sceneObjectKey)
    )
    if (sceneObject instanceof MirabufSceneObject) {
        if (sceneObject.isOwnObject) {
            console.warn("received disable for own object")
            return
        }
        sceneObject.disablePhysics()
    } else {
        pendingOperations.push(() => disableObjectPhysics(sceneObjectKey, peerId))
    }
}

function enableObjectPhysics(sceneObjectKey: RemoteSceneObjectId, peerId: string) {
    const sceneObject = World.sceneRenderer.sceneObjects.get(
        World.multiplayerSystem!.convertSceneObjectId(peerId, sceneObjectKey)
    )
    if (sceneObject instanceof MirabufSceneObject) {
        if (sceneObject.isOwnObject) {
            console.warn("received enablephysics for own object")
            return
        }
        sceneObject.enablePhysics()
    } else {
        pendingOperations.push(() => enableObjectPhysics(sceneObjectKey, peerId))
    }
}

function handleMatchModePenalty(data: MatchModePenalty, peerId: string) {
    const obj = World.sceneRenderer.sceneObjects.get(
        World.multiplayerSystem!.convertSceneObjectId(peerId, data.objectId)
    )
    if (!(obj instanceof MirabufSceneObject)) {
        console.warn("can't handle penalty for object", data.objectId, obj)
        pendingOperations.push(() => handleMatchModePenalty(data, peerId))
        return
    }
    ScoreTracker.robotPenalty(obj, data.points, data.description, false)
}
