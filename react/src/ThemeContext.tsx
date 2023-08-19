import React, { ReactNode, createContext, useContext, useState } from "react"
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

const colorNameToProp = (colorName: ColorName) => {
    return (
        "-" +
        colorName
            .replace(/([A-Z]+)/g, "-$1")
            .replace(/(?<=[A-Z])([A-Z])(?![A-Z])/g, "-$1")
            .toLowerCase()
    )
}
export type Theme = { [name in ColorName]: RgbaColor }
export type Themes = { [name: string]: Theme }

type ThemeContextType = {
    themes: Themes
    defaultTheme: Theme
    currentTheme: string
    setTheme: (themeName: string) => void
    updateColor: (
        themeName: string,
        colorName: ColorName,
        rgbaColor: RgbaColor
    ) => void
    createTheme: (themeName: string) => void
    deleteTheme: (themeName: string) => void
    deleteAllThemes: () => void
    applyTheme: (themeName: string) => void
}

type ThemeProviderProps = {
    themes: Themes
    defaultTheme: Theme
    initialTheme: string
    children: ReactNode
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined)

export const ThemeProvider: React.FC<ThemeProviderProps> = ({
    initialTheme,
    themes,
    defaultTheme,
    children,
}) => {
    const [currentTheme, setCurrentTheme] = useState<string>(initialTheme)

    const updateColor = (
        themeName: string,
        colorName: ColorName,
        rgbaColor: RgbaColor
    ) => {
        if (themes[themeName]) {
            themes[themeName][colorName] = rgbaColor
        }
    }

    const createTheme = (themeName: string) => {
        // deep copy
        themes[themeName] = JSON.parse(JSON.stringify(defaultTheme))
    }

    const deleteTheme = (themeName: string) => {
        if (themeName == defaultThemeName) return
        if (themeName == currentTheme) {
            setCurrentTheme(defaultThemeName)
        }
        delete themes[themeName]
    }

    const deleteAllThemes = () => {
        for (const themeName of Object.keys(themes)) {
            deleteTheme(themeName)
        }
        setCurrentTheme(defaultThemeName)
    }

    const applyTheme = (themeName: string) => {
        const themeObject: Theme = themes[themeName]
        if (!themeObject) return

        const root = document.documentElement
        Object.entries(themeObject).map(([n, c]) => {
            root.style.setProperty(
                colorNameToProp(n as ColorName),
                `rgba(${c.r}, ${c.g}, ${c.b}, ${c.a}`
            )
        })
    }

    return (
        <ThemeContext.Provider
            value={{
                themes,
                currentTheme,
                defaultTheme,
                setTheme: setCurrentTheme,
                updateColor,
                createTheme,
                deleteTheme,
                deleteAllThemes,
                applyTheme,
            }}
        >
            {children}
        </ThemeContext.Provider>
    )
}

export const useTheme = () => {
    const context = useContext(ThemeContext)
    if (!context) {
        throw new Error("useTheme must be used within a ThemeProvider!")
    }
    return context
}
