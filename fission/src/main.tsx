import ReactDOM from "react-dom/client"
import { ThemeProvider } from "@/ui/ThemeContext"
import Synthesis from "./Synthesis"
import "./index.css"
import { defaultColors, initialThemeName, themes } from "@/theme.ts"
import APS from "./aps/APS"

window.convertAuthToken = code => APS.convertAuthToken(code)

ReactDOM.createRoot(document.getElementById("root")!).render(
    <ThemeProvider initialThemeName={initialThemeName} themes={themes} defaultTheme={defaultColors}>
        <Synthesis />
    </ThemeProvider>
)
