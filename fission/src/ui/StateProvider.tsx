import type { InputScheme } from "@/systems/input/InputSchemeManager"
import type React from "react"
import { createContext, type ReactNode, useContext, useMemo, useState } from "react"
import { ConfigurationType, ConfigurePanelSettings } from "./panels/configuring/assembly-config/ConfigurePanel"

interface StateProviderProps {
    children: ReactNode
}

interface AppState {
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

export const StateProvider: React.FC<StateProviderProps> = ({ children }) => {
    const [unconfirmedImport, setUnconfirmedImport] = useState<boolean>(false)
    const [selectedScheme, setSelectedScheme] = useState<InputScheme | undefined>(undefined)
    const [configurePanelSettings, setConfigurePanelSettings] = useState<ConfigurePanelSettings | undefined>(undefined)
    const [configurationType, setConfigurationType] = useState<ConfigurationType>("ROBOTS")
    const [isMainMenuOpen, setIsMainMenuOpen] = useState<boolean>(true)

    const stateContextValue = useMemo(
        () => ({
            unconfirmedImport,
            setUnconfirmedImport,
            selectedScheme,
            setSelectedScheme,
            configurePanelSettings,
            setConfigurePanelSettings,
            configurationType,
            setConfigurationType,
            isMainMenuOpen,
            setIsMainMenuOpen,
        }),
        [unconfirmedImport, selectedScheme, configurePanelSettings, configurationType, isMainMenuOpen]
    )

    return <StateContext.Provider value={stateContextValue}>{children}</StateContext.Provider>
}
