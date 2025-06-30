import { ModalControlContext, ModalControlContextType } from "./helpers/UseModalManager"

export const ModalControlProvider: React.FC<ModalControlContextType> = ({ children, ...methods }) => {
    return <ModalControlContext.Provider value={methods}>{children}</ModalControlContext.Provider>
}
