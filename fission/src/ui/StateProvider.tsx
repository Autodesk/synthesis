import type React from "react"
import { createContext, type ReactNode, useContext, useMemo, useState } from "react"
import type { InputScheme } from "@/systems/input/InputSchemeManager"
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
})

export const useStateContext = () => useContext(StateContext)

export const StateProvider: React.FC<StateProviderProps> = ({ children }) => {
    const [unconfirmedImport, setUnconfirmedImport] = useState<boolean>(false)
    const [selectedScheme, setSelectedScheme] = useState<InputScheme | undefined>(undefined)
    const [configurePanelSettings, setConfigurePanelSettings] = useState<ConfigurePanelSettings | undefined>(undefined)
    const [configurationType, setConfigurationType] = useState<ConfigurationType>("ROBOTS")

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
        }),
        [unconfirmedImport, selectedScheme, configurePanelSettings, configurationType]
    )

    return <StateContext.Provider value={stateContextValue}>{children}</StateContext.Provider>
}
