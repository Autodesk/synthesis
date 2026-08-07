import { render } from "@testing-library/react"
import { describe, expect, test } from "vitest"
import TopBar from "@/ui/components/TopBar"
import { UIContext, type UIContextProps } from "@/ui/helpers/UIProviderHelpers"
import { type AppState, StateContext } from "@/ui/helpers/StateProviderHelpers.ts"
import { APP_MODES } from "@/systems/AppMode.ts"

const BLOCKED_CONTEXT: UIContextProps = {
    panels: [],
    blockState: { blocked: true, blockMessage: "" },
    openModal: () => null,
    openPanel: async () => null,
    togglePanel: async () => null,
    closeModal: () => {},
    closePanel: () => {},
    addToast: () => {},
    configureScreen: () => {},
}

describe("top bar blocking", () => {
    test.for(APP_MODES)("interactive elements disabled ($0)", mode => {
        const stateContextValue: AppState = {
            setSelectedScheme: () => {},
            appMode: mode,
            setAppMode: () => {},
        }

        const { container } = render(
            <UIContext.Provider value={BLOCKED_CONTEXT}>
                <StateContext.Provider value={stateContextValue}>
                    <TopBar />
                </StateContext.Provider>
            </UIContext.Provider>
        )

        const elements = [...container.querySelectorAll("button"), ...container.querySelectorAll("input")]
        expect(elements).not.toHaveLength(0)
        for (const el of elements) {
            expect(el).toBeDisabled()
        }
    })
})
