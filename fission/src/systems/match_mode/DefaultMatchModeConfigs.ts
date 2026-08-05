import type { MatchModeConfig } from "@/ui/panels/configuring/MatchModeConfigPanel"
import { API_URL } from "@/util/Consts.ts"

type ManifestMatchModeConfig = Omit<MatchModeConfig, "id">
interface MatchConfigManifest {
    private: Record<string, ManifestMatchModeConfig>
    public: Record<string, ManifestMatchModeConfig>
}

/** The purpose of this class is to store any defaults related to match mode configurations. */
class DefaultMatchModeConfigs {
    private static readonly MANIFEST_LOCATION = `${API_URL}/match_configs/manifest.json`
    private static _configs: MatchModeConfig[] = []

    private static _loading: Promise<void> = new Promise<void>(resolve => {
        setTimeout(() => resolve(this.load()))
    })

    static reload(): Promise<void> {
        this._loading = this.load()
        return this._loading
    }

    private static async load(): Promise<void> {
        const configs = await this.fetchConfigs()
        if (configs) {
            this._configs = configs
        }
    }

    private static async fetchConfigs(): Promise<MatchModeConfig[] | undefined> {
        try {
            const response = await fetch(this.MANIFEST_LOCATION)
            const manifest: MatchConfigManifest | undefined = await response.json()
            if (manifest == undefined) {
                console.error("Could not load match mode manifest")
                return undefined
            }

            const keys: (keyof MatchConfigManifest)[] = import.meta.env.DEV ? ["public", "private"] : ["public"]
            return keys.flatMap(key => Object.entries(manifest[key] ?? {}).map(([id, config]) => ({ ...config, id })))
        } catch (e) {
            console.error("Could not load match mode manifest", e)
            return undefined
        }
    }

    public static async getConfigs(): Promise<MatchModeConfig[]> {
        await this._loading
        return this._configs
    }

    public static get configs(): MatchModeConfig[] {
        return this._configs
    }

    static fallbackValues = (): MatchModeConfig => {
        return {
            id: "default",
            name: "Default",
            isDefault: true,
            autonomousTime: 15,
            teleopTime: 135,
            endgameTime: 20,
            ignoreRotation: true,
            maxHeight: -1,
            heightLimitPenalty: 2,
            sideMaxExtension: -1,
            sideExtensionPenalty: 2,
        }
    }
}

export default DefaultMatchModeConfigs
