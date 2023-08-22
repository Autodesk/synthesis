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
    | "MatchRedAlliance"
    | "MatchBlueAlliance"
    | "ToastInfo"
    | "ToastWarning"
    | "ToastError"

export const colorNameToTailwind = (colorName: ColorName) => {
    return "bg" +
        colorName
            .replace(/([A-Z]+)/g, "-$1")
            .replace(/(?<=[A-Z])([A-Z])(?![A-Z]|$)/g, "-$1")
            .toLowerCase()
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

    // potentially dumb algorithm
    const findUnusedColor = () => {
        const MAX_VALUE = 255;
        const reds = Object.values(themes[currentTheme]).map((c: RgbaColor) => c.r).sort();
        const greens = Object.values(themes[currentTheme]).map((c: RgbaColor) => c.r).sort();
        const blues = Object.values(themes[currentTheme]).map((c: RgbaColor) => c.r).sort();


        const values = [];

        for (const color of [reds, greens, blues]) {
            let lower = -1;
            let largestGap = -1;

            for (let i = 0; i < color.length - 1; i++) {
                const col1 = color[i];
                const col2 = color[i + 1];

                if (col2 - col1 > largestGap) {
                    largestGap = col2 - col1;
                    lower = col1;
                }
            }

            const col1 = color[color.length - 1];
            const col2 = color[0] + MAX_VALUE;

            if (col1 - col2 > largestGap) {
                largestGap = col2 - col1;
                lower = col1;
            }

            values.push((lower + largestGap / 2) % MAX_VALUE);
        }

        document.body.style.background = `rgba(${values[0]}, ${values[1]}, ${values[2]}, ${MAX_VALUE})`;
    }

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
            const propName = colorNameToProp(n as ColorName)
            root.style.setProperty(
                propName,
                `rgba(${c.r}, ${c.g}, ${c.b}, ${c.a})`
            )
        })
        findUnusedColor();
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
