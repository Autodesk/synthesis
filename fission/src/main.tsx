import ReactDOM from "react-dom/client"
import "./index.css"
import APS from "./aps/APS"
import { App } from "./App"

window.convertAuthToken = code => APS.convertAuthToken(code)

ReactDOM.createRoot(document.getElementById("root")!).render(
    <App />
)
