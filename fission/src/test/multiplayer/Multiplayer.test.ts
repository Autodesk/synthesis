import { afterEach, beforeAll, beforeEach, describe, expect, test, vi, afterAll } from "vitest"
import MultiplayerSystem from "@/systems/multiplayer/MultiplayerSystem.ts"
import World from "@/systems/World.ts"
import { mockConsole } from "@/test/mocks/Common.ts"
import MultiplayerWebsocket from "@/systems/multiplayer/MultiplayerWebsocket.ts"

const HOST = "wss://localhost:2610/"

vi.mock("@/systems/scene/SceneRenderer.ts", () => {
    return {
        default: vi.fn(),
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
    beforeEach(() => {})
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
            ws.onClose = err => {
                reject(err)
            }
        })
        expect(ws.ready).toBe(true)

        await MultiplayerSystem.setup(MultiplayerWebsocket.init(null, "User", ws), "User")

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
        ws.onClose = err => {
            reject(err)
        }
    })
    expect(ws.ready).toBe(true)

    const success = await MultiplayerSystem.setup(MultiplayerWebsocket.init(roomId, name, ws), name)
    expect(success).toBe(true)
    return ws
}
