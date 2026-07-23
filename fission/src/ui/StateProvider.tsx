import type React from "react"
import { useMemo, useState } from "react"
import type { AppMode } from "@/systems/AppMode"
import type { InputScheme } from "@/systems/input/InputTypes"
import { StateContext, type StateProviderProps } from "./helpers/StateProviderHelpers"
import { useSpawnedAssemblies } from "./helpers/UseSpawnedAssemblies"

export const StateProvider: React.FC<StateProviderProps> = ({ children }) => {
    const [selectedScheme, setSelectedScheme] = useState<InputScheme | undefined>(undefined)
    const [appMode, setAppMode] = useState<AppMode>("Configure")
    const { assemblies, selectedConfigAssembly, setSelectedConfigAssembly } = useSpawnedAssemblies()

    const stateContextValue = useMemo(
        () => ({
            selectedScheme,
            setSelectedScheme,
            appMode,
            setAppMode,
            assemblies,
            selectedConfigAssembly,
            setSelectedConfigAssembly,
        }),
        [selectedScheme, appMode, assemblies, selectedConfigAssembly, setSelectedConfigAssembly]
    )

    return <StateContext.Provider value={stateContextValue}>{children}</StateContext.Provider>
}
