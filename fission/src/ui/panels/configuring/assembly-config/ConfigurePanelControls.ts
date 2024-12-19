import MirabufSceneObject from "@/mirabuf/MirabufSceneObject"

export enum ConfigMode {
    SUBSYSTEMS,
    EJECTOR,
    INTAKE,
    CONTROLS,
    SEQUENTIAL,
    SCORING_ZONES,
    MOVE,
    SIM,
    BRAIN,
}

export type ConfigurePanelSettings = {
    configMode?: ConfigMode
    selectedAssembly: MirabufSceneObject
}

let configurePanelSettings: ConfigurePanelSettings | undefined = undefined

export function popConfigurePanelSettings(): ConfigurePanelSettings | undefined {
    const tmp = configurePanelSettings
    configurePanelSettings = undefined
    return tmp
}

export function setNextConfigurePanelSettings(settings: ConfigurePanelSettings) {
    configurePanelSettings = settings
}
