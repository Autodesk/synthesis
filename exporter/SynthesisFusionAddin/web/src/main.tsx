import { StrictMode } from "react"
import { createRoot } from "react-dom/client"
import "./index.css"
import App from "./App.tsx"
import { AppContainer } from "./AppContainer.tsx"

// biome-ignore lint/style/noNonNullAssertion: can't do anything if it's not there
createRoot(document.getElementById("root")!).render(
    <StrictMode>
        <AppContainer>
            <App />
        </AppContainer>
    </StrictMode>
)
