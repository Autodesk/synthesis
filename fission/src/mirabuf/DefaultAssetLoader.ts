import { type MirabufCacheInfo, MiraType } from "@/mirabuf/MirabufLoader.ts"
import type { ManifestFileType } from "../../manifest.d.ts"
import { API_URL } from "@/util/Consts.ts"

export type DefaultAssetInfo = Required<Pick<MirabufCacheInfo, "hash" | "remotePath" | "miraType" | "name">>

class DefaultAssetLoader {
    private static _assets: DefaultAssetInfo[] = []
    private static _hasLoaded = false
    static {
        setTimeout(() => {
            if (!this._hasLoaded) this.refresh().catch(console.error)
        }, 1000)
    }

    public static async refresh() {
        this._hasLoaded = true
        this._assets = []
        const baseUrl = `${API_URL}/mira`
        const manifest: ManifestFileType = await fetch(`${baseUrl}/manifest.json`).then(x => x.json())

        const miraTypeMap: Partial<Record<keyof ManifestFileType, MiraType>> = {
            robots: MiraType.ROBOT,
            fields: MiraType.FIELD,
        }

        Object.entries(manifest).forEach(([dir, assets]) => {
            const miraType = miraTypeMap[dir as keyof ManifestFileType]
            if (miraType == undefined) return

            assets.forEach(obj => {
                this._assets.push({
                    remotePath: `${baseUrl}/${dir}/${obj.filename}`,
                    hash: obj.hash,
                    miraType,
                    name: obj.filename,
                })
            })
        })
    }

    public static get robots() {
        return this._assets.filter(obj => obj.miraType == MiraType.ROBOT)
    }

    public static get fields() {
        return this._assets.filter(obj => obj.miraType == MiraType.FIELD)
    }
}

export default DefaultAssetLoader
