import { describe, expect, test } from "vitest"
import DefaultMatchModeConfigs from "@/systems/match_mode/DefaultMatchModeConfigs.ts"

describe("Match Mode Config Checks", () => {
    test("Default Configs are Serializable", () => {
        const configs = DefaultMatchModeConfigs.defaultMatchModeConfigCopies
        configs.forEach(config => {
            expect(JSON.parse(JSON.stringify(config))).toEqual(config)
        })
    })
})
