import { assert, beforeAll, beforeEach, describe, expect, test, vi } from "vitest"
import DefaultAssetLoader from "@/mirabuf/DefaultAssetLoader.ts"
import MirabufLoader from "@/mirabuf/MirabufLoader.ts"
import MirabufCachingService, { MiraType } from "@/mirabuf/MirabufLoader.ts"

describe("Default Asset Tests", () => {
    beforeAll(async () => {
        await DefaultAssetLoader.refresh()
        vi.spyOn(console, "log").mockImplementation(() => {})
    })
    beforeEach(async () => {
        await MirabufLoader.removeAll()
    })

    test("Manifest loaded", () => {
        expect(DefaultAssetLoader.fields.length).toBeGreaterThan(1)
        expect(DefaultAssetLoader.robots.length).toBeGreaterThan(1)
    })

    test("Manifest hashes match assets (fields)", async () => {
        for (const asset of DefaultAssetLoader.fields) {
            const info = await MirabufCachingService.cacheRemote(asset.remotePath, asset.miraType)
            assert.exists(info)
            expect(asset.hash, `Hashes for "${info.name}" do not match`).toBe(info.hash)
            expect(asset.miraType).toBe(MiraType.FIELD)
            await MirabufCachingService.remove(info.hash)
        }
        expect(MirabufCachingService.getAll()).toHaveLength(0)
    })
    test("Manifest hashes match assets (robots)", async () => {
        for (const asset of DefaultAssetLoader.robots) {
            const info = await MirabufCachingService.cacheRemote(asset.remotePath, asset.miraType)
            assert.exists(info)
            expect(asset.hash, `Hashes for "${info.name}" do not match`).toBe(info.hash)
            expect(asset.miraType).toBe(MiraType.ROBOT)
            await MirabufCachingService.remove(info.hash)
        }
        expect(MirabufCachingService.getAll()).toHaveLength(0)
    })
})
