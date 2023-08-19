import React from "react"
import ReactDOM from "react-dom/client"
import App from "./App.tsx"
import { Theme, ThemeProvider } from "./ThemeContext.tsx"
import "./index.css"

const initialTheme = "Default"
const defaultColors: Theme = {
    InteractiveElementSolid: { r: 250, g: 162, b: 27, a: 255 },
    InteractiveElementLeft: { r: 224, g: 130, b: 65, a: 255 },
    InteractiveElementRight: { r: 218, g: 102, b: 89, a: 255 },
    Background: { r: 0, g: 0, b: 0, a: 255 },
    BackgroundSecondary: { r: 30, g: 30, b: 30, a: 255 },
    InteractiveBackground: { r: 52, g: 58, b: 64, a: 255 },
    MainText: { r: 255, g: 255, b: 255, a: 255 },
    Scrollbar: { r: 170, g: 170, b: 170, a: 255 },
    AcceptButton: { r: 71, g: 138, b: 226, a: 255 },
    CancelButton: { r: 231, g: 85, b: 81, a: 255 },
    InteractiveElementText: { r: 255, g: 255, b: 255, a: 255 },
    AcceptCancelButtonText: { r: 0, g: 0, b: 0, a: 255 },
    BackgroundHUD: { r: 23, g: 23, b: 23, a: 255 },
    InteractiveHover: { r: 150, g: 150, b: 150, a: 255 },
    InteractiveSelect: { r: 100, g: 100, b: 100, a: 255 },
    Icon: { r: 255, g: 255, b: 255, a: 255 },
    MainHUDIcon: { r: 255, g: 255, b: 255, a: 255 },
    MainHUDCloseIcon: { r: 0, g: 0, b: 0, a: 255 },
    HighlightHover: { r: 89, g: 255, b: 133, a: 255 },
    HighlightSelect: { r: 255, g: 89, b: 133, a: 255 },
    SkyboxTop: { r: 255, g: 255, b: 255, a: 255 },
    SkyboxBottom: { r: 255, g: 255, b: 255, a: 255 },
    FloorGrid: { r: 93, g: 93, b: 93, a: 255 },
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
