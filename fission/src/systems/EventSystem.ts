import type Jolt from "@synthesis.adsk/jolt-physics"
import type { Data } from "@/aps/APSDataManagement.ts"
import type { ContextData } from "@/components/ContextMenuData.ts"
import type { ProgressHandle } from "@/components/ProgressNotificationData.ts"
import type { SceneOverlayTag } from "@/components/overlays/SceneOverlayEvents.ts"
import type { MatchModeType } from "@/systems/match_mode/MatchModeTypes.ts"
import type { CurrentContactData, OnContactValidateData } from "@/systems/physics/ContactEvents.ts"
import type TaskStatus from "@/util/TaskStatus.ts"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject.ts"
import type { CameraPoint } from "@/systems/preferences/PreferenceTypes.ts"
import type { SceneObjectId } from "@/systems/scene/SceneRenderer.ts"

interface EventDataMap {
    // Mirabuf
    ProgressEvent: ProgressHandle
    MirabufObjectChangeEvent: MirabufSceneObject | null
    SpawnPendingChangeEvent: boolean

    // APS
    MirabufFilesUpdateEvent: Data[]
    MirabufFilesStatusUpdateEvent: TaskStatus

    // Physics
    OnContactAddedEvent: CurrentContactData
    OnContactPersistedEvent: CurrentContactData
    OnContactValidateEvent: OnContactValidateData
    OnContactRemovedEvent: { message: Jolt.SubShapeIDPair }

    // Scene Overlay Tags
    SceneOverlayTagAddEvent: SceneOverlayTag
    SceneOverlayTagRemoveEvent: SceneOverlayTag
    SceneOverlayUpdateEvent: never

    ConfigurationSavedEvent: never
    InputSchemeChanged: { panelId?: string }

    TourRestartEvent: never

    // Match Mode
    ScoreChangedEvent: { red: number; blue: number }
    TimeChangedEvent: { time: number }
    MatchStateChangedEvent: { mode: MatchModeType }

    // Code Sim
    SimMapUpdateEvent: { internalUpdate: boolean }
    RobotCamerasChangeEvent: never

    // Context Menu
    ContextSupplierEvent: { data: ContextData; mousePosition: [number, number] }

    // Touch Controls
    SetPlaceAssetButtonVisibleEvent: boolean
    ToggleTouchControlsVisibilityEvent: never
    SetTouchControlsVisibilityEvent: boolean
    TouchControlsVisibilityChangedEvent: { visible: boolean }

    SetDragModeEvent: { enabled: boolean }

    CameraModeChangedEvent: { mode: string }
    CameraFocusChangedEvent: { focusProvider: MirabufSceneObject | undefined }
    // Field View: the active camera point changed (the selected point, or undefined when none).
    CameraViewChangedEvent: { point: CameraPoint | undefined; focusedRobotId?: SceneObjectId }
    // The active camera control scheme changed (e.g. "Target" or "FieldView").
    CameraControlsTypeChangedEvent: { controlsType: string }

    APSUserInfoUpdate: never

    MultiplayerStateJoinRoom: never
    MultiplayerStatePeerChange: never
}

type EventKey = keyof EventDataMap
type EventKeyWithValue = {
    [K in EventKey]: EventDataMap[K] extends never ? never : K
}[EventKey]
type EventKeyWithoutValue = Exclude<EventKey, EventKeyWithValue>

class CustomEvent<K extends EventKey, T extends EventDataMap[K]> extends Event {
    public readonly data: T
    public override readonly type: K
    public constructor(event: K, data: T) {
        super(event)
        this.type = event
        this.data = data
    }

    public dispatch() {
        window.dispatchEvent(this)
    }
}

export type SynthesisEvent<K extends EventKey> = CustomEvent<K, EventDataMap[K]>
export type SynthesisEventData<K extends EventKey> = EventDataMap[K]
export type SynthesisEventListener<K extends EventKey> = (data: EventDataMap[K]) => void

class EventSystem {
    public static dispatch<K extends EventKeyWithoutValue>(key: K): void
    public static dispatch<K extends EventKeyWithValue>(key: K, data: SynthesisEventData<K>): void
    public static dispatch<K extends EventKey, T extends EventDataMap[K]>(key: K, data?: T): void {
        const event = new CustomEvent(key, data as T)
        event.dispatch()
    }

    public static create<K extends EventKeyWithoutValue>(key: K): SynthesisEvent<K>
    public static create<K extends EventKeyWithValue>(key: K, data: SynthesisEventData<K>): SynthesisEvent<K>
    public static create<K extends EventKey, T extends EventDataMap[K]>(key: K, data?: T): SynthesisEvent<K> {
        return new CustomEvent(key, data as T)
    }

    public static listen<K extends EventKey>(key: K, listener: SynthesisEventListener<K>) {
        const cb = (event: Event) => {
            if (!(event instanceof CustomEvent)) {
                console.warn("Incorrect event type dispatched", event, key)
                return
            }
            listener(event.data)
        }
        window.addEventListener(key, cb)
        return () => {
            window.removeEventListener(key, cb)
        }
    }
}
export default EventSystem
