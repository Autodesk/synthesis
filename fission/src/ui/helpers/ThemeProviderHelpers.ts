import type { PaletteMode } from "@mui/material"
import { createContext, ReactNode, useContext } from "react"

export interface ThemeProviderProps {
    children: ReactNode
}

export interface StoredTheme {
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
