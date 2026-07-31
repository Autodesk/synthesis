import type React from "react"
import { useEffect } from "react"
import type { ModalImplProps } from "@/components/Modal.tsx"
import { useUIContext } from "@/ui/helpers/UIProviderHelpers.ts"
import Label from "@/components/Label.tsx"

const ConfirmChangesModal: React.FC<ModalImplProps<void, void>> = ({ modal }) => {
    const { configureScreen } = useUIContext()
    useEffect(() => {
        configureScreen(modal!, { title: "Confirm Changes", acceptText: "Save & Continue", cancelText: "Back" }, {})
    }, [])

    return (
        <Label size="sm">
            You may have unsaved changes to a robot or field configuration! Would you like to save and proceed or go
            back and continue making changes?
        </Label>
    )
}

export default ConfirmChangesModal
