import { Button, Stack } from "@mui/material"
import type { ReactElement } from "react"
import { Global_SetAlert } from "../../lib/GlobalUtils.tsx"

interface FusionSelectButtonProps<T> {
    label: string
    onClick: () => T

    onSelection: (data: Awaited<T>) => void
    selection: {
        isSelecting: boolean
        setIsSelecting: (val: boolean) => void
    }
}

const FusionSelectButton = <T,>({
    label,
    onClick,
    onSelection,
    selection,
}: FusionSelectButtonProps<T>): ReactElement => {
    return (
        <Stack direction={"row"} spacing={2} paddingTop="15px" alignItems={"center"} justifyContent={"left"}>
            <Button
                variant="contained"
                color="secondary"
                loading={selection.isSelecting}
                sx={{ px: "20px" }}
                loadingIndicator={"Selecting..."}
                onClick={async () => {
                    selection.setIsSelecting(true)
                    try {
                        const data = await onClick()
                        onSelection(data)
                    } catch (e) {
                        console.error(e)
                        Global_SetAlert("error", `${e}`)
                    } finally {
                        selection.setIsSelecting(false)
                    }
                }}
            >
                {label}
            </Button>
        </Stack>
    )
}

export default FusionSelectButton
