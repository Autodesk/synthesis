import Panel, { PanelPropsImpl } from "@/components/Panel"
import Label from "@/ui/components/Label"
import { usePanelControlContext } from "@/ui/PanelContext"
import { useEffect } from "react"

const ChooseInputSchemePanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    const { closePanel } = usePanelControlContext()

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
            <Label>Test</Label>
        </Panel>
    )
}

export default ChooseInputSchemePanel
