import { PanelControlContext, PanelControlContextType } from "./helpers/UsePanelManager"

export const PanelControlProvider: React.FC<PanelControlContextType> = ({ children, ...methods }) => {
    return <PanelControlContext.Provider value={methods}>{children}</PanelControlContext.Provider>
}
