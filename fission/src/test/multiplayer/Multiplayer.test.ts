import { afterAll, afterEach, beforeAll, describe, expect, test, vi } from "vitest"
import MultiplayerSystem from "@/systems/multiplayer/MultiplayerSystem.ts"
import World from "@/systems/World.ts"
import { mockConsole } from "@/test/mocks/Common.ts"
import MultiplayerWebsocket from "@/systems/multiplayer/MultiplayerWebsocket.ts"

const HOST = "wss://localhost:2610/"

vi.mock("@/systems/scene/SceneRenderer.ts", () => {
    return {
        default: vi.fn().mockReturnValue({
            removeAllSceneObjects: vi.fn(),
            mirabufSceneObjects: {
                getAll: () => [],
                getField: () => undefined,
            },
        }),
    }
})

describe.runIf(import.meta.env.VITE_RUN_MULTIPLAYER_TEST)("Multiplayer Tests", () => {
    let multiplayer: MultiplayerSystem | undefined
    beforeAll(() => {
        World.initWorld()
        vi.spyOn(World, "setMultiplayerSystem").mockImplementation(system => {
            multiplayer = system
        })
        mockConsole()
    })

    afterEach(() => {
        multiplayer?.destroy()
        multiplayer = undefined
    })

    afterAll(() => {
        vi.restoreAllMocks()
    })

    test("Multiplayer system connects to server", async () => {
        await expect(setUpClient(null, "User")).resolves.toBeDefined()
        expect(multiplayer).toBeDefined()
        expect(multiplayer?.roomId).toBeDefined()
    })

    test("Multiplayer clients connect to each other", async () => {
        const ws = new MultiplayerWebsocket(HOST)
        await new Promise<void>((resolve, reject) => {
            ws.onOpen = () => {
                resolve()
            }
            ws.onClose = () => {
                reject("closed")
            }
        })
        expect(ws.ready).toBe(true)

        await MultiplayerSystem.setup(ws.init(null, "User"), "User", true)

        expect(multiplayer).toBeDefined()
        expect(multiplayer?.roomId).toBeDefined()
    })
})

async function setUpClient(roomId: null | string, name: string) {
    const ws = new MultiplayerWebsocket(HOST)
    await new Promise<void>((resolve, reject) => {
        ws.onOpen = () => {
            resolve()
        }
        ws.onClose = () => {
            reject("closed")
        }
    })
    expect(ws.ready).toBe(true)

    const success = await MultiplayerSystem.setup(ws.init(roomId, name), name, false)
    expect(success).toBe(true)
    return ws
}
