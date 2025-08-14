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
    actionType?: "removal" | "modification"
}

const DevtoolZoneRemovalModal: React.FC<DevtoolZoneRemovalModalProps> = ({
    isOpen,
    onClose,
    zoneType,
    zoneName,
    onTemporaryRemoval,
    onPermanentRemoval,
    actionType = "removal",
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
            const actionText = actionType === "modification" ? "modified" : "removed"
            const actionCapitalized = actionType === "modification" ? "Modified" : "Removed"
            globalAddToast?.(
                "info",
                `Zone ${actionCapitalized}`,
                `${zoneName} has been permanently ${actionText} in the field file.`
            )
        } catch (error) {
            const actionText = actionType === "modification" ? "modify" : "remove"
            globalAddToast?.(
                "error",
                `${actionType === "modification" ? "Modification" : "Removal"} Failed`,
                `Failed to permanently ${actionText} zone in the field file cache.`
            )
            console.error(`Failed to ${actionText} zone in field file:`, error)
        } finally {
            setIsRemoving(false)
            onClose()
        }
    }

    return (
        <Dialog open={isOpen} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>
                {actionType === "modification" ? "Modify" : "Remove"} {zoneType === "scoring" ? "Scoring" : "Protected"}{" "}
                Zone
            </DialogTitle>
            <DialogContent>
                <Stack spacing={2}>
                    <Typography variant="body1">
                        The {zoneType} zone "{zoneName}" was defined in the field file and is cached.
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                        Choose how you'd like to{" "}
                        {actionType === "modification" ? "save your modifications" : "remove it"}:
                    </Typography>
                    <Stack spacing={1}>
                        <Typography variant="body2">
                            <strong>Temporary {actionType === "modification" ? "modification" : "removal"}:</strong>{" "}
                            {actionType === "modification"
                                ? "Save changes until next field reload. Original zone will reappear when you refresh or reload the field."
                                : "Remove zone until next field reload. The zone will reappear when you refresh or reload the field."}
                        </Typography>
                        <Typography variant="body2">
                            <strong>Permanent {actionType === "modification" ? "modification" : "removal"}:</strong>{" "}
                            {actionType === "modification"
                                ? "Save changes to the field file cache. This will persist your modifications on future loads."
                                : "Remove zone from the field file cache. This will prevent it from reappearing on future loads."}
                        </Typography>
                    </Stack>
                </Stack>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose} disabled={isRemoving}>
                    Cancel
                </Button>
                <Button onClick={handleTemporaryRemoval} disabled={isRemoving} variant="outlined" color="warning">
                    Temporary {actionType === "modification" ? "Modification" : "Removal"}
                </Button>
                <Button
                    onClick={handlePermanentRemoval}
                    disabled={isRemoving}
                    variant="contained"
                    color={actionType === "modification" ? "primary" : "error"}
                >
                    {isRemoving
                        ? `${actionType === "modification" ? "Modifying" : "Removing"}...`
                        : `Permanent ${actionType === "modification" ? "Modification" : "Removal"}`}
                </Button>
            </DialogActions>
        </Dialog>
    )
}

export default DevtoolZoneRemovalModal
