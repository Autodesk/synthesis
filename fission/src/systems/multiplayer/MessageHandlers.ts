import { globalAddToast } from "@/ui/components/GlobalUIControls"
import MatchMode from "../match_mode/MatchMode"
import {
    AssemblyRequestData,
    ClientInfo,
    EncodedAssembly,
    InitData,
    InitObjectData,
    MatchModeStateData,
    Message,
    MetadataUpdateData,
    ObjectPreferences,
    UpdateObjectData,
} from "./types"
import World from "../World"
import { COLLISION_TIMEOUT, MultiplayerStateEvent, MultiplayerStateEventType } from "./MultiplayerSystem"
import MirabufSceneObject, { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import { mirabuf } from "@/proto/mirabuf"
import JOLT from "@/util/loading/JoltSyncLoader"
import MirabufCachingService from "@/mirabuf/MirabufLoader"

export async function handleMatchModeState(data: MatchModeStateData) {
    console.log(data)
    if (data.event == "start") {
        MatchMode.getInstance().setMatchModeConfig(data.config)
        await MatchMode.getInstance().start(false)
    }
    if (data.event == "cancel") {
        MatchMode.getInstance().sandboxModeStart()
        globalAddToast("info", "Match Mode Cancelled")
    }
}

export function handlePeerInfo(data: ClientInfo) {
    World.multiplayerSystem?._clientToObjectMap.set(data.clientId, [])
    World.multiplayerSystem?._clientToInfoMap.set(data.clientId, data)
    MultiplayerStateEvent.dispatch(MultiplayerStateEventType.PEER_CHANGE)
}

export async function handleWorldInitialization(data: InitData) {
    World.physicsSystem = data.physicsSystem
    World.sceneRenderer.sceneObjects = await encodedAssemblyToSceneObjectMap(data.objects)
}

async function encodedAssemblyToSceneObjectMap(
    assemblies: EncodedAssembly[]
): Promise<Map<number, MirabufSceneObject>> {
    return new Map(
        await Promise.all(
            assemblies.map(async assembly => {
                const object = await createMirabuf(mirabuf.Assembly.decode(assembly))
                if (object == null) return

                World.sceneRenderer.registerSceneObject(object)

                return [object.id, object] as [number, MirabufSceneObject]
            })
        ).then(objects => objects.filter(n => n != null))
    )
}

export function handlePeerUpdate(data: UpdateObjectData[]) {
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

        // Set all the ejectables that are in activeEjectables but not gamePiecesControlled
        sceneObject.activeEjectables
            .filter(id => !gamePiecesControlled.includes(id.GetIndexAndSequenceNumber()))
            // We're not ejecting the actual game piece here, but the robots should be configured to eject in the same order so it's fine
            .forEach(_ => sceneObject.eject())

        // Set all the ejectables that are in gamePiecesControlled but not activeEjectables
        gamePiecesControlled
            .filter(id => !sceneObject.activeEjectables.map(n => n.GetIndexAndSequenceNumber()).includes(id))
            .forEach(id => {
                const bodyId = new JOLT.BodyID(id)
                return sceneObject.setEjectable(bodyId)
            })

        // Sets the physics data for each body in the assembly
        bodies.forEach(({ bodyId, linearVelocityStr, angularVelocityStr, positionStr, rotationStr }) => {
            const lin: { x: number; y: number; z: number } = JSON.parse(linearVelocityStr)
            const ang: { x: number; y: number; z: number } = JSON.parse(angularVelocityStr)
            const pos: { x: number; y: number; z: number } = JSON.parse(positionStr)
            const rot: { x: number; y: number; z: number; w: number } = JSON.parse(rotationStr)

            const linearVelocity = new JOLT.Vec3(lin.x, lin.y, lin.z)
            const angularVelocity = new JOLT.Vec3(ang.x, ang.y, ang.z)
            const position = new JOLT.RVec3(pos.x, pos.y, pos.z)
            const rotation = new JOLT.Quat(rot.x, rot.y, rot.z, rot.w)

            const joltBodyId = new JOLT.BodyID(bodyId)

            const clientBody = World.physicsSystem.getBody(joltBodyId)
            if (!clientBody) {
                console.error(`Body ${bodyId} on Scene Object ${sceneObject.assemblyName} not found`)
                return
            }

            clientBody.SetLinearVelocity(linearVelocity)
            clientBody.SetAngularVelocity(angularVelocity)
            World.physicsSystem.setBodyPosition(joltBodyId, position)
            World.physicsSystem.setBodyRotation(joltBodyId, rotation)
        })
    })
}

