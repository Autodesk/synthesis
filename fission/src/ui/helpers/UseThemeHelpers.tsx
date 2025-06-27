import { createContext, useContext } from "react"
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

export type ThemeContextType = {
    themes: Themes
    initialThemeName: string
    defaultTheme: Theme
    currentTheme: string
    setTheme: (themeName: string) => void
    updateColor: (themeName: string, colorName: ColorName, rgbaColor: RgbaColor) => void
    createTheme: (themeName: string) => void
    deleteTheme: (themeName: string) => void
    deleteAllThemes: () => void
    applyTheme: (themeName: string) => void
}

export const ThemeContext = createContext<ThemeContextType | undefined>(undefined)

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

export const useTheme = () => {
    const context = useContext(ThemeContext)
    if (!context) {
        throw new Error("useTheme must be used within a ThemeProvider!")
    }
    return context
}
