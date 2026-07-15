import { createContext, type ReactNode, useContext } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
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
    // Assembly selected in the top bar's Configure controls
    selectedConfigAssembly?: MirabufSceneObject
    setSelectedConfigAssembly: (_assembly: MirabufSceneObject | undefined) => void
}

export const StateContext = createContext<AppState>({
    selectedScheme: undefined,
    setSelectedScheme: () => {},
    appMode: "Configure",
    setAppMode: () => {},
    selectedConfigAssembly: undefined,
    setSelectedConfigAssembly: () => {},
})

export const useStateContext = () => useContext(StateContext)