export function handleCollision(data: UpdateObjectData[]) {
    // TODO Expand on this logic
    if (World.multiplayerSystem?.lastSentCollisionTimestamp < COLLISION_TIMEOUT) return

    handlePeerUpdate(data)
}

export async function handleNewObject(data: InitObjectData, peerId: string) {
    let assembly: mirabuf.Assembly | undefined
    if (data.assembly) {
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
        await World.multiplayerSystem?.send(peerId, {
            type: "needAssembly",
            data: { assemblyHash: data.assemblyHash, sceneObjectKey: data.sceneObjectKey },
        })
        return
    }

    const object = await createMirabuf(assembly)
    if (object == null) return

    const clientToObjectMap = World.multiplayerSystem?._clientToObjectMap
    const clientToInfoMap = World.multiplayerSystem?._clientToInfoMap
    if (clientToInfoMap == null || clientToObjectMap == null) return

    object.setPreferenceData(data.initialPreferences)
    object.nameOverride =
        (clientToInfoMap.get(peerId)?.displayName ?? peerId) + " " + (clientToObjectMap.get(peerId)?.length ?? "0")

    console.log("Registering object", object, data)
    World.sceneRenderer.registerSceneObject(object, data.sceneObjectKey)

    clientToObjectMap.get(peerId)?.push(object.id) || clientToObjectMap.set(peerId, [object.id])
}

export async function handleAssemblyRequest(data: AssemblyRequestData, peerId: string) {
    const sceneObjectKey = data.sceneObjectKey

    const assembly = await MirabufCachingService.getEncoded(data.assemblyHash)
    if (!assembly) {
        console.error(`Failed to get assembly: ${data.assemblyHash} from cache`)
        return
    }
    const { buffer, info } = assembly

    const encodedAssembly = new Uint8Array(buffer) as EncodedAssembly

    const message: Message = {
        type: "newObject",
        data: {
            sceneObjectKey,
            assembly: encodedAssembly,
            assemblyHash: info.hash,
            miraType: info.miraType,
            initialPreferences: (
                World.sceneRenderer.sceneObjects.get(data.sceneObjectKey)! as MirabufSceneObject
            ).getPreferenceData(),
        },
    }

    await World.multiplayerSystem?.send(peerId, message)
}

export function handleDeleteObject(sceneObjectKey: number, _peerId: string) {
    const clientToObjectMap = World.multiplayerSystem?._clientToObjectMap
    if (clientToObjectMap == null) return

    const [peerId, _keys] = [...clientToObjectMap.entries()].find(([_id, keys]) => keys.includes(sceneObjectKey)) ?? [
        undefined,
        undefined,
    ]
    if (peerId) {
        const keys = clientToObjectMap.get(peerId)
        const index = keys?.indexOf(sceneObjectKey) ?? -1
        if (index != -1) {
            keys?.splice(index)
        }
    }

    const sceneObject = World.sceneRenderer.sceneObjects.get(sceneObjectKey)
    if (!sceneObject || !(sceneObject instanceof MirabufSceneObject)) return

    sceneObject.dispose()
    World.sceneRenderer.removeSceneObject(sceneObjectKey)
}

export function handleObjectConfiguration(data: ObjectPreferences) {
    const sceneObject = World.sceneRenderer.sceneObjects.get(data.sceneObjectKey) as MirabufSceneObject
    sceneObject.setPreferenceData(data.objectConfigurationData)
}

export function disableObjectPhysics(sceneObjectKey: number) {
    const sceneObject = World.sceneRenderer.sceneObjects.get(sceneObjectKey) as MirabufSceneObject
    sceneObject.disablePhysics()
}

export function enableObjectPhysics(sceneObjectKey: number) {
    const sceneObject = World.sceneRenderer.sceneObjects.get(sceneObjectKey) as MirabufSceneObject
    sceneObject.enablePhysics()
}

export function handleMetadataUpdate(data: MetadataUpdateData) {
    const sceneObject = World.sceneRenderer.sceneObjects.get(data.sceneObjectKey)
    if (!sceneObject || !(sceneObject instanceof MirabufSceneObject)) return

    sceneObject.multiplayerInfo = data
}
