import { Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack, Typography } from "@mui/material"
import type React from "react"
import { useState } from "react"
import { globalAddToast } from "../components/GlobalUIControls"

interface DevtoolZoneModificationModalProps {
    isOpen: boolean
    onClose: () => void
    zoneType: "scoring" | "protected"
    zoneName: string
    mode: "remove" | "modify"
    onTemporaryAction: () => void
    onPermanentAction: () => Promise<void>
}

const DevtoolZoneModificationModal: React.FC<DevtoolZoneModificationModalProps> = ({
    isOpen,
    onClose,
    zoneType,
    zoneName,
    mode,
    onTemporaryAction,
    onPermanentAction,
}) => {
    const [isPending, setIsPending] = useState(false)

    const isRemove = mode === "remove"
    const zoneLabel = zoneType === "scoring" ? "Scoring" : "Protected"
    const actionLabel = isRemove ? "Removal" : "Modification"

    const handleTemporaryAction = () => {
        onTemporaryAction()
        onClose()
    }

    const handlePermanentAction = async () => {
        setIsPending(true)
        try {
            await onPermanentAction()
            globalAddToast?.(
                "info",
                isRemove ? "Zone Removed" : "Zone Modified",
                `${zoneName} has been permanently ${isRemove ? "removed from" : "modified in"} the field file.`
            )
        } catch (error) {
            globalAddToast?.(
                "error",
                isRemove ? "Removal Failed" : "Modification Failed",
                `Failed to permanently ${isRemove ? "remove" : "modify"} zone in the field file cache.`
            )
            console.error(`Failed to ${isRemove ? "remove" : "modify"} zone in field file:`, error)
        } finally {
            setIsPending(false)
            onClose()
        }
    }

    return (
        <Dialog open={isOpen} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>
                {isRemove ? "Remove" : "Modify"} {zoneLabel} Zone
            </DialogTitle>
            <DialogContent>
                <Stack spacing={2}>
                    <Typography variant="body1">
                        The {zoneType} zone &quot;{zoneName}&quot; was defined in the field file and is cached.
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                        Choose how you&apos;d like to {isRemove ? "remove" : "save"} it:
                    </Typography>
                    <Stack spacing={1}>
                        <Typography variant="body2">
                            <strong>Temporary {actionLabel}:</strong>{" "}
                            {isRemove
                                ? "Remove until next field reload. The zone will reappear when you refresh."
                                : "Save changes until next field reload. The original zone will reappear when you refresh."}
                        </Typography>
                        <Typography variant="body2">
                            <strong>Permanent {actionLabel}:</strong> Commits immediately to the local asset file.
                            This action cannot be undone by clicking Cancel in this panel.
                        </Typography>
                    </Stack>
                </Stack>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose} disabled={isPending}>
                    Cancel
                </Button>
                <Button onClick={handleTemporaryAction} disabled={isPending} variant="outlined" color="warning">
                    Temporary {actionLabel}
                </Button>
                <Button onClick={handlePermanentAction} disabled={isPending} variant="contained" color="primary">
                    {isPending ? `${isRemove ? "Removing" : "Modifying"}...` : `Permanent ${actionLabel}`}
                </Button>
            </DialogActions>
        </Dialog>
    )
}

export default DevtoolZoneModificationModal
