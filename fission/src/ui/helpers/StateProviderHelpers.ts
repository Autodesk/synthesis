import { createContext, ReactNode, useContext } from "react"
import type { InputScheme } from "@/systems/input/InputTypes"
import type { ConfigurationType } from "../panels/configuring/assembly-config/ConfigTypes"
import { ConfigurePanelSettings } from "../panels/configuring/assembly-config/ConfigurePanel"

export interface StateProviderProps {
    children: ReactNode
}

export interface AppState {
    // ImportMirabufPanel
    unconfirmedImport: boolean
    setUnconfirmedImport: (_state: boolean) => void
    // ConfigureInputs stuff
    selectedScheme?: InputScheme
    setSelectedScheme: (_scheme: InputScheme) => void
    // Configure Panel
    configurePanelSettings?: ConfigurePanelSettings
    setConfigurePanelSettings: (_settings?: ConfigurePanelSettings) => void
    configurationType: ConfigurationType
    setConfigurationType: (_type: ConfigurationType) => void
    // View Cube
    isMainMenuOpen: boolean
    setIsMainMenuOpen: (_state: boolean) => void
}

export const StateContext = createContext<AppState>({
    unconfirmedImport: false,
    setUnconfirmedImport: () => {},
    selectedScheme: undefined,
    setSelectedScheme: () => {},
    configurePanelSettings: undefined,
    setConfigurePanelSettings: () => {},
    configurationType: "ROBOTS",
    setConfigurationType: () => {},
    isMainMenuOpen: true,
    setIsMainMenuOpen: () => {},
})

export const useStateContext = () => useContext(StateContext)
