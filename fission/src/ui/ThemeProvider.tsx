import type { PaletteMode } from "@mui/material"
import { createTheme, ThemeProvider as MUIThemeProvider } from "@mui/material/styles"
import type React from "react"
import { createContext, type ReactNode, useCallback, useContext, useEffect, useMemo, useState } from "react"

interface ThemeProviderProps {
	children: ReactNode;
}

interface StoredTheme {
	mode: PaletteMode;
	primary: {
		main: string;
	};
	secondary: {
		main: string;
	};
}

export const ThemeContext = createContext({
    mode: "dark",
    setMode: (_mode: PaletteMode) => {},
    setPrimaryColor: (_color: string) => {},
    setSecondaryColor: (_color: string) => {},
    primaryColor: "",
    secondaryColor: "",
})

export const useThemeContext = () => useContext(ThemeContext)

export const ThemeProvider: React.FC<ThemeProviderProps> = ({ children }) => {
    const loadTheme = useCallback(() => {
        const themeObj = localStorage.getItem("theme") ?? "{}"
        let json: StoredTheme;
        try {
            json = JSON.parse(themeObj)
        } catch (_e) {
            json = {} as StoredTheme
        }

        return json
    }, [])

	const [themeOptions, setThemeOptions] = useState<StoredTheme>(loadTheme());
	const [mode, setMode] = useState<PaletteMode>(themeOptions.mode ?? "dark");
	const [primaryColor, setPrimaryColor] = useState(themeOptions.primary?.main ?? "#90caf9");
	const [secondaryColor, setSecondaryColor] = useState(themeOptions.secondary?.main ?? "#ce93d8");

	useEffect(() => {
		localStorage.setItem("theme", JSON.stringify(themeOptions));
	}, [themeOptions]);

	const theme = useMemo(
		() =>
			createTheme({
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
			}),
		[mode, primaryColor, secondaryColor],
	);

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
		});
	}, [mode, primaryColor, secondaryColor]);

	const themeContextValue = useMemo(
		() => ({
            setMode,
			setPrimaryColor,
			setSecondaryColor,
			mode,
			primaryColor,
			secondaryColor,
		}),
		[mode, primaryColor, secondaryColor],
	);

	return (
		<ThemeContext.Provider value={themeContextValue}>
			<MUIThemeProvider theme={theme}>{children}</MUIThemeProvider>
		</ThemeContext.Provider>
	);
};
