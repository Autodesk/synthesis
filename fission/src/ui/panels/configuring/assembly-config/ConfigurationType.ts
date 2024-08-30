// eslint-disable-next-line react-refresh/only-export-components
export enum ConfigurationType {
    ROBOT,
    FIELD,
    INPUTS,
}

let selectedConfigurationType: ConfigurationType = ConfigurationType.ROBOT
// eslint-disable-next-line react-refresh/only-export-components
export function setSelectedConfigurationType(type: ConfigurationType) {
    selectedConfigurationType = type
}

export function getConfigurationType() {
    return selectedConfigurationType
}