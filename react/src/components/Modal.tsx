import React, { ReactNode } from "react"
import { useModalControlContext } from "../ModalContext"

export type ModalPropsImpl = {
    modalId: string
}

type ModalProps = {
    name: string
    icon: ReactNode | string
    modalId: string
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
}

const Modal: React.FC<ModalProps> = ({
    children,
    name,
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
    acceptBlocked = false
}) => {
    const { closeModal } = useModalControlContext()

    const iconEl: ReactNode =
        typeof icon === "string" ? (
            <img src={icon} className="w-6" alt="Icon" />
        ) : (
            icon
        )

    return (
        <div
            id={name}
            className="absolute w-fit h-fit left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 bg-black text-white m-auto border-5 rounded-2xl shadow-sm shadow-slate-800"
        >
            <div id="header" className="flex items-center gap-8 h-16">
                <span className="flex justify-center align-center ml-8">
                    {iconEl}
                </span>
                <h1 className="text-3xl inline-block align-middle whitespace-nowrap mr-10">
                    {name}
                </h1>
            </div>
            <div id="content" className="mx-16 flex flex-col gap-8">
                {children}
            </div>
            <div id="footer" className="flex justify-between mx-10 py-8 gap-4">
                {cancelEnabled && (
                    <input
                        type="button"
                        value={cancelName || "Cancel"}
                        onClick={() => {
                            closeModal()
                            if (!cancelBlocked && onCancel) onCancel()
                        }}
                        className={`${cancelBlocked ? "bg-gray-700" : "bg-red-500"} rounded-md cursor-pointer px-4 py-1 text-black font-bold duration-100 hover:bg-red-600`}
                    />
                )}
                {middleEnabled && (
                    <input
                        type="button"
                        value={middleName || ""}
                        onClick={() => {
                            closeModal()
                            if (!middleBlocked && onMiddle) onMiddle()
                        }}
                        className={`${middleBlocked ? "bg-gray-700" : "bg-blue-500"} rounded-md cursor-pointer px-4 py-1 text-black font-bold duration-100 hover:bg-blue-600`}
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
                        className={`${acceptBlocked ? "bg-gray-700" : "bg-blue-500"} rounded-md cursor-pointer px-4 py-1 text-black font-bold duration-100 hover:bg-blue-600`}
                    />
                )}
            </div>
        </div>
    )
}

export default Modal
