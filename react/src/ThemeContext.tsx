import React, { ReactNode, createContext, useContext, useState } from 'react'
import { RgbaColor } from 'react-colorful'

export enum ColorName {
    InteractiveSecondary = '--interactive-secondary',
    Background = '--background',
    BackgroundSecondary = '--background-secondary',
    MainText = '--main-text',
    Scrollbar = '--scrollbar',
    AcceptButton = '--accept-button',
    CancelButton = '--cancel-button',
    InteractiveElementText = '--interactive-element-text',
    Icon = '--icon',
    HighlightHover = '--highlight-hover',
    HighlightSelect = '--highlight-select',
    SkyboxTop = '--skybox-top',
    SkyboxBottom = '--skybox-bottom',
    FloorGrid = '--floor-grid',
}
export type ColorPropName = `${ColorName}`
export type Theme = { [name in ColorName]: RgbaColor };
export type Themes = { [name: string]: Theme };

type ThemeContextType = {
    themes: Themes
    defaultTheme: Theme
    currentTheme: string
    setTheme: (themeName: string) => void
    updateColor: (themeName: string, colorName: ColorName, rgbaColor: RgbaColor) => void
    createTheme: (themeName: string) => void
    deleteTheme: (themeName: string) => void
    deleteAllThemes: () => void
}

type ThemeProviderProps = {
    themes: Themes
    defaultTheme: Theme
    initialTheme: string
    children: ReactNode
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export const ThemeProvider: React.FC<ThemeProviderProps> = ({ initialTheme, themes, defaultTheme, children }) => {
    const [currentTheme, setCurrentTheme] = useState<string>(initialTheme);

    const updateColor = (themeName: string, colorName: ColorName, rgbaColor: RgbaColor) => {
        if (themes[themeName]) {
            themes[themeName][colorName] = rgbaColor;
        }
    }

    const createTheme = (themeName: string) => {
        themes[themeName] = defaultTheme;
    }

    const deleteTheme = (themeName: string) => {
        delete themes[themeName];
    }

    const deleteAllThemes = () => {
        themes = {
            Default: defaultTheme
        }
    }

    return (
        <ThemeContext.Provider value={{ themes, currentTheme, defaultTheme, setTheme: setCurrentTheme, updateColor, createTheme, deleteTheme, deleteAllThemes }}>
            {children}
        </ThemeContext.Provider>
    )
}

export const useTheme = () => {
    const context = useContext(ThemeContext);
    if (!context) {
        throw new Error("useTheme must be used within a ThemeProvider!");
    }
    return context;
}
