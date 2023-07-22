import React, { createContext, useState, useEffect, useCallback, useContext } from 'react';

type ModalControlContextType = {
    openModal: (modalId: string) => void;
    closeModal: () => void;
}

const ModalControlContext = createContext<ModalControlContextType | null>(null)

export const useModalControlContext = () => {
    const context = useContext(ModalControlContext);
    if (!context) throw new Error('useModalControlContext must be used within a ModalControlProvider');
    return context;
}

export const ModalControlProvider: React.FC<ModalControlContextType> = ({ children, ...methods }) => {
    return <ModalControlContext.Provider value={methods}>{children}</ModalControlContext.Provider>
}

type ModalInstance = {
    id: string;
    component: React.ReactNode;
    onOpen: () => void;
    onClose: () => void;
}

type ModalData = {
    id: string;
    component: React.ReactNode;
}

export const useModalManager = (modals: ModalData[]) => {
    const [modalDictionary, setModalDictionary] = useState<{ [key: number]: ModalInstance }>({});
    const [activeModalId, setActiveModalId] = useState<string | null>(null);

    const openModal = useCallback((modalId: string) => {
        setActiveModalId(modalId);
    }, [])

    const closeModal = useCallback(() => {
        setActiveModalId(null)
    }, [])

    const registerModal = useCallback((modalId: string, modal: ModalInstance) => {
        setModalDictionary(prevDictionary => ({ ...prevDictionary, [modalId]: modal }))
    }, [])

    const unregisterModal = useCallback((modalId: string) => {
        setModalDictionary(prevDictionary => {
            const newDictionary = { ...prevDictionary };
            delete newDictionary[modalId];
            return newDictionary;
        })
    }, [])

    const getActiveModalElement = useCallback(() => {
        if (activeModalId !== null) {
            const modal = modalDictionary[activeModalId];
            return modal ? modal.component : null;
        }
        return null
    }, [activeModalId, modalDictionary])

    useEffect(() => {
        modals.forEach(modalData => {
            registerModal(modalData.id, {
                id: modalData.id,
                component: modalData.component,
                onOpen: () => openModal(modalData.id),
                onClose: closeModal,
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
        getActiveModalElement
    }
}
