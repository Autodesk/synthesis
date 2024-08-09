import ReactDOM from "react-dom/client"
import { Theme, ThemeProvider } from "@/ui/ThemeContext"
import Synthesis from "./Synthesis"
import "./index.css"
import APS from "./aps/APS"

const initialThemeName = "Default"
const defaultColors: Theme = {
    InteractiveElementSolid: {
        color: { r: 250, g: 162, b: 27, a: 1 },
        above: [],
    },
    InteractiveElementLeft: {
        color: { r: 207, g: 114, b: 57, a: 1 },
        above: ["Background", "BackgroundSecondary"],
    },
    InteractiveElementRight: {
        color: { r: 212, g: 75, b: 62, a: 1 },
        above: ["Background", "BackgroundSecondary"],
    },
    Background: { color: { r: 0, g: 0, b: 0, a: 1 }, above: [] },
    BackgroundSecondary: { color: { r: 18, g: 18, b: 18, a: 1 }, above: [] },
    InteractiveBackground: { color: { r: 40, g: 44, b: 47, a: 1 }, above: [] },
    MainText: {
        color: { r: 255, g: 255, b: 255, a: 1 },
        above: [
            "Background",
            "BackgroundSecondary",
            "BackgroundHUD",
            "InteractiveBackground",
            "InteractiveElementLeft",
            "InteractiveElementRight",
        ],
    },
    Scrollbar: { color: { r: 170, g: 170, b: 170, a: 1 }, above: [] },
    AcceptButton: { color: { r: 33, g: 137, b: 228, a: 1 }, above: [] },
    CancelButton: { color: { r: 248, g: 78, b: 78, a: 1 }, above: [] },
    InteractiveElementText: {
        color: { r: 255, g: 255, b: 255, a: 1 },
        above: [],
    },
    AcceptCancelButtonText: {
        color: { r: 0, g: 0, b: 0, a: 1 },
        above: ["AcceptButton", "CancelButton"],
    },
    BackgroundHUD: { color: { r: 23, g: 23, b: 23, a: 1 }, above: [] },
    InteractiveHover: { color: { r: 150, g: 150, b: 150, a: 1 }, above: [] },
    InteractiveSelect: { color: { r: 100, g: 100, b: 100, a: 1 }, above: [] },
    Icon: {
        color: { r: 255, g: 255, b: 255, a: 1 },
        above: ["Background", "BackgroundSecondary", "InteractiveBackground"],
    },
    MainHUDIcon: {
        color: { r: 255, g: 255, b: 255, a: 1 },
        above: ["BackgroundHUD"],
    },
    MainHUDCloseIcon: {
        color: { r: 0, g: 0, b: 0, a: 1 },
        above: ["InteractiveElementRight", "#ffffff"],
    },
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

window.convertAuthToken = code => APS.convertAuthToken(code)

ReactDOM.createRoot(document.getElementById("root")!).render(
    <ThemeProvider initialThemeName={initialThemeName} themes={themes} defaultTheme={defaultColors}>
        <Synthesis />
    </ThemeProvider>
)
