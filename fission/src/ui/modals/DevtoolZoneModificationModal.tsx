import { Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack, Typography } from "@mui/material"
import type React from "react"
import { useState } from "react"
import { globalAddToast } from "../components/GlobalUIControls"

interface DevtoolZoneModificationModalProps {
    isOpen: boolean
    onClose: () => void
    zoneType: "scoring" | "protected"
    zoneName: string
    onTemporaryModification: () => void
    onPermanentModification: () => void
}

const DevtoolZoneModificationModal: React.FC<DevtoolZoneModificationModalProps> = ({
    isOpen,
    onClose,
    zoneType,
    zoneName,
    onTemporaryModification,
    onPermanentModification,
}) => {
    const [isModifying, setIsModifying] = useState(false)

    const handleTemporaryModification = () => {
        onTemporaryModification()
        onClose()
    }

    const handlePermanentModification = async () => {
        setIsModifying(true)
        try {
            await onPermanentModification()
            globalAddToast?.("info", "Zone Modified", `${zoneName} has been permanently modified in the field file.`)
        } catch (error) {
            globalAddToast?.(
                "error",
                "Modification Failed",
                "Failed to permanently modify zone in the field file cache."
            )
            console.error("Failed to modify zone in field file:", error)
        } finally {
            setIsModifying(false)
            onClose()
        }
    }

    return (
        <Dialog open={isOpen} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>Modify {zoneType === "scoring" ? "Scoring" : "Protected"} Zone</DialogTitle>
            <DialogContent>
                <Stack spacing={2}>
                    <Typography variant="body1">
                        The {zoneType} zone "{zoneName}" was defined in the field file and is cached.
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                        Choose how you'd like to save your modifications:
                    </Typography>
                    <Stack spacing={1}>
                        <Typography variant="body2">
                            <strong>Temporary modification:</strong> Save changes until next field reload. Original zone
                            will reappear when you refresh the page.
                        </Typography>
                        <Typography variant="body2">
                            <strong>Permanent modification:</strong> Save changes to the local asset file. This will
                            persist your modifications until you remove it from the cache.
                        </Typography>
                    </Stack>
                </Stack>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose} disabled={isModifying}>
                    Cancel
                </Button>
                <Button onClick={handleTemporaryModification} disabled={isModifying} variant="outlined" color="warning">
                    Temporary Modification
                </Button>
                <Button
                    onClick={handlePermanentModification}
                    disabled={isModifying}
                    variant="contained"
                    color="primary"
                >
                    {isModifying ? "Modifying..." : "Permanent Modification"}
                </Button>
            </DialogActions>
        </Dialog>
    )
}

export default DevtoolZoneModificationModal
