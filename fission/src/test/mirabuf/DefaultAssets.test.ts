import { afterAll, assert, describe, expect, test, vi } from "vitest"
import DefaultAssetLoader from "@/mirabuf/DefaultAssetLoader.ts"
import MirabufLoader from "@/mirabuf/MirabufLoader.ts"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader.ts"

describe("Default Asset Tests", async () => {
    await DefaultAssetLoader.refresh()
    vi.spyOn(console, "log").mockImplementation(() => {})
    afterAll(async () => {
        await MirabufLoader.removeAll()
    })

    test("Manifest loaded", () => {
        expect(DefaultAssetLoader.fields.length).toBeGreaterThan(1)
        expect(DefaultAssetLoader.robots.length).toBeGreaterThan(1)
    })

    test.each(DefaultAssetLoader.fields)("Manifest hashes match assets ($name)", async asset => {
        const info = await MirabufCachingService.cacheRemote(asset.remotePath, asset.miraType)
        assert.exists(info)
        expect(asset.hash, `Hashes for "${info.name}" do not match`).toBe(info.hash)
        expect(asset.miraType).toBe(MiraType.FIELD)
        await MirabufCachingService.remove(info.hash)
    })
    test.each(DefaultAssetLoader.robots)("Manifest hashes match assets ($name)", async asset => {
        const info = await MirabufCachingService.cacheRemote(asset.remotePath, asset.miraType)
        assert.exists(info)
        expect(asset.hash, `Hashes for "${info.name}" do not match`).toBe(info.hash)
        expect(asset.miraType).toBe(MiraType.ROBOT)
        await MirabufCachingService.remove(info.hash)
    })
})
