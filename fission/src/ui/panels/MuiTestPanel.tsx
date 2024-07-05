import Panel, { PanelPropsImpl } from "@/components/Panel"
import { Button } from "@mui/material"
import { AiOutlineTool } from "react-icons/ai"

const MuiTestPanel: React.FC<PanelPropsImpl> = ({ panelId }) => {

    console.debug('Mui Test Panel')

    return (
        <Panel
            name={"MultiBot"}
            icon={<AiOutlineTool />}
            panelId={panelId}
            openLocation="right"
        >
            <Button
                size='small'
            >
                Sample Small
            </Button>
            <Button
                size='medium'
            >
                Sample Medium
            </Button>
            <Button
                size='large'
            >
                Sample Large
            </Button>
        </Panel>
    )
}

export default MuiTestPanel
