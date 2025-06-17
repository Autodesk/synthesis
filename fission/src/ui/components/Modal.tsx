import React, { ReactNode } from "react"
import { ClickAwayListener } from "@mui/base/ClickAwayListener"
import { useModalControlContext } from "@/ui/ModalContext"

export type ModalPropsImpl = {
    modalId: string
}

type ModalProps = {
    name: string
    modalId: string
    icon?: ReactNode | string
    onCancel?: () => void
    onMiddle?: () => void
    onAccept?: () => void
    cancelName?: string
    middleName?: string
    acceptName?: string
    cancelEnabled?: boolean
    middleEnabled?: boolean
    acceptEnabled?: boolean
    cancelBlocked?: boolean
    middleBlocked?: boolean
    acceptBlocked?: boolean
    children?: ReactNode
    className?: string
    contentClassName?: string
}

const Modal: React.FC<ModalProps> = ({
    children,
    name,
    modalId,
    icon,
    onCancel,
    onMiddle,
    onAccept,
    cancelName,
    middleName,
    acceptName,
    cancelEnabled = true,
    middleEnabled = false,
    acceptEnabled = true,
    cancelBlocked = false,
    middleBlocked = false,
    acceptBlocked = false,
    className,
    contentClassName,
}) => {
    const { closeModal } = useModalControlContext()

    const iconEl: ReactNode = typeof icon === "string" ? <img src={icon} className="w-6" alt="Icon" /> : icon

    return (
        <ClickAwayListener onClickAway={_ => closeModal()} key={modalId}>
            <div
                id={modalId}
                key={modalId}
                className={`${className} flex flex-col absolute max-w-[50vw] max-h-[95vh] w-fit h-fit left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 bg-background text-main-text m-auto border-5 rounded-2xl shadow-sm shadow-slate-800`}
            >
                {name && (
                    <div id="header" className="flex items-center gap-8 h-16">
                        <span className="flex justify-center align-center ml-8 text-icon">{iconEl && iconEl}</span>
                        <h1
                            className="text-3xl inline-block align-middle whitespace-nowrap mr-10"
                            style={{
                                userSelect: "none",
                                MozUserSelect: "none",
                                msUserSelect: "none",
                                WebkitUserSelect: "none",
                            }}
                        >
                            {name}
                        </h1>
                    </div>
                )}
                <div
                    id="content"
                    className={`${contentClassName || ""} ${
                        !contentClassName?.includes("mx") ? "mx-[2rem]" : ""
                    } flex flex-col gap-4 max-h-[75vh]`}
                >
                    {children}
                </div>
                {(cancelEnabled || middleEnabled || acceptEnabled) && (
                    <div
                        id="footer"
                        className="flex justify-between mx-[2rem] py-[1rem] text-accept-cancel-button-text"
                    >
                        {cancelEnabled && (
                            <input
                                type="button"
                                value={cancelName || "Cancel"}
                                onClick={() => {
                                    closeModal()
                                    if (!cancelBlocked && onCancel) onCancel()
                                }}
                                className={`${
                                    cancelBlocked ? "bg-interactive-background" : "bg-cancel-button"
                                } rounded-md cursor-pointer px-4 py-1 font-bold duration-100 hover:brightness-90
                                transform transition-transform hover:scale-[1.03] active:scale-[1.06]`}
                                style={{ fontWeight: "bold" }}
                            />
                        )}
                        {middleEnabled && (
                            <input
                                type="button"
                                value={middleName || ""}
                                onClick={() => {
                                    if (!middleBlocked && onMiddle) onMiddle()
                                }}
                                className={`${
                                    middleBlocked ? "bg-interactive-background" : "bg-accept-button"
                                } rounded-md cursor-pointer px-4 py-1 font-bold duration-100 hover:brightness-90 
                                transform transition-transform hover:scale-[1.03] active:scale-[1.06]`}
                                style={{ fontWeight: "bold" }}
                            />
                        )}
                        {acceptEnabled && (
                            <input
                                type="button"
                                value={acceptName || "Accept"}
                                onClick={() => {
                                    closeModal()
                                    if (!acceptBlocked && onAccept) onAccept()
                                }}
                                className={`${
                                    acceptBlocked ? "bg-interactive-background" : "bg-accept-button"
                                } rounded-md cursor-pointer px-4 py-1 font-bold duration-100 hover:brightness-90
                                transform transition-transform hover:scale-[1.03] active:scale-[1.06]`}
                                style={{ fontWeight: "bold" }}
                            />
                        )}
                    </div>
                )}
            </div>
        </ClickAwayListener>
    )
}

export default Modal
