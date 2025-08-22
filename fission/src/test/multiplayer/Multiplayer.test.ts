import { server } from "@vitest/browser/context"
import { afterEach, beforeAll, beforeEach, describe, expect, test, vi } from "vitest"
import MultiplayerSystem from "@/systems/multiplayer/MultiplayerSystem.ts"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem.ts"
import World from "@/systems/World.ts"

vi.spyOn(World, "initWorld").mockImplementation(async () => {
    console.log("tried to init world")
})
describe("Multiplayer Tests", () => {
    let multiplayer: MultiplayerSystem | undefined
    let roomId: string = "1000000"
    let altRoomId: string = "1000001"
    beforeAll(() => {
        vi.spyOn(World, "setMultiplayerSystem").mockImplementation(system => {
            multiplayer = system
        })
        vi.spyOn(console, "log").mockImplementation(() => {})
        vi.spyOn(console, "warn").mockImplementation(() => {})
        vi.spyOn(console, "info").mockImplementation(() => {})
        vi.spyOn(console, "debug").mockImplementation(() => {})
    })
    beforeEach(() => {
        vi.clearAllMocks()
        roomId = (parseInt(roomId) - 2).toString(10)
        altRoomId = (parseInt(altRoomId) - 2).toString(10)
    })
    afterEach(() => {
        multiplayer?.destroy()
        multiplayer = undefined
        PreferencesSystem.setGlobalPreference("MultiplayerClientID", "")
    })

    test("Multiplayer system connects to server", async () => {
        const success = await MultiplayerSystem.setup(roomId, "User", true)
        expect(success).toBe(true)
        expect(multiplayer).toBeDefined()
        expect(multiplayer?.roomId).toBe(roomId)
    })
    test("Can't join empty room", async () => {
        const success = await MultiplayerSystem.setup(roomId, "User", false)
        expect(success).toBe(false)
        expect(multiplayer).not.toBeDefined()
    })

    describe.skipIf(server.browser == "firefox")("P2P connections", async () => {
        test("Multiplayer clients connect to each other", async () => {
            await MultiplayerSystem.setup(roomId, "User1", true)
            expect(multiplayer).toBeDefined()
            const player1 = multiplayer!

            PreferencesSystem.setGlobalPreference("MultiplayerClientID", "")
            await MultiplayerSystem.setup(roomId, "User2", false)
            expect(multiplayer).toBeDefined()
            const player2 = multiplayer!

            expect(player1.roomId).toBe(player2.roomId)
            await vi.waitUntil(() => player1.peerIDs.length > 0 && player2.peerIDs.length > 0)
            expect(player1.peerIDs).toStrictEqual([player2.clientId])
            expect(player2.peerIDs).toStrictEqual([player1.clientId])
        })

        test("Multiplayer clients check authentication", async () => {
            await MultiplayerSystem.setup(roomId, "User1", true)
            expect(multiplayer).toBeDefined()
            const player1 = multiplayer!

            PreferencesSystem.setGlobalPreference("MultiplayerClientID", "")
            await MultiplayerSystem.setup(altRoomId, "User2", true)
            expect(multiplayer).toBeDefined()
            const player2 = multiplayer!

            const connectionSpy = vi.fn()
            const acceptedConnectionSpy = vi.spyOn(player1, "setupConnectionHandlers")
            player1.getClient().on("connection", connectionSpy)

            player2.getClient().connect(player1.clientId, {
                metadata: {
                    authHash: "invalid",
                },
            })

            await vi.waitUntil(() => connectionSpy.mock.calls.length > 0)

            expect(acceptedConnectionSpy).not.toHaveBeenCalled()

            expect(player1.peerIDs).toStrictEqual([])
            expect(player2.peerIDs).toStrictEqual([])
        })
    })
})
