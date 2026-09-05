import { fireEvent, render, waitFor } from "@testing-library/react"
import { SnackbarProvider } from "notistack"
import type React from "react"
import { useCallback, useEffect, useMemo, useState } from "react"
import { afterEach, describe, expect, test } from "vitest"
import { UICallback } from "@/ui/UICallbacks"
import { UIProvider } from "@/ui/UIProvider"
import { type CloseType, type Modal, useUIContext } from "@/ui/helpers/UIProviderHelpers"

export const ConfigurableScreen: React.FC = () => {
    const { configureScreen, openPanel } = useUIContext()
    const [hasSelectedFile, setHasSelectedFile] = useState(false)
    const screen = useMemo<Modal<unknown, object>>(
        () => ({
            id: "import-screen",
            content: () => null,
            props: {
                type: "modal",
                configured: false,
                custom: {},
            },
            onClose: new UICallback<[CloseType], void>(),
            onCancel: new UICallback<[], void>(),
            onAccept: new UICallback<[unknown], void>(),
            onBeforeAccept: new UICallback<[], unknown>(),
        }),
        []
    )

    const onBeforeAccept = useCallback(() => {
        if (!openPanel) return
    }, [openPanel])
    const onCancel = useCallback(() => {
        if (!openPanel) return
    }, [openPanel])
    const onFileSelected = useCallback(() => setHasSelectedFile(true), [])

    useEffect(() => {
        configureScreen(screen, { hideAccept: !hasSelectedFile }, { onBeforeAccept, onCancel })
    }, [configureScreen, hasSelectedFile, onBeforeAccept, onCancel, screen])

    return (
        <>
            <label>
                Import file
                <input type="file" onChange={onFileSelected} />
            </label>
            {!screen.props.hideAccept && <button>Accept</button>}
        </>
    )
}

describe("UIProvider import screen configuration", () => {
    afterEach(() => {
        document.body.replaceChildren()
    })

    test("shows Accept after a file selection without a render loop", async () => {
        const { getByLabelText, getByRole, queryByRole } = render(
            <SnackbarProvider>
                <UIProvider>
                    <ConfigurableScreen />
                </UIProvider>
            </SnackbarProvider>
        )

        expect(queryByRole("button", { name: "Accept" })).toBeNull()
        const file = new File(["kitbot"], "Kitbot.2026.zip", { type: "application/zip" })
        fireEvent.change(getByLabelText("Import file"), { target: { files: [file] } })

        await waitFor(() => expect(getByRole("button", { name: "Accept" })).toBeInTheDocument())
    })
})
