import ReactDOM from "react-dom/client"
import Synthesis from "./Synthesis"
import "./index.css"
import APS from "./aps/APS"

window.convertAuthToken = (code, state) => APS.convertAuthToken(code, state)
ReactDOM.createRoot(document.getElementById("root")!).render(<Synthesis />)
