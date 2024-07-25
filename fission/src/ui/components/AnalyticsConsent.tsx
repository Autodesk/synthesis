import { Box } from "@mui/material"
import Label, { LabelSize } from "./Label"
import Button from "./Button"
import { colorNameToVar } from "../ThemeContext"
import { AiOutlineClose } from "react-icons/ai"

interface AnalyticsConsentProps {
    onClose: () => void
    onConsent: () => void
}

function AnalyticsConsent({ onConsent, onClose }: AnalyticsConsentProps) {
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
                backgroundColor: colorNameToVar("Background"),
                padding: "1rem",
                borderRadius: "0.5rem",
                gap: "0.5rem"
            }}
        >
            <Label size={LabelSize.Small}>Synthesis uses cookies to improve the performance and quality of our app. Do you consent to the usage of cookies for tracking analytics data?</Label>
            <a target="_blank" rel="noopener noreferrer" href="https://synthesis.autodesk.com/data-collection/" className={`text-sm font-artifakt-normal`}>See here for more information</a>
            <Box
                component="div"
                display="flex"
                sx={{
                    flexDirection: "row-reverse",
                    gap: "0.5rem",
                    justifyContent: "space-between"
                }}
            >
                <Button value="I consent" onClick={() => onConsent()} />
                <Button value={<AiOutlineClose />} onClick={() => onClose()} sizeOverrideClass="h-full" colorOverrideClass="bg-background-secondary" />
            </Box>
        </Box>
    )
}

export default AnalyticsConsent