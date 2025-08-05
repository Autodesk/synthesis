import type { PaletteMode } from "@mui/material"
import { createTheme, ThemeProvider as MUIThemeProvider } from "@mui/material/styles"
import type React from "react"
import { useCallback, useEffect, useMemo, useState } from "react"
import { type StoredTheme, ThemeContext, type ThemeProviderProps } from "./helpers/ThemeProviderHelpers"

export const ThemeProvider: React.FC<ThemeProviderProps> = ({ children }) => {
    const loadTheme = useCallback(() => {
        const themeObj = localStorage.getItem("theme") ?? "{}"
        let json: StoredTheme
        try {
            json = JSON.parse(themeObj)
        } catch (_e) {
            json = {} as StoredTheme
        }

        return json
    }, [])

    const [themeOptions, setThemeOptions] = useState<StoredTheme>(loadTheme())
    const [mode, setMode] = useState<PaletteMode>(themeOptions.mode ?? "dark")
    const [primaryColor, setPrimaryColor] = useState(themeOptions.primary?.main ?? "#90caf9")
    const [secondaryColor, setSecondaryColor] = useState(themeOptions.secondary?.main ?? "#ce93d8")
    const [blueAllianceColor, setBlueAllianceColor] = useState(themeOptions.blueAlliance?.main ?? "#0066b3")
    const [redAllianceColor, setRedAllianceColor] = useState(themeOptions.redAlliance?.main ?? "#ed1c24")

    useEffect(() => {
        localStorage.setItem("theme", JSON.stringify(themeOptions))
    }, [themeOptions])

    const theme = useMemo(() => {
        const t = createTheme({
            palette: {
                mode,
                primary: {
                    main: primaryColor,
                },
                secondary: {
                    main: secondaryColor,
                },
            },
            components: {
                MuiButton: {
                    defaultProps: {
                        variant: "contained",
                    },
                },
            },
        })

        return createTheme(t, {
            palette: {
                redAlliance: t.palette.augmentColor({
                    color: {
                        main: redAllianceColor,
                    },
                    name: "redAlliance",
                }),
                blueAlliance: t.palette.augmentColor({
                    color: {
                        main: blueAllianceColor,
                    },
                    name: "blueAlliance",
                }),
            },
        })
    }, [mode, primaryColor, secondaryColor, blueAllianceColor, redAllianceColor])

    useEffect(() => {
        setThemeOptions({
            ...themeOptions,
            mode,
            primary: {
                main: primaryColor,
            },
            secondary: {
                main: secondaryColor,
            },
            blueAlliance: {
                main: blueAllianceColor,
            },
            redAlliance: {
                main: redAllianceColor,
            },
        })
    }, [mode, primaryColor, secondaryColor, blueAllianceColor, redAllianceColor])

    const themeContextValue = useMemo(
        () => ({
            setMode,
            setPrimaryColor,
            setSecondaryColor,
            mode,
            primaryColor,
            secondaryColor,
            blueAllianceColor,
            setBlueAllianceColor,
            redAllianceColor,
            setRedAllianceColor,
        }),
        [mode, primaryColor, secondaryColor, blueAllianceColor, redAllianceColor]
    )

    return (
        <ThemeContext.Provider value={themeContextValue}>
            <MUIThemeProvider theme={theme}>{children}</MUIThemeProvider>
        </ThemeContext.Provider>
    )
}
