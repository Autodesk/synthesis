/**
 * So this is Hunter's kinda cursed approach to de-react-ifying some of our UI controls.
 * Essentially, this component will expose context controls for our UI, which allows
 * non-UI components (such as APSDataManagement) to use UI controls (such as addToast).
 *
 * Stored in a component to ensure a lifetime is followed with this handles.
 *
 * @returns Global UI Component
 */
function GlobalUIComponent() {
    // const { openModal } = useModalControlContext()
    // const { openPanel } = usePanelControlContext()
    // const { addToast } = useToastContext()

    // useEffect(() => {
    //     setOpenModal(openModal)

    //     return () => {
    //         setOpenModal(undefined)
    //     }
    // }, [openModal])

    // useEffect(() => {
    //     setOpenPanel(openPanel)

    //     return () => {
    //         setOpenPanel(undefined)
    //     }
    // }, [openPanel])

    // useEffect(() => {
    //     setAddToast(addToast)

    //     return () => {
    //         setAddToast(undefined)
    //     }
    // }, [addToast])

    return <></>
}

export default GlobalUIComponent
