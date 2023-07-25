import React, {
    createContext,
    useState,
    useEffect,
    useCallback,
    useContext,
    ReactNode,
    ReactElement,
} from "react"

type ModalControlContextType = {
    openModal: (
        modalId: string,
        onOpen?: () => void,
        onClose?: () => void
    ) => void
    closeModal: () => void
    children?: ReactNode
}

const ModalControlContext = createContext<ModalControlContextType | null>(null)

export const useModalControlContext = () => {
    const context = useContext(ModalControlContext)
    if (!context)
        throw new Error(
            "useModalControlContext must be used within a ModalControlProvider"
        )
    return context
}

export const ModalControlProvider: React.FC<ModalControlContextType> = ({
    children,
    ...methods
}) => {
    return (
        <ModalControlContext.Provider value={methods}>
            {children}
        </ModalControlContext.Provider>
    )
}

type ModalInstance = {
    id: string
    component: ReactElement
    onOpen?: () => void
    onClose?: () => void
}

export const useModalManager = (modals: ReactElement[]) => {
    const [modalDictionary, setModalDictionary] = useState<{
        [key: string]: ModalInstance
    }>({})
    const [activeModalId, setActiveModalId] = useState<string | null>(null)

    const openModal = useCallback(
        (modalId: string, onOpen?: () => void, onClose?: () => void) => {
            if (modalDictionary[modalId]) {
                if (onOpen) {
                    modalDictionary[modalId].onOpen = onOpen
                    onOpen()
                }
                if (onClose) modalDictionary[modalId].onClose = onClose
            }
            setActiveModalId(modalId)
        },
        [modalDictionary]
    )

    const closeModal = useCallback(() => {
        if (activeModalId) {
            const inst = modalDictionary[activeModalId]
            if (inst && inst.onClose) {
                inst.onClose()
            }
        }
        setActiveModalId(null)
    }, [activeModalId, modalDictionary])

    const registerModal = useCallback(
        (modalId: string, modal: ModalInstance) => {
            modalDictionary[modalId] = modal
        },
        [modalDictionary]
    )

    const unregisterModal = useCallback((modalId: string) => {
        setModalDictionary(prevDictionary => {
            const newDictionary = { ...prevDictionary }
            delete newDictionary[modalId]
            return newDictionary
        })
    }, [])

    const getActiveModalElement = useCallback(() => {
        if (activeModalId !== null) {
            const modal = modalDictionary[activeModalId]
            return modal ? modal.component : null
        }
        return null
    }, [activeModalId, modalDictionary])

    useEffect(() => {
        modals.forEach(modalComponent => {
            const id = modalComponent.props.modalId
            registerModal(id, {
                id: id,
                component: modalComponent,
                onOpen: () => { },
                onClose: () => { },
            })
        })
    }, [modals, closeModal, openModal, registerModal])

    return {
        modalDictionary,
        activeModalId,
        openModal,
        closeModal,
        registerModal,
        unregisterModal,
        getActiveModalElement,
    }
}
