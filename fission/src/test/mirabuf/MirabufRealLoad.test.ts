import {beforeAll, afterAll, beforeEach, describe, expect, test, vi} from "vitest"
import MirabufLoader, { MiraType } from "../../mirabuf/MirabufLoader"
import { createMirabuf } from "@/mirabuf/MirabufSceneObject.ts"
import World from "@/systems/World.ts"
import { server } from "@vitest/browser/context"
import { FIELD_MODELS, ROBOT_MODELS } from "@/test/GetAssets.ts"
import { mockConsole } from "@/test/mocks/Common.ts"


// Skip on firefox due to WebGL bug in github actions
describe.skipIf(server.browser == "firefox")("Real Load Assets", () => {
    beforeAll(async () => {
        await World.initWorld()
        vi.spyOn(World.analyticsSystem!, "event").mockReturnValue()
        vi.spyOn(World.analyticsSystem!, "exception").mockReturnValue()
        mockConsole()

    })
    beforeEach(async () => {
        await MirabufLoader.removeAll()
    })

    afterAll(async () => {
        vi.restoreAllMocks()
    })

    const tests: [string, MiraType, string][] = [
        [ROBOT_MODELS.DOZER, MiraType.ROBOT, "Dozer"],
        [ROBOT_MODELS.MULTI_JOINT, MiraType.ROBOT, "Multi-Joint Wheels"],
        [FIELD_MODELS[2023], MiraType.FIELD, "2023 Field"],
    ]
    test.for(tests)("Loads $2", {timeout: 20000}, async ([url, miratype]) => {
        const info = await MirabufLoader.cacheRemote(url, miratype)
        expect(info).toBeDefined()
        const assembly = await MirabufLoader.get(info!.hash)
        expect(assembly).toBeDefined()
        const sceneObject = await createMirabuf(info!.hash, assembly!)
        expect(sceneObject).toBeDefined()
        expect(sceneObject?.miraType).toBe(miratype)
    })
})
