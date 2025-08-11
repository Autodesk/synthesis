import { Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack, Typography } from "@mui/material"
import type React from "react"
import { useState } from "react"
import { globalAddToast } from "../components/GlobalUIControls"

interface DevtoolZoneRemovalModalProps {
    isOpen: boolean
    onClose: () => void
    zoneType: "scoring" | "protected"
    zoneName: string
    onTemporaryRemoval: () => void
    onPermanentRemoval: () => void
}

const DevtoolZoneRemovalModal: React.FC<DevtoolZoneRemovalModalProps> = ({
    isOpen,
    onClose,
    zoneType,
    zoneName,
    onTemporaryRemoval,
    onPermanentRemoval,
}) => {
    const [isRemoving, setIsRemoving] = useState(false)

    const handleTemporaryRemoval = () => {
        onTemporaryRemoval()
        onClose()
    }

    const handlePermanentRemoval = async () => {
        setIsRemoving(true)
        try {
            await onPermanentRemoval()
            globalAddToast?.("info", "Zone Removed", `${zoneName} has been permanently removed from the field file.`)
        } catch (error) {
            globalAddToast?.("error", "Removal Failed", "Failed to permanently remove zone from the field file cache.")
            console.error("Failed to remove zone from field file:", error)
        } finally {
            setIsRemoving(false)
            onClose()
        }
    }

    return (
        <Dialog open={isOpen} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>Remove {zoneType === "scoring" ? "Scoring" : "Protected"} Zone</DialogTitle>
            <DialogContent>
                <Stack spacing={2}>
                    <Typography variant="body1">
                        The {zoneType} zone "{zoneName}" was defined in the field file and is cached.
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                        Choose how you'd like to remove it:
                    </Typography>
                    <Stack spacing={1}>
                        <Typography variant="body2">
                            <strong>Temporary removal:</strong> Remove zone until next field reload. The zone will
                            reappear when you refresh or reload the field.
                        </Typography>
                        <Typography variant="body2">
                            <strong>Permanent removal:</strong> Remove zone from the field file cache. This will prevent it
                            from reappearing on future loads.
                        </Typography>
                    </Stack>
                </Stack>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose} disabled={isRemoving}>
                    Cancel
                </Button>
                <Button onClick={handleTemporaryRemoval} disabled={isRemoving} variant="outlined" color="warning">
                    Temporary Removal
                </Button>
                <Button onClick={handlePermanentRemoval} disabled={isRemoving} variant="contained" color="error">
                    {isRemoving ? "Removing..." : "Permanent Removal"}
                </Button>
            </DialogActions>
        </Dialog>
    )
}

export default DevtoolZoneRemovalModal
