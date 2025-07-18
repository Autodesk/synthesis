import ReactDOM from "react-dom/client"
import { ThemeProvider } from "@/ui/ThemeContext"
import Synthesis from "./Synthesis"
import "./index.css"
import APS from "./aps/APS"
import { initialThemeName, themes, defaultColors } from "@/theme.ts"


window.convertAuthToken = code => APS.convertAuthToken(code)

ReactDOM.createRoot(document.getElementById("root")!).render(
    <ThemeProvider initialThemeName={initialThemeName} themes={themes} defaultTheme={defaultColors}>
        <Synthesis />
    </ThemeProvider>
)
