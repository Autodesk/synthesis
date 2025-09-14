import ReactDOM from "react-dom/client"
import Synthesis from "./Synthesis"
import "./index.css"
import APS from "./aps/APS"

window.convertAuthToken = code => APS.convertAuthToken(code)
ReactDOM.createRoot(document.getElementById("root")!).render(<Synthesis />)
