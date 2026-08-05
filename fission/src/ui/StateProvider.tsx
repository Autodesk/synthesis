import type React from "react"
import { useMemo, useState, useReducer } from "react"
import type { AppMode } from "@/systems/AppMode"
import type { InputScheme } from "@/systems/input/InputTypes"
import * as UUID from "uuid"
import { StateContext, type StateProviderProps } from "./helpers/StateProviderHelpers"

function updateScheme(
    newScheme: InputScheme | undefined,
    previousScheme: InputScheme | undefined
): InputScheme | undefined {
    if (newScheme === undefined) return undefined
    if (previousScheme === undefined) return newScheme

    return {
        ...newScheme,
        customized: true,
        schemeId: previousScheme.customized ? previousScheme.schemeId : UUID.v4(),
    }
}

export const StateProvider: React.FC<StateProviderProps> = ({ children }) => {
    const [selectedScheme, setSelectedScheme] = useReducer(updateScheme, undefined)
    const [appMode, setAppMode] = useState<AppMode>("Configure")

    const stateContextValue = useMemo(
        () => ({
            selectedScheme,
            setSelectedScheme,
            appMode,
            setAppMode,
        }),
        [selectedScheme, appMode]
    )

    return <StateContext.Provider value={stateContextValue}>{children}</StateContext.Provider>
}
