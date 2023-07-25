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
    onAccept?: () => void
    children?: ReactNode
}

const Modal: React.FC<ModalProps> = ({
    children,
    name,
    icon,
    onCancel,
    onAccept,
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
            <div id="footer" className="flex justify-between mx-10 py-8">
                <input
                    type="button"
                    value="Cancel"
                    onClick={() => {
                        closeModal()
                        if (onCancel) onCancel()
                    }}
                    className="bg-red-500 rounded-md cursor-pointer px-4 py-1 text-black font-bold duration-100 hover:bg-red-600"
                />
                <input
                    type="button"
                    value="Accept"
                    onClick={() => {
                        closeModal()
                        if (onAccept) onAccept()
                    }}
                    className="bg-blue-500 rounded-md cursor-pointer px-4 py-1 text-black font-bold duration-100 hover:bg-blue-600"
                />
            </div>
        </div>
    )
}

export default Modal
