import Panel, { PanelPropsImpl } from "../components/Panel"
import { SynthesisIcons } from "../components/StyledComponents"

const DeveloperToolPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {
    return (
        <Panel
            name={"Developer Tool"}
            icon={SynthesisIcons.CodeSquare}
            panelId={panelId}
            acceptEnabled={true}
            cancelEnabled={true}
            onAccept={() => {

            }}
            onCancel={() => {

            }}
            acceptName="Accept"
            cancelName="Cancel"
        >

        </Panel>
    )
}

export default DeveloperToolPanel