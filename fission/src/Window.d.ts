import { Theme } from "@mui/material"

declare interface Window {
    setAuthCode(code: string): void
    getTheme(): Theme
}
