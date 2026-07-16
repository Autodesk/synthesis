import { createContext, type ReactNode, useContext } from "react"
import type { InputScheme } from "@/systems/input/InputTypes"

export interface StateProviderProps {
    children: ReactNode
}

export interface AppState {
    // ConfigureInputs stuff
    selectedScheme?: InputScheme
    setSelectedScheme: (_scheme: InputScheme | undefined) => void
    // View Cube
    isMainMenuOpen: boolean
    setIsMainMenuOpen: (_state: boolean) => void
}

export const StateContext = createContext<AppState>({
    selectedScheme: undefined,
    setSelectedScheme: () => {},
    isMainMenuOpen: true,
    setIsMainMenuOpen: () => {},
})

export const useStateContext = () => useContext(StateContext)
