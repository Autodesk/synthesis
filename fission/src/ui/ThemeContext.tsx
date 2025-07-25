import React, { ReactNode, useState } from "react"
import { RgbaColor } from "react-colorful"
import { addGlobalFunc } from "@/util/dom"
import { ColorName, colorNameToProp, defaultThemeName, Theme, ThemeContext, Themes } from "./helpers/UseThemeHelpers"

type ThemeProviderProps = {
    themes: Themes
    defaultTheme: Theme
    initialThemeName: string
    children: ReactNode
}

export const ThemeProvider: React.FC<ThemeProviderProps> = ({ initialThemeName, themes, defaultTheme, children }) => {
    const [currentTheme, setCurrentTheme] = useState<string>(initialThemeName)

    addGlobalFunc<Theme>("getTheme", () => themes[currentTheme])

    // potentially dumb algorithm
    const findUnusedColor = () => {
        if (process.env.NODE_ENV !== "production") return
        const MAX_VALUE = 255
        const sortFunc = (a: number, b: number) => a - b
        const themeValue: RgbaColor[] = Object.values(themes[currentTheme]).map(v => v.color)
        const reds = themeValue.map(c => c.r).sort(sortFunc)
        const greens = themeValue.map(c => c.g).sort(sortFunc)
        const blues = themeValue.map(c => c.b).sort(sortFunc)

        const values = []

        for (const colorArr of [reds, greens, blues]) {
            const color = Array.from(new Set(colorArr))
            let lower = -1
            let largestGap = -1

            for (let i = 0; i < color.length - 1; i++) {
                const col1 = color[i]
                const col2 = color[i + 1]

                if (col2 - col1 > largestGap) {
                    largestGap = col2 - col1
                    lower = col1
                }
            }

            const col1 = color[color.length - 1]
            const col2 = color[0] + MAX_VALUE

            if (col1 - col2 > largestGap) {
                largestGap = col2 - col1
                lower = col1
            }

            values.push((lower + largestGap / 2) % MAX_VALUE)
        }

        document.body.style.background = `rgba(${values[0]}, ${values[1]}, ${values[2]}, ${MAX_VALUE})`
    }

    const updateColor = (themeName: string, colorName: ColorName, rgbaColor: RgbaColor) => {
        if (themes[themeName]) {
            themes[themeName][colorName].color = rgbaColor
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
            root.style.setProperty(propName, `rgba(${c.color.r}, ${c.color.g}, ${c.color.b}, ${c.color.a})`)
        })
        findUnusedColor()
    }

    return (
        <ThemeContext.Provider
            value={{
                themes,
                initialThemeName,
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
