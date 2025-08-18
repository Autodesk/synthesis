import { type MirabufCacheInfo, MiraType } from "@/mirabuf/MirabufLoader.ts"
import type { ManifestFileType } from "../../update_manifest.ts"

export type DefaultAssetInfo = Required<Pick<MirabufCacheInfo, "hash" | "remotePath" | "miraType" | "name">>

class DefaultAssetLoader {
    private static _assets: DefaultAssetInfo[] = []

    static {
        setTimeout(() => this.refresh(), 1000)
    }

    private static async refresh() {
        this._assets = []

        const isElectron = window.electronAPI != null
        const host = isElectron ? "https://synthesis.autodesk.com" : ""
        const baseUrl = `${host}/api/mira`

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
