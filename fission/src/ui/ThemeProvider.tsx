import {
	ThemeProvider as MUIThemeProvider,
	createTheme,
} from "@mui/material/styles";
import type React from "react";
import { type ReactNode, createContext, useMemo, useState } from "react";
import type { PaletteMode } from "@mui/material";

interface ThemeProviderProps {
	children: ReactNode;
}

export const ThemeContext = createContext({
	toggleColorMode: () => {},
	setPrimaryColor: () => {},
	setSecondaryColor: () => {},
	primaryColor: "#1976d2",
	secondaryColor: "#dc004e",
});

export const ThemeProvider: React.FC<ThemeProviderProps> = ({ children }) => {
	const [mode, setMode] = useState<PaletteMode>("light");
	const [primaryColor, setPrimaryColor] = useState("#1976d2");
	const [secondaryColor, setSecondaryColor] = useState("#dc004e");

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
			}),
		[mode, primaryColor, secondaryColor],
	);

	const themeContextValue = useMemo(
		() => ({
			toggleColorMode: () => {
				setMode((prevMode) => (prevMode === "light" ? "dark" : "light"));
			},
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
