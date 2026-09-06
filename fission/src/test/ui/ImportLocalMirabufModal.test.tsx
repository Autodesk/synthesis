import { fireEvent, render, waitFor } from "@testing-library/react"
import { act } from "react"
import { beforeEach, describe, expect, test, vi } from "vitest"
import EventSystem from "@/systems/EventSystem"
import { ProgressHandleStatus } from "@/components/ProgressNotificationData"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject"
import ImportLocalMirabufModal from "@/ui/modals/mirabuf/ImportLocalMirabufModal"
import { type Modal, type UIContextProps, UIContext } from "@/ui/helpers/UIProviderHelpers"

vi.mock("@/mirabuf/MirabufLoader", () => ({
    default: {
        cacheLocalAndReturn: vi.fn(),
    },
    MiraType: {
        ROBOT: 1,
        FIELD: 2,
    },
}))

vi.mock("@/mirabuf/MirabufSceneObject", () => ({
    createMirabuf: vi.fn(),
}))

vi.mock("@/mirabuf/MirabufThumbnail", () => ({
    embedAssemblyThumbnail: vi.fn(() => Promise.resolve()),
}))

vi.mock("@/systems/World", () => ({
    default: {
        analyticsSystem: undefined,
        physicsSystem: {
            holdPause: vi.fn(),
            releasePause: vi.fn(),
        },
        sceneRenderer: {
            registerSceneObject: vi.fn(),
        },
    },
}))

vi.mock("@/systems/scene/CameraControls", () => ({
    getTargetControls: vi.fn(() => undefined),
}))

vi.mock("@/components/GlobalUIControls.ts", () => ({
    globalAddToast: vi.fn(),
    globalCloseModal: vi.fn(),
    globalOpenModal: vi.fn(),
    globalOpenPanel: vi.fn(),
}))

vi.mock("@/ui/modals/mirabuf/LibraryModal", () => ({
    default: () => null,
}))

vi.mock("@/ui/panels/configuring/initial-config/InitialConfigPanel", () => ({
    default: () => null,
}))

vi.mock("@/urdf/URDFLoader", () => ({
    loadURDF: vi.fn(),
}))

vi.mock("uuid", () => ({
    v4: vi.fn(),
}))

const cacheLocalAndReturn = vi.mocked(MirabufCachingService.cacheLocalAndReturn)
const createMirabufMock = vi.mocked(createMirabuf)

function makeModal() {
    return {
        props: {
            custom: { configurationType: "ROBOTS" },
            configured: true,
            type: "modal",
        },
    } as unknown as Modal<void, { configurationType: "ROBOTS" }>
}

describe("ImportLocalMirabufModal", () => {
    beforeEach(() => {
        vi.clearAllMocks()
        cacheLocalAndReturn.mockResolvedValue({
            assembly: {} as never,
            cacheInfo: { hash: "native-mira-hash" } as never,
        })
        createMirabufMock.mockResolvedValue({ miraType: MiraType.ROBOT } as never)
    })

    test("completes the progress notification after importing a native Mira file", async () => {
        let accept: (() => void | Promise<unknown>) | undefined
        const configureScreen: UIContextProps["configureScreen"] = (_screen, _props, callbacks) => {
            accept = callbacks.onBeforeAccept as (() => void | Promise<unknown>) | undefined
        }
        const uiContext: UIContextProps = {
            panels: [],
            blockState: { blocked: false },
            openModal: vi.fn(),
            openPanel: vi.fn(),
            togglePanel: vi.fn(),
            closeModal: vi.fn(),
            closePanel: vi.fn(),
            addToast: vi.fn(),
            configureScreen,
        }

        const { container } = render(
            <UIContext.Provider value={uiContext}>
                <ImportLocalMirabufModal modal={makeModal()} />
            </UIContext.Provider>
        )

        const input = container.querySelector('input[type="file"]')
        expect(input).not.toBeNull()
        fireEvent.change(input!, {
            target: { files: [new File(["native mira"], "fixture.mira")] },
        })

        await waitFor(() => expect(accept).toBeDefined())
        const progressEvents: { status: ProgressHandleStatus; message: string }[] = []
        const unsubscribe = EventSystem.listen("ProgressEvent", handle => {
            progressEvents.push({ status: handle.status, message: handle.message })
        })

        await act(async () => {
            await accept!()
        })
        unsubscribe()

        expect(progressEvents.at(-1)).toEqual({
            status: ProgressHandleStatus.DONE,
            message: "Import complete!",
        })
    })
})
