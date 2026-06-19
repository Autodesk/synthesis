import type React from "react"
import { useMemo, useState } from "react"
import type MirabufSceneObject from "@/mirabuf/MirabufSceneObject"
import type { AppMode } from "@/systems/AppMode"
import type { InputScheme } from "@/systems/input/InputTypes"
import { StateContext, type StateProviderProps } from "./helpers/StateProviderHelpers"

export const StateProvider: React.FC<StateProviderProps> = ({ children }) => {
    const [unconfirmedImport, setUnconfirmedImport] = useState<boolean>(false)
    const [selectedScheme, setSelectedScheme] = useState<InputScheme | undefined>(undefined)
    const [isMainMenuOpen, setIsMainMenuOpen] = useState<boolean>(true)
    const [appMode, setAppMode] = useState<AppMode>("Configure")
    const [selectedConfigAssembly, setSelectedConfigAssembly] = useState<MirabufSceneObject | undefined>(undefined)

    const stateContextValue = useMemo(
        () => ({
            unconfirmedImport,
            setUnconfirmedImport,
            selectedScheme,
            setSelectedScheme,
            isMainMenuOpen,
            setIsMainMenuOpen,
            appMode,
            setAppMode,
            selectedConfigAssembly,
            setSelectedConfigAssembly,
        }),
        [unconfirmedImport, selectedScheme, isMainMenuOpen, appMode, selectedConfigAssembly]
    )

    return <StateContext.Provider value={stateContextValue}>{children}</StateContext.Provider>
}
