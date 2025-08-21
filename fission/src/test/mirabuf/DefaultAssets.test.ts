import { assert, describe, expect, test, vi } from "vitest"
import DefaultAssetLoader from "@/mirabuf/DefaultAssetLoader.ts"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader.ts"

describe("Default Asset Tests", async () => {
    await DefaultAssetLoader.refresh()
    vi.spyOn(console, "log").mockImplementation(() => {})

    test("Manifest loaded", () => {
        expect(DefaultAssetLoader.fields.length).toBeGreaterThan(1)
        expect(DefaultAssetLoader.robots.length).toBeGreaterThan(1)
    })
    test.runIf(import.meta.env.VITE_RUN_ASSETPACK_TEST).each(DefaultAssetLoader.fields)(
        "Manifest hashes match assets ($name)",
        async asset => {
            const info = await MirabufCachingService.cacheRemote(asset.remotePath, asset.miraType)
            assert.exists(info)
            expect(asset.hash, `Hashes for "${info.name}" do not match`).toBe(info.hash)
            expect(asset.miraType).toBe(MiraType.FIELD)
            await MirabufCachingService.remove(info.hash)
        }
    )
    test.runIf(import.meta.env.VITE_RUN_ASSETPACK_TEST).each(DefaultAssetLoader.robots)(
        "Manifest hashes match assets ($name)",
        async asset => {
            const info = await MirabufCachingService.cacheRemote(asset.remotePath, asset.miraType)
            assert.exists(info)
            expect(asset.hash, `Hashes for "${info.name}" do not match`).toBe(info.hash)
            expect(asset.miraType).toBe(MiraType.ROBOT)
            await MirabufCachingService.remove(info.hash)
        }
    )
})
