import type { ConfigurationSubpanelComponent } from "@/panels/configuring/assembly-config/ConfigTypes.ts"
import InputSystem from "@/systems/input/InputSystem.ts"
import { Button } from "@/components/StyledComponents.tsx"
import { setSpotlightAssembly } from "@/mirabuf/MirabufSceneObject.ts"
import ChooseInputSchemePanel from "@/panels/configuring/ChooseInputSchemePanel.tsx"
import { CloseType, useUIContext } from "@/ui/helpers/UIProviderHelpers.ts"
import ConfigureSchemeInterface from "@/panels/configuring/assembly-config/interfaces/inputs/ConfigureSchemeInterface.tsx"
import { useEffect, useMemo, useState } from "react"
import type SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain.ts"
import { Stack } from "@mui/material"
import ConfirmChangesModal from "@/modals/configuring/ConfirmChangesModal.tsx"

const ControlsConfigInterface: ConfigurationSubpanelComponent = ({
    selectedAssembly,
    panel,
    registerCleanupFunction,
    hasMadeChanges,
}) => {
    console.assert(selectedAssembly.brain?.isSynthesis())

    const [isEditing, setIsEditing] = useState<boolean>(false)
    const brainIndex = useMemo(() => (selectedAssembly.brain as SynthesisBrain).brainIndex, [selectedAssembly])

    const [scheme, setScheme] = useState(InputSystem.getBrainIndexSchemeMapping(brainIndex))
    useEffect(() => {
        setScheme(InputSystem.getBrainIndexSchemeMapping(brainIndex))
    }, [brainIndex])
    useEffect(() => {
        if (scheme === undefined) return
        InputSystem.setBrainIndexSchemeMapping(brainIndex, scheme)
    }, [scheme])

    const { openPanel, closePanel, openModal } = useUIContext()
    return (
        <Stack spacing={1} direction={"column"} alignItems={"center"}>
            <Stack spacing={1} direction={"row"}>
                <Button
                    onClick={() => {
                        if (hasMadeChanges) {
                            openModal(ConfirmChangesModal, undefined, panel, {
                                onAccept: () => {
                                    setSpotlightAssembly(selectedAssembly)
                                    openPanel(ChooseInputSchemePanel, undefined, panel)
                                    closePanel(panel.id, CloseType.OVERWRITE)
                                },
                            })
                        } else {
                            setSpotlightAssembly(selectedAssembly)
                            openPanel(ChooseInputSchemePanel, undefined, panel)
                            closePanel(panel.id, CloseType.ACCEPT)
                        }
                    }}
                >
                    Set Scheme
                </Button>
                {scheme && !isEditing && (
                    <Button
                        onClick={() => {
                            setIsEditing(true)
                        }}
                    >
                        Edit Scheme Bindings
                    </Button>
                )}
            </Stack>
            {isEditing && scheme && (
                <ConfigureSchemeInterface
                    registerCleanupFunction={registerCleanupFunction}
                    selectedScheme={scheme}
                    setSelectedScheme={setScheme}
                    panelId={panel?.id}
                />
            )}
        </Stack>
    )
}
export default ControlsConfigInterface
