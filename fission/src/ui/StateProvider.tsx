import type React from "react"
import { useMemo, useState } from "react"
import type { InputScheme } from "@/systems/input/InputTypes"
import { StateContext, StateProviderProps } from "./helpers/StateProviderHelpers"
import { ConfigurePanelSettings } from "./panels/configuring/assembly-config/ConfigurePanel"
import { ConfigurationType } from "./panels/configuring/assembly-config/ConfigTypes"

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
