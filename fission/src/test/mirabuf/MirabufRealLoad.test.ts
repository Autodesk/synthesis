import { beforeAll, beforeEach, describe, expect, test, vi } from "vitest"
import MirabufLoader, { MiraType } from "../../mirabuf/MirabufLoader"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject.ts"
import World from "@/systems/World.ts"
import { server } from "@vitest/browser/context"

// Skip on firefox due to WebGL bug in github actions
describe.skipIf(server.browser == "firefox")("Real Load Assets", () => {
    beforeAll(async () => {
        await World.initWorld()
        vi.spyOn(World.analyticsSystem!, "event").mockReturnValue()
        vi.spyOn(World.analyticsSystem!, "exception").mockReturnValue()
        vi.spyOn(console, "debug").mockReturnValue()
        vi.spyOn(console, "warn").mockReturnValue()
        vi.spyOn(console, "log").mockReturnValue()
    })
    beforeEach(async () => {
        await MirabufLoader.removeAll()
    })

    const tests: [string, MiraType, string][] = [
        ["/api/mira/robots/Dozer v11.mira", MiraType.ROBOT, "Dozer"],
        ["/api/mira/private/Multi-Joint Wheels v0.mira", MiraType.ROBOT, "Multi-Joint Wheels"],
        ["/api/mira/fields/FRC Field 2023 v8.mira", MiraType.FIELD, "2023 Field"],
    ]
    test.for(tests)("Loads $2", { timeout: 20000 }, async ([url, miratype]) => {
        const info = await MirabufLoader.cacheRemote(url, miratype)
        expect(info).toBeDefined()
        const assembly = await MirabufLoader.get(info!.hash)
        expect(assembly).toBeDefined()
        const sceneObject = await createMirabuf(info!.hash, assembly!)
        expect(sceneObject).toBeDefined()
        expect(sceneObject?.miraType).toBe(miratype)
    })
})
