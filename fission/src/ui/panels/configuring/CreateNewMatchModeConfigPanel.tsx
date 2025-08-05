import { PanelImplProps } from "@/ui/components/Panel"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers"
import { Box } from "@mui/material"
import { useEffect } from "react"

const CreateNewMatchModeConfigPanel: React.FC<PanelImplProps<void, void>> = ({ panel }) => {
    const { configureScreen } = useUIContext()

    useEffect(() => {
        configureScreen(panel!, { title: "Create New Match Mode Config", hideAccept: true, cancelText: "Close" }, {})
    }, [])

    return (
        <Box
            component="div"
            alignItems="center"
            sx={{
                padding: "0.25rem",
                overflowY: "auto",
                borderRadius: "0.5rem",
            }}
            justifyContent="center"
            textAlign="center"
            minWidth="290px"
        ></Box>
    )
}

export default CreateNewMatchModeConfigPanel
