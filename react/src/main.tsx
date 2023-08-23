import React from "react"
import ReactDOM from "react-dom/client"
import App from "./App.tsx"
import { Theme, ThemeProvider } from "./ThemeContext.tsx"
import "./index.css"

const initialTheme = "Default"
const defaultColors: Theme = {
    InteractiveElementSolid: { r: 250, g: 162, b: 27, a: 1 },
    InteractiveElementLeft: { r: 224, g: 130, b: 65, a: 1 },
    InteractiveElementRight: { r: 218, g: 102, b: 89, a: 1 },
    Background: { r: 0, g: 0, b: 0, a: 1 },
    BackgroundSecondary: { r: 30, g: 30, b: 30, a: 1 },
    InteractiveBackground: { r: 52, g: 58, b: 64, a: 1 },
    MainText: { r: 255, g: 255, b: 255, a: 1 },
    Scrollbar: { r: 170, g: 170, b: 170, a: 1 },
    AcceptButton: { r: 71, g: 138, b: 226, a: 1 },
    CancelButton: { r: 231, g: 85, b: 81, a: 1 },
    InteractiveElementText: { r: 255, g: 255, b: 255, a: 1 },
    AcceptCancelButtonText: { r: 0, g: 0, b: 0, a: 1 },
    BackgroundHUD: { r: 23, g: 23, b: 23, a: 1 },
    InteractiveHover: { r: 150, g: 150, b: 150, a: 1 },
    InteractiveSelect: { r: 100, g: 100, b: 100, a: 1 },
    Icon: { r: 255, g: 255, b: 255, a: 1 },
    MainHUDIcon: { r: 255, g: 255, b: 255, a: 1 },
    MainHUDCloseIcon: { r: 0, g: 0, b: 0, a: 1 },
    HighlightHover: { r: 89, g: 255, b: 133, a: 1 },
    HighlightSelect: { r: 255, g: 89, b: 133, a: 1 },
    SkyboxTop: { r: 255, g: 255, b: 255, a: 1 },
    SkyboxBottom: { r: 255, g: 255, b: 255, a: 1 },
    FloorGrid: { r: 93, g: 93, b: 93, a: 1 },
    MatchRedAlliance: { r: 255, g: 0, b: 0, a: 1 },
    MatchBlueAlliance: { r: 0, g: 0, b: 255, a: 1 },
    ToastInfo: { r: 126, g: 34, b: 206, a: 1 },
    ToastWarning: { r: 234, g: 179, b: 8, a: 1 },
    ToastError: { r: 239, g: 68, b: 68, a: 1 },
}
const themes = {
    Default: defaultColors,
}

ReactDOM.createRoot(document.getElementById("root")!).render(
    <React.StrictMode>
        <ThemeProvider
            initialTheme={initialTheme}
            themes={themes}
            defaultTheme={defaultColors}
        >
            <App />
        </ThemeProvider>
    </React.StrictMode>
)
