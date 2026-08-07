import { Stack, TextField } from "@mui/material"
import type React from "react"
import { useEffect, useState } from "react"
import type { ModalImplProps } from "@/ui/components/Modal"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"

export type NameBuildModalCustomProps = {
    title: string
    acceptText: string
    defaultName: string
}

/** Prompts for a name, e.g. before finishing or exporting a mix-and-match build. */
const NameBuildModal: React.FC<ModalImplProps<string, NameBuildModalCustomProps>> = ({ modal }) => {
    const { configureScreen } = useUIContext()
    const [name, setName] = useState(modal?.props.custom.defaultName ?? "")

    const trimmed = name.trim()

    useEffect(() => {
        if (!modal) return

        configureScreen(
            modal,
            {
                title: modal.props.custom.title,
                acceptText: modal.props.custom.acceptText,
                disableAccept: trimmed === "",
            },
            { onBeforeAccept: () => trimmed }
        )
    }, [modal, configureScreen, trimmed])

    if (!modal) return null

    return (
        <Stack component="div" gap={1} sx={{ minWidth: 320 }}>
            <TextField
                autoFocus
                label="Name"
                value={name}
                onChange={e => setName(e.target.value)}
                error={trimmed === ""}
                helperText={trimmed === "" ? "Name cannot be empty" : ""}
            />
        </Stack>
    )
}

export default NameBuildModal
