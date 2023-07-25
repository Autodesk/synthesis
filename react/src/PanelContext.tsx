import React, {
    createContext,
    useState,
    useEffect,
    useCallback,
    useContext,
    ReactNode,
} from "react"

type PanelControlContextType = {
    openPanel: (panelId: string) => void
    closePanel: (panelId: string) => void
    children?: ReactNode
}

const PanelControlContext = createContext<PanelControlContextType | null>(null)

export const usePanelControlContext = () => {
    const context = useContext(PanelControlContext)
    if (!context)
        throw new Error(
            "usePanelControlContext must be used within a PanelControlProvider"
        )
    return context
}

export const PanelControlProvider: React.FC<PanelControlContextType> = ({
    children,
    ...methods
}) => {
    return (
        <PanelControlContext.Provider value={methods}>
            {children}
        </PanelControlContext.Provider>
    )
}

type PanelInstance = {
    id: string
    component: React.ReactNode
    onOpen: () => void
    onClose: () => void
}

type PanelData = {
    id: string
    component: React.ReactNode
}

export const usePanelManager = (panels: PanelData[]) => {
    const [panelDictionary, setPanelDictionary] = useState<{
        [key: string]: PanelInstance
    }>({})
    const [activePanelIds, setActivePanelIds] = useState<string[]>([])

    const openPanel = useCallback(
        (panelId: string, onOpen?: () => void, onClose?: () => void) => {
            if (!activePanelIds.includes(panelId)) {
                setActivePanelIds([...activePanelIds, panelId])
                if (panelDictionary[panelId]) {
                    if (onOpen) {
                        panelDictionary[panelId].onOpen = onOpen
                        onOpen()
                    }
                    if (onClose) panelDictionary[panelId].onClose = onClose
                }
            }
        },
        [activePanelIds, panelDictionary]
    )

    const closePanel = useCallback(
        (panelId: string) => {
            if (panelDictionary[panelId]) {
                panelDictionary[panelId].onClose()
                setActivePanelIds(activePanelIds.filter(i => i != panelId))
            }
        },
        [activePanelIds, panelDictionary]
    )

    const closeAllPanels = useCallback(() => {
        if (activePanelIds.length > 0) {
            activePanelIds.forEach(id => {
                if (panelDictionary[id]) {
                    panelDictionary[id].onClose()
                }
            })
        }
        setActivePanelIds([])
    }, [activePanelIds, panelDictionary])

    const registerPanel = useCallback(
        (panelId: string, panel: PanelInstance) => {
            setPanelDictionary(prevDictionary => ({
                ...prevDictionary,
                [panelId]: panel,
            }))
        },
        []
    )

    const unregisterPanel = useCallback((panelId: string) => {
        setPanelDictionary(prevDictionary => {
            const newDictionary = { ...prevDictionary }
            delete newDictionary[panelId]
            return newDictionary
        })
    }, [])

    const getActivePanelElements = useCallback(() => {
        if (activePanelIds !== null && activePanelIds.length > 0) {
            return activePanelIds
                .map((id: string) => {
                    const panel: PanelData = panelDictionary[id]
                    return panel ? panel.component : null
                })
                .filter(p => p != null)
        }
        return []
    }, [activePanelIds, panelDictionary])

    useEffect(() => {
        panels.forEach(panelData => {
            registerPanel(panelData.id, {
                id: panelData.id,
                component: panelData.component,
                onOpen: () => openPanel(panelData.id),
                onClose: () => closePanel(panelData.id),
            })
        })
    }, [panels, closePanel, openPanel, registerPanel])

    return {
        panelDictionary,
        activePanelIds,
        openPanel,
        closePanel,
        closeAllPanels,
        registerPanel,
        unregisterPanel,
        getActivePanelElements,
    }
}
