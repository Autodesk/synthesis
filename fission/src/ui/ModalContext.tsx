import { createContext, useContext, ReactNode } from "react"

type ModalControlContextType = {
    openModal: (modalId: string, onOpen?: () => void, onClose?: () => void) => void
    closeModal: () => void
    children?: ReactNode
}

const ModalControlContext = createContext<ModalControlContextType | null>(null)

export const useModalControlContext = () => {
    const context = useContext(ModalControlContext)
    if (!context) throw new Error("useModalControlContext must be used within a ModalControlProvider")
    return context
}

export const ModalControlProvider: React.FC<ModalControlContextType> = ({ children, ...methods }) => {
    return <ModalControlContext.Provider value={methods}>{children}</ModalControlContext.Provider>
}
