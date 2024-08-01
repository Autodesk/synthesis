import Panel, { PanelPropsImpl } from "@/components/Panel"
import DefaultInputs from "@/systems/input/DefaultInputs"
import InputSchemeManager, { InputScheme } from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import { LabelSize } from "@/ui/components/Label"
import {
    EditButton,
    DeleteButton,
    SelectButton,
    AddButtonInteractiveColor,
    SectionLabel,
    SectionDivider,
    SynthesisIcons,
} from "@/ui/components/StyledComponents"
import { useModalControlContext } from "@/ui/ModalContext"
import { usePanelControlContext } from "@/ui/PanelContext"
import { Box } from "@mui/material"
import { useEffect, useReducer } from "react"

let selectedBrainIndexGlobal: number | undefined = undefined
// eslint-disable-next-line react-refresh/only-export-components
export function setSelectedBrainIndexGlobal(index: number | undefined) {
    selectedBrainIndexGlobal = index
}

function getBrainIndex() {
    return selectedBrainIndexGlobal != undefined ? selectedBrainIndexGlobal : SynthesisBrain.brainIndexMap.size - 1
}

const ChooseInputSchemePanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { closePanel } = usePanelControlContext()
    const { openModal } = useModalControlContext()

    const [_, update] = useReducer(x => !x, false)

    useEffect(() => {
        closePanel("import-mirabuf")

        /** If the panel is closed before a scheme is selected, defaults to the top of the list */
        return () => {
            const brainIndex = getBrainIndex()

            if (InputSystem.brainIndexSchemeMap.has(brainIndex)) return

            const scheme = InputSchemeManager.availableInputSchemes[0]

            InputSystem.brainIndexSchemeMap.set(brainIndex, scheme)
            InputSystem.selectedScheme = scheme

            openModal("change-inputs")
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [])

    useEffect(() => {
        return () => {
            selectedBrainIndexGlobal = undefined
        }
    }, [])

    return (
        <Panel
            name="Choose Input Scheme"
            panelId={panelId}
            openLocation={"right"}
            sidePadding={8}
            acceptEnabled={false}
            icon={SynthesisIcons.Gamepad}
            cancelEnabled={selectedBrainIndexGlobal != undefined}
            cancelName="Close"
        >
            {/** A scroll view with buttons to select default and custom input schemes */}
            <div className="flex overflow-y-auto flex-col gap-2 min-w-[20vw] max-h-[45vh] bg-background-secondary rounded-md p-2">
                {/** The label and divider at the top of the scroll view */}
                <SectionLabel size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                    {`${InputSchemeManager.availableInputSchemes.length} Input Schemes`}
                </SectionLabel>
                <SectionDivider />

                {/** Creates list items with buttons */}
                {InputSchemeManager.availableInputSchemes.map(scheme => {
                    return (
                        <Box
                            component={"div"}
                            display={"flex"}
                            justifyContent={"space-between"}
                            alignItems={"center"}
                            gap={"1rem"}
                            key={scheme.schemeName}
                        >
                            <SectionLabel>
                                {`${scheme.schemeName} | ${scheme.customized ? "Custom" : scheme.descriptiveName}`}
                            </SectionLabel>
                            <Box
                                component={"div"}
                                display={"flex"}
                                flexDirection={"row-reverse"}
                                gap={"0.25rem"}
                                justifyContent={"center"}
                                alignItems={"center"}
                            >
                                {/** Select button */}
                                {SelectButton(() => {
                                    InputSystem.brainIndexSchemeMap.set(getBrainIndex(), scheme)
                                    closePanel(panelId)
                                })}
                                {/** Edit button - same as select but opens the inputs modal */}
                                {EditButton(() => {
                                    InputSystem.brainIndexSchemeMap.set(getBrainIndex(), scheme)
                                    InputSystem.selectedScheme = scheme
                                    openModal("change-inputs")
                                })}

                                {/** Delete button (only if the scheme is customized) */}
                                {scheme.customized ? (
                                    DeleteButton(() => {
                                        // Fetch current custom schemes
                                        InputSchemeManager.saveSchemes()
                                        InputSchemeManager.resetDefaultSchemes()
                                        const schemes =
                                            PreferencesSystem.getGlobalPreference<InputScheme[]>("InputSchemes")

                                        // Find and remove this input scheme
                                        const index = schemes.indexOf(scheme)
                                        schemes.splice(index, 1)

                                        // Save to preferences
                                        PreferencesSystem.setGlobalPreference("InputSchemes", schemes)
                                        PreferencesSystem.savePreferences()

                                        update()
                                    })
                                ) : (
                                    <></>
                                )}
                            </Box>
                        </Box>
                    )
                })}
            </div>
            {/** New scheme with a randomly assigned name button */}
            {AddButtonInteractiveColor(() => {
                InputSystem.brainIndexSchemeMap.set(getBrainIndex(), DefaultInputs.newBlankScheme)
                openModal("assign-new-scheme")
            })}

            <Box height="12px"></Box>
        </Panel>
    )
}

export default ChooseInputSchemePanel
