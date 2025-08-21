import { Box } from "@mui/material"
import { AiOutlineClose } from "react-icons/ai"
import Label from "./Label"
import { Button } from "./StyledComponents"

interface AnalyticsConsentProps {
    onClose: () => void
    onConsent: () => void
}

const AnalyticsConsent: React.FC<AnalyticsConsentProps> = ({ onConsent, onClose }) => {
    return (
        <Box
            component="div"
            display="flex"
            sx={{
                flexDirection: "column",
                maxWidth: "300pt",
                position: "fixed",
                right: "0.5rem",
                bottom: "0.5rem",
                bgcolor: "background.paper",
                padding: "1rem",
                borderRadius: "0.5rem",
                gap: "0.5rem",
                boxShadow: 6,
            }}
        >
            <Label size="sm" color="text.primary">
                Synthesis uses cookies to improve the performance and quality of our app. Do you consent to the usage of
                cookies for tracking analytics data?
            </Label>
            <a
                target="_blank"
                rel="noopener noreferrer"
                href="https://synthesis.autodesk.com/data-collection/"
                className="text-sm font-artifakt-normal"
            >
                See here for more information
            </a>
            <Box
                component="div"
                display="flex"
                sx={{
                    flexDirection: "row-reverse",
                    gap: "0.5rem",
                    justifyContent: "space-between",
                }}
            >
                <Button onClick={() => onConsent()}>I consent</Button>
                <Button
                    onClick={() => onClose()}
                    color="error"
                    sx={{
                        minWidth: 0,
                        width: 36,
                        height: 36,
                        borderRadius: "50%",
                        alignItems: "center",
                        justifyContent: "center",
                        display: "flex",
                        p: 0,
                        bgcolor: theme => theme.palette.action.hover,
                        "&:hover": { bgcolor: theme => theme.palette.action.selected },
                        color: theme => theme.palette.error.main,
                    }}
                >
                    <AiOutlineClose />
                </Button>
            </Box>
        </Box>
    )
}

export default AnalyticsConsent
