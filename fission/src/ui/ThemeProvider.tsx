import type { PaletteMode } from "@mui/material"
import { createTheme, ThemeProvider as MUIThemeProvider } from "@mui/material/styles"
import type React from "react"
import { createContext, type ReactNode, useMemo, useState } from "react"

interface ThemeProviderProps {
    children: ReactNode
}

export const ThemeContext = createContext({
    mode: "",
    toggleColorMode: () => {},
    setPrimaryColor: (_color: string) => {},
    setSecondaryColor: (_color: string) => {},
    primaryColor: "",
    secondaryColor: "",
})

export const ThemeProvider: React.FC<ThemeProviderProps> = ({ children }) => {
    const [mode, setMode] = useState<PaletteMode>("dark")
    const theme = useMemo(
        () =>
            createTheme({
                palette: {
                    mode,
                },
                components: {
                    MuiButton: {
                        defaultProps: {
                            variant: "contained",
                        },
                    },
                },
            }),
        [mode]
    )

    const [primaryColor, setPrimaryColor] = useState(theme.palette.primary.dark)
    const [secondaryColor, setSecondaryColor] = useState(theme.palette.secondary.dark)

    const themeContextValue = useMemo(
        () => ({
            toggleColorMode: () => {
                setMode(prevMode => (prevMode === "light" ? "dark" : "light"))
            },
            setPrimaryColor,
            setSecondaryColor,
            mode,
            primaryColor,
            secondaryColor,
        }),
        [mode, primaryColor, secondaryColor]
    )

    return (
        <ThemeContext.Provider value={themeContextValue}>
            <MUIThemeProvider theme={theme}>{children}</MUIThemeProvider>
        </ThemeContext.Provider>
    )
}
