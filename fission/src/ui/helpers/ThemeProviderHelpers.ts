import type { PaletteMode } from "@mui/material"
import { createContext, type ReactNode, useContext } from "react"

export interface ThemeProviderProps {
    children: ReactNode
}

export interface StoredTheme {
    mode: PaletteMode
    primary: {
        main: string
    }
    secondary: {
        main: string
    }
    blueAlliance: {
        main: string
    }
    redAlliance: {
        main: string
    }
}

export const ThemeContext = createContext({
    mode: "dark",
    setMode: (_mode: PaletteMode) => {},
    setPrimaryColor: (_color: string) => {},
    setSecondaryColor: (_color: string) => {},
    primaryColor: "",
    secondaryColor: "",
    blueAllianceColor: "",
    setBlueAllianceColor: (_color: string) => {},
    redAllianceColor: "",
    setRedAllianceColor: (_color: string) => {},
})

export const useThemeContext = () => useContext(ThemeContext)
