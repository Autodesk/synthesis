import type React from "react"
import { useMemo, useState } from "react"
import type { InputScheme } from "@/systems/input/InputTypes"
import { StateContext, type StateProviderProps } from "./helpers/StateProviderHelpers"

export const StateProvider: React.FC<StateProviderProps> = ({ children }) => {
    const [unconfirmedImport, setUnconfirmedImport] = useState<boolean>(false)
    const [selectedScheme, setSelectedScheme] = useState<InputScheme | undefined>(undefined)
    const [isMainMenuOpen, setIsMainMenuOpen] = useState<boolean>(true)

    const stateContextValue = useMemo(
        () => ({
            unconfirmedImport,
            setUnconfirmedImport,
            selectedScheme,
            setSelectedScheme,
            isMainMenuOpen,
            setIsMainMenuOpen,
        }),
        [unconfirmedImport, selectedScheme, isMainMenuOpen]
    )

    return <StateContext.Provider value={stateContextValue}>{children}</StateContext.Provider>
}
