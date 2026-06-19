import { createContext, type ReactNode, useContext } from "react"

export interface ThemeProviderProps {
    children: ReactNode
}

export interface StoredTheme {
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
    topBar: {
        main: string
    }
    surface: {
        main: string
    }
    topBarText: {
        main: string
    }
}

export const ThemeContext = createContext({
    setPrimaryColor: (_color: string) => {},
    setSecondaryColor: (_color: string) => {},
    primaryColor: "",
    secondaryColor: "",
    blueAllianceColor: "",
    setBlueAllianceColor: (_color: string) => {},
    redAllianceColor: "",
    setRedAllianceColor: (_color: string) => {},
    topBarColor: "",
    setTopBarColor: (_color: string) => {},
    surfaceColor: "",
    setSurfaceColor: (_color: string) => {},
    topBarTextColor: "",
    setTopBarTextColor: (_color: string) => {},
})

export const useThemeContext = () => useContext(ThemeContext)
