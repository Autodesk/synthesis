import { RgbaColor } from "react-colorful"

export const defaultThemeName = "Default"
export type ColorName =
    | "InteractiveElementSolid"
    | "InteractiveElementLeft"
    | "InteractiveElementRight"
    | "Background"
    | "BackgroundSecondary"
    | "InteractiveBackground"
    | "BackgroundHUD"
    | "InteractiveHover"
    | "InteractiveSelect"
    | "MainText"
    | "Scrollbar"
    | "AcceptButton"
    | "CancelButton"
    | "InteractiveElementText"
    | "Icon"
    | "MainHUDIcon"
    | "MainHUDCloseIcon"
    | "HighlightHover"
    | "HighlightSelect"
    | "SkyboxTop"
    | "SkyboxBottom"
    | "FloorGrid"
    | "AcceptCancelButtonText"
    | "MatchRedAlliance"
    | "MatchBlueAlliance"
    | "ToastInfo"
    | "ToastWarning"
    | "ToastError"


export const colorNameToTailwind = (colorName: ColorName) => {
    return (
        "bg" +
        colorName
            .replace(/([A-Z]+)/g, "-$1")
            .replace(/(?<=[A-Z])([A-Z])(?![A-Z]|$)/g, "-$1")
            .toLowerCase()
    )
}
export const colorNameToProp = (colorName: ColorName) => {
    return (
        "-" +
        colorName
            .replace(/([A-Z]+)/g, "-$1")
            .replace(/(?<=[A-Z])([A-Z])(?![A-Z]|$)/g, "-$1")
            .toLowerCase()
    )
}

export const colorNameToVar = (colorName: ColorName) => {
    return `var(${colorNameToProp(colorName)})`
}

export type Theme = {
    [name in ColorName]: { color: RgbaColor; above: (ColorName | string)[] }
}
export type Themes = { [name: string]: Theme }
