import { Box } from "@mui/material"
import { Button } from "./StyledComponents"
import { AiOutlineClose } from "react-icons/ai"
import Label from "./Label"

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
                bgcolor: "background.default",
                padding: "1rem",
                borderRadius: "0.5rem",
                gap: "0.5rem",
            }}
        >
            <Label size="sm">
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
                <Button startIcon={<AiOutlineClose />} onClick={() => onClose()} color="secondary" />
            </Box>
        </Box>
    )
}

export default AnalyticsConsent
