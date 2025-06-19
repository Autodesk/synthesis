import React, { createContext, useContext, ReactNode } from "react"

type PanelControlContextType = {
    openPanel: (panelId: string) => void
    closePanel: (panelId: string) => void
    closeAllPanels: () => void
    children?: ReactNode
}

const PanelControlContext = createContext<PanelControlContextType | null>(null)

export const usePanelControlContext = () => {
    const context = useContext(PanelControlContext)
    if (!context) throw new Error("usePanelControlContext must be used within a PanelControlProvider")
    return context
}

export const PanelControlProvider: React.FC<PanelControlContextType> = ({ children, ...methods }) => {
    return <PanelControlContext.Provider value={methods}>{children}</PanelControlContext.Provider>
}
