import { createContext, type ReactNode, useContext } from "react"
import type { AppMode } from "@/systems/AppMode"
import type { InputScheme } from "@/systems/input/InputTypes"

export interface StateProviderProps {
    children: ReactNode
}

export interface AppState {
    // ConfigureInputs stuff
    selectedScheme?: InputScheme
    setSelectedScheme: (_scheme: InputScheme | undefined) => void
    // Top bar mode selector
    appMode: AppMode
    setAppMode: (_mode: AppMode) => void
}

export const StateContext = createContext<AppState>({
    selectedScheme: undefined,
    setSelectedScheme: () => {},
    appMode: "Configure",
    setAppMode: () => {},
})

export const useStateContext = () => useContext(StateContext)
