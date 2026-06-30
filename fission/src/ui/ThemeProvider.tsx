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
    const [primaryColor, setPrimaryColor] = useState(themeOptions.primary?.main ?? "#90caf9")
    const [secondaryColor, setSecondaryColor] = useState(themeOptions.secondary?.main ?? "#ce93d8")
    const [blueAllianceColor, setBlueAllianceColor] = useState(themeOptions.blueAlliance?.main ?? "#0066b3")
    const [redAllianceColor, setRedAllianceColor] = useState(themeOptions.redAlliance?.main ?? "#ed1c24")
    const [topBarColor, setTopBarColor] = useState(themeOptions.topBar?.main ?? "#3D4352")
    const [surfaceColor, setSurfaceColor] = useState(themeOptions.surface?.main ?? "#2A3340")
    const [topBarTextColor, setTopBarTextColor] = useState(themeOptions.topBarText?.main ?? "#BFC5CE")

    useEffect(() => {
        localStorage.setItem("theme", JSON.stringify(themeOptions))
    }, [themeOptions])

    const theme = useMemo(() => {
        const t = createTheme({
            palette: {
                mode: "dark",
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
                MuiPopover: {
                    defaultProps: {
                        elevation: 8,
                    },
                    styleOverrides: {
                        paper: {
                            boxShadow: "0 4px 8px rgba(0,0,0,0.25), 0 2px 4px rgba(0,0,0,0.2)",
                        },
                    },
                },
                MuiPaper: {
                    styleOverrides: {
                        root: {
                            boxShadow: "0 2px 6px rgba(0,0,0,0.24), 0 1px 3px rgba(0,0,0,0.18)",
                        },
                    },
                },
                MuiMenu: {
                    defaultProps: {
                        elevation: 8,
                    },
                    styleOverrides: {
                        paper: {
                            boxShadow: "0 4px 12px rgba(0,0,0,0.28), 0 2px 6px rgba(0,0,0,0.18)",
                        },
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
                topBar: t.palette.augmentColor({
                    color: {
                        main: topBarColor,
                    },
                    name: "topBar",
                }),
                surface: t.palette.augmentColor({
                    color: {
                        main: surfaceColor,
                    },
                    name: "surface",
                }),
                topBarText: t.palette.augmentColor({
                    color: {
                        main: topBarTextColor,
                    },
                    name: "topBarText",
                }),
            },
        })
    }, [primaryColor, secondaryColor, blueAllianceColor, redAllianceColor, topBarColor, surfaceColor, topBarTextColor])

    useEffect(() => {
        setThemeOptions({
            ...themeOptions,
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
            topBar: {
                main: topBarColor,
            },
            surface: {
                main: surfaceColor,
            },
            topBarText: {
                main: topBarTextColor,
            },
        })
    }, [primaryColor, secondaryColor, blueAllianceColor, redAllianceColor, topBarColor, surfaceColor, topBarTextColor])

    const themeContextValue = useMemo(
        () => ({
            setPrimaryColor,
            setSecondaryColor,
            primaryColor,
            secondaryColor,
            blueAllianceColor,
            setBlueAllianceColor,
            redAllianceColor,
            setRedAllianceColor,
            topBarColor,
            setTopBarColor,
            surfaceColor,
            setSurfaceColor,
            topBarTextColor,
            setTopBarTextColor,
        }),
        [primaryColor, secondaryColor, blueAllianceColor, redAllianceColor, topBarColor, surfaceColor, topBarTextColor]
    )

    return (
        <ThemeContext.Provider value={themeContextValue}>
            <MUIThemeProvider theme={theme}>{children}</MUIThemeProvider>
        </ThemeContext.Provider>
    )
}
