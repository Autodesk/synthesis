import Panel, { PanelPropsImpl } from "@/components/Panel"
import DefaultInputs from "@/systems/input/DefaultInputs"
import InputSchemeManager from "@/systems/input/InputSchemeManager"
import InputSystem from "@/systems/input/InputSystem"
import SynthesisBrain from "@/systems/simulation/synthesis_brain/SynthesisBrain"
import Button from "@/ui/components/Button"
import { usePanelControlContext } from "@/ui/PanelContext"
import { Box } from "@mui/material"
import { useEffect } from "react"
import { AiOutlinePlus } from "react-icons/ai"

const ChooseInputSchemePanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { closePanel } = usePanelControlContext()
    const AddIcon = <AiOutlinePlus size={"1.25rem"} />

    useEffect(() => {
        closePanel("import-mirabuf")
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
            <div className="flex overflow-y-auto flex-col gap-2 min-w-[20vw] max-h-[20vh] bg-background-secondary rounded-md p-2">
                {InputSchemeManager.AVAILABLE_INPUT_SCHEMES.map(scheme => {
                    return (
                        <Button
                            value={`${scheme.schemeName} | ${scheme.descriptiveName}`}
                            onClick={() => {
                                // Remove the chosen scheme from the list
                                //const index = InputSchemeManager.AVAILABLE_INPUT_SCHEMES.indexOf(scheme)
                                //InputSchemeManager.AVAILABLE_INPUT_SCHEMES.splice(index, 1)

                                InputSystem.brainIndexSchemeMap.set(SynthesisBrain.currentBrainIndex - 1, scheme)

                                closePanel(panelId)
                            }}
                            key={scheme.schemeName}
                        />
                    )
                })}
            </div>
            <Button
                value={AddIcon}
                onClick={() => {
                    // Find a random name and remove it from the list
                    const index = Math.floor(Math.random() * DefaultInputs.NAMES.length)
                    const schemeName = DefaultInputs.NAMES[index]
                    DefaultInputs.NAMES.splice(index, 1)

                    // TODO: Create a new blank scheme for this name

                    // SynthesisBrain.triggerBrainConfiguredEvent(schemeName)
                    closePanel(panelId)
                }}
            />
            <Box height="12px"></Box>
        </Panel>
    )
}

export default ChooseInputSchemePanel
