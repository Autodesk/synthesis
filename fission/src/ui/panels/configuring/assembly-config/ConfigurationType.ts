export enum ConfigurationType {
    ROBOT,
    FIELD,
    PIECES,
    INPUTS,
}

let selectedConfigurationType: ConfigurationType = ConfigurationType.ROBOT

export function setSelectedConfigurationType(type: ConfigurationType) {
    selectedConfigurationType = type
}

export function getConfigurationType() {
    return selectedConfigurationType
}
