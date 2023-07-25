import React, {
    createContext,
    useState,
    useEffect,
    useCallback,
    useContext,
    ReactNode,
    ReactElement,
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
    component: ReactElement
    onOpen?: () => void
    onClose?: () => void
}

export const usePanelManager = (panels: ReactElement[]) => {
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
            let inst = panelDictionary[panelId];
            if (inst) {
                if (inst.onClose)
                    inst.onClose()
                setActivePanelIds(activePanelIds.filter(i => i != panelId))
            } else {
                setActivePanelIds(activePanelIds.filter(i => {
                    inst = panelDictionary[i];
                    if (inst.component.props.name == panelId) {
                        if (inst.onClose)
                            inst.onClose()
                        return false;
                    } else {
                        return true;
                    }
                }))
            }
        },
        [activePanelIds, panelDictionary]
    )

    const closeAllPanels = useCallback(() => {
        if (activePanelIds.length > 0) {
            activePanelIds.forEach(id => {
                const inst = panelDictionary[id];
                if (inst && inst.onClose) {
                    inst.onClose()
                }
            })
        }
        setActivePanelIds([])
    }, [activePanelIds, panelDictionary])

    const registerPanel = useCallback(
        (panelId: string, panel: PanelInstance) => {
            panelDictionary[panelId] = panel;
        },
        [panelDictionary]
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
                    const panel: PanelInstance = panelDictionary[id]
                    return panel ? panel.component : null
                })
                .filter(p => p != null)
        }
        return []
    }, [activePanelIds, panelDictionary])

    useEffect(() => {
        panels.forEach(panelData => {
            const id = panelData.props.panelId;
            registerPanel(id, {
                id: id,
                component: panelData,
                onOpen: () => { },
                onClose: () => { },
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
