import Panel, { PanelPropsImpl } from "@/components/Panel"
import DefaultInputs from "@/systems/input/DefaultInputs"
import InputSchemeManager, { InputScheme } from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import PreferencesSystem from "@/systems/preferences/PreferencesSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import Button from "@/ui/components/Button"
import Label, { LabelSize } from "@/ui/components/Label"
import { useModalControlContext } from "@/ui/ModalContext"
import { usePanelControlContext } from "@/ui/PanelContext"
import { Box, Divider, styled } from "@mui/material"
import { useEffect, useReducer } from "react"
import { AiOutlinePlus } from "react-icons/ai"
import { IoCheckmark, IoPencil, IoTrashBin } from "react-icons/io5"

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

    const AddIcon = <AiOutlinePlus size={"1.25rem"} />
    const DeleteIcon = <IoTrashBin size={"1.25rem"} />
    const SelectIcon = <IoCheckmark size={"1.25rem"} />
    const EditIcon = <IoPencil size={"1.25rem"} />

    const LabelStyled = styled(Label)({
        fontWeight: 700,
        margin: "0pt",
    })

    const DividerStyled = styled(Divider)({
        borderColor: "white",
    })

    useEffect(() => {
        closePanel("import-mirabuf")

        /** If the panel is closed before a scheme is selected, defaults to the top of the list */
        return () => {
            const brainIndex = getBrainIndex()
            console.log(brainIndex)

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
            cancelEnabled={false}
        >
            {/** A scroll view with buttons to select default and custom input schemes */}
            <div className="flex overflow-y-auto flex-col gap-2 min-w-[20vw] max-h-[45vh] bg-background-secondary rounded-md p-2">
                {/** The label and divider at the top of the scroll view */}
                <LabelStyled size={LabelSize.Medium} className="text-center mt-[4pt] mb-[2pt] mx-[5%]">
                    {`${InputSchemeManager.availableInputSchemes.length} Input Schemes`}
                </LabelStyled>
                <DividerStyled />

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
                            <LabelStyled>
                                {`${scheme.schemeName} | ${scheme.customized ? "Custom" : scheme.descriptiveName}`}
                            </LabelStyled>
                            <Box
                                component={"div"}
                                display={"flex"}
                                flexDirection={"row-reverse"}
                                gap={"0.25rem"}
                                justifyContent={"center"}
                                alignItems={"center"}
                            >
                                {/** Select button */}
                                <Button
                                    value={SelectIcon}
                                    onClick={() => {
                                        InputSystem.brainIndexSchemeMap.set(getBrainIndex(), scheme)
                                        closePanel(panelId)
                                    }}
                                    colorOverrideClass="bg-accept-button hover:brightness-90"
                                />
                                {/** Edit button - same as select but opens the inputs modal */}
                                <Button
                                    value={EditIcon}
                                    onClick={() => {
                                        InputSystem.brainIndexSchemeMap.set(getBrainIndex(), scheme)
                                        InputSystem.selectedScheme = scheme
                                        openModal("change-inputs")
                                    }}
                                    colorOverrideClass="bg-accept-button hover:brightness-90"
                                />
                                {/** Delete button (only if the scheme is customized) */}
                                {scheme.customized ? (
                                    <>
                                        <Button
                                            value={DeleteIcon}
                                            onClick={() => {
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
                                            }}
                                            colorOverrideClass="bg-cancel-button hover:brightness-90"
                                        />
                                    </>
                                ) : (
                                    <></>
                                )}
                            </Box>
                        </Box>
                    )
                })}
            </div>
            {/** New scheme with a randomly assigned name button */}
            <Button
                value={AddIcon}
                onClick={() => {
                    InputSystem.brainIndexSchemeMap.set(getBrainIndex(), DefaultInputs.newBlankScheme)
                    openModal("assign-new-scheme")
                }}
            />
            <Box height="12px"></Box>
        </Panel>
    )
}

export default ChooseInputSchemePanel
