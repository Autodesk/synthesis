import { useEffect } from "react"
import { useModalControlContext } from "@/ui/helpers/UseModalManager"
import { usePanelControlContext } from "@/ui/helpers/UsePanelManager"
import { useToastContext } from "@/ui/ToastContext"
import { setAddToast, setOpenModal, setOpenPanel } from "./GlobalUIControls"

/**
 * So this is Hunter's kinda cursed approach to de-react-ifying some of our UI controls.
 * Essentially, this component will expose context controls for our UI, which allows
 * non-UI components (such as APSDataManagement) to use UI controls (such as addToast).
 *
 * Stored in a component to ensure a lifetime is followed with this handles.
 *
 * @returns Global UI Component
 */
const GlobalUIComponent: React.FC = () => {
    const { openModal } = useModalControlContext()
    const { openPanel } = usePanelControlContext()
    const { addToast } = useToastContext()

    useEffect(() => {
        setOpenModal(openModal)

        return () => {
            setOpenModal(() => {})
        }
    }, [openModal])

    useEffect(() => {
        setOpenPanel(openPanel)

        return () => {
            setOpenPanel(() => {})
        }
    }, [openPanel])

    useEffect(() => {
        setAddToast(addToast)

        return () => {
            setAddToast(() => {})
        }
    }, [addToast])

    return <></>
}

export default GlobalUIComponent
