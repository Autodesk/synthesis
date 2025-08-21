import type { MatchModeConfig } from "@/ui/panels/configuring/MatchModeConfigPanel"
import { API_URL } from "@/util/Consts.ts"

type ManifestMatchModeConfig = Omit<MatchModeConfig, "id"> & { id: string }
interface MatchConfigManifest {
    private: Record<string, ManifestMatchModeConfig>
    public: Record<string, ManifestMatchModeConfig>
}

/** The purpose of this class is to store any defaults related to match mode configurations. */
class DefaultMatchModeConfigs {
    private static readonly MANIFEST_LOCATION = `${API_URL}/match_configs/manifest.json`
    private static _configs: MatchModeConfig[] = []

    static {
        setTimeout(() => this.reload())
    }
    static async reload() {
        const manifest = await fetch(this.MANIFEST_LOCATION)
        const json: MatchConfigManifest | undefined = await manifest.json().catch(e => {
            console.error(e)
            return undefined
        })
        if (json == null) {
            console.error("Could not load match mode manifest")
            return undefined
        }
        const keys: (keyof MatchConfigManifest)[] = import.meta.env.DEV
            ? (["public", "private"] as const)
            : (["public"] as const)
        for (const key of keys) {
            const configs = json[key as keyof MatchConfigManifest]
            Object.entries(configs).forEach(([key, value]) => {
                value.id = key
                this._configs.push(value)
            })
        }
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
