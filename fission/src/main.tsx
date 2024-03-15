import React from "react"
import ReactDOM from "react-dom/client"
import { Theme, ThemeProvider } from "./ThemeContext"
import Synthesis from "./Synthesis"
import "./index.css"

const initialThemeName = "Default"
const defaultColors: Theme = {
    InteractiveElementSolid: { color: { r: 250, g: 162, b: 27, a: 1 }, above: [] },
    InteractiveElementLeft: { color: { r: 224, g: 130, b: 65, a: 1 }, above: ['Background', 'BackgroundSecondary'] },
    InteractiveElementRight: { color: { r: 218, g: 102, b: 89, a: 1 }, above: ['Background', 'BackgroundSecondary'] },
    Background: { color: { r: 0, g: 0, b: 0, a: 1 }, above: [] },
    BackgroundSecondary: { color: { r: 30, g: 30, b: 30, a: 1 }, above: [] },
    InteractiveBackground: { color: { r: 52, g: 58, b: 64, a: 1 }, above: [] },
    MainText: { color: { r: 255, g: 255, b: 255, a: 1 }, above: ['Background', 'BackgroundSecondary', 'BackgroundHUD', 'InteractiveBackground', 'InteractiveElementLeft', 'InteractiveElementRight'] },
    Scrollbar: { color: { r: 170, g: 170, b: 170, a: 1 }, above: [] },
    AcceptButton: { color: { r: 71, g: 138, b: 226, a: 1 }, above: [] },
    CancelButton: { color: { r: 231, g: 85, b: 81, a: 1 }, above: [] },
    InteractiveElementText: { color: { r: 255, g: 255, b: 255, a: 1 }, above: [] },
    AcceptCancelButtonText: { color: { r: 0, g: 0, b: 0, a: 1 }, above: ['AcceptButton', 'CancelButton'] },
    BackgroundHUD: { color: { r: 23, g: 23, b: 23, a: 1 }, above: [] },
    InteractiveHover: { color: { r: 150, g: 150, b: 150, a: 1 }, above: [] },
    InteractiveSelect: { color: { r: 100, g: 100, b: 100, a: 1 }, above: [] },
    Icon: { color: { r: 255, g: 255, b: 255, a: 1 }, above: ['Background', 'BackgroundSecondary', 'InteractiveBackground'] },
    MainHUDIcon: { color: { r: 255, g: 255, b: 255, a: 1 }, above: ['BackgroundHUD'] },
    MainHUDCloseIcon: { color: { r: 0, g: 0, b: 0, a: 1 }, above: ['InteractiveElementRight', '#ffffff'] },
    HighlightHover: { color: { r: 89, g: 255, b: 133, a: 1 }, above: [] },
    HighlightSelect: { color: { r: 255, g: 89, b: 133, a: 1 }, above: [] },
    SkyboxTop: { color: { r: 255, g: 255, b: 255, a: 1 }, above: [] },
    SkyboxBottom: { color: { r: 255, g: 255, b: 255, a: 1 }, above: [] },
    FloorGrid: { color: { r: 93, g: 93, b: 93, a: 1 }, above: [] },
    MatchRedAlliance: { color: { r: 255, g: 0, b: 0, a: 1 }, above: [] },
    MatchBlueAlliance: { color: { r: 0, g: 0, b: 255, a: 1 }, above: [] },
    ToastInfo: { color: { r: 126, g: 34, b: 206, a: 1 }, above: [] },
    ToastWarning: { color: { r: 234, g: 179, b: 8, a: 1 }, above: [] },
    ToastError: { color: { r: 239, g: 68, b: 68, a: 1 }, above: [] },
}
const themes = {
    Default: defaultColors,
}

ReactDOM.createRoot(document.getElementById("root")!).render(
    <React.StrictMode>
        <ThemeProvider
            initialThemeName={initialThemeName}
            themes={themes}
            defaultTheme={defaultColors}
        >
            <Synthesis />
        </ThemeProvider>
    </React.StrictMode>
)
