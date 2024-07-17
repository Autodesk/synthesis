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
                {/** A scroll view with buttons to select default and custom input schemes */}
                {InputSchemeManager.availableInputSchemes.map(scheme => {
                    return (
                        <Button
                            value={`${scheme.schemeName} | ${scheme.customized ? "Custom" : scheme.descriptiveName}`}
                            onClick={() => {
                                InputSystem.brainIndexSchemeMap.set(SynthesisBrain.brainIndexMap.size - 1, scheme)
                                closePanel(panelId)
                            }}
                            key={scheme.schemeName}
                        />
                    )
                })}
            </div>
            {/** A button to create a new scheme with a randomly assigned name */}
            <Button
                value={AddIcon}
                onClick={() => {
                    // Assign a blank input scheme a random name
                    const name = InputSchemeManager.randomAvailableName
                    const scheme = DefaultInputs.newBlankScheme
                    scheme.schemeName = name

                    InputSystem.brainIndexSchemeMap.set(SynthesisBrain.brainIndexMap.size - 1, scheme)

                    closePanel(panelId)
                }}
            />
            <Box height="12px"></Box>
        </Panel>
    )
}

export default ChooseInputSchemePanel
