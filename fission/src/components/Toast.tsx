import React, { ReactElement, useEffect } from "react"
import { ToastData, useToastContext } from "../ToastContext"
import { GrFormClose } from "react-icons/gr"
import { BsFillWrenchAdjustableCircleFill } from "react-icons/bs"
import { AiFillWarning } from "react-icons/ai"
import { BiSolidErrorCircle } from "react-icons/bi"

const TOAST_TIMEOUT: number = 5_000

const Toast: React.FC<ToastData> = ({ id, type, title, description }) => {
    const { removeToast } = useToastContext()

    useEffect(() => {
        const timer = setTimeout(() => {
            removeToast(id)
        }, TOAST_TIMEOUT)

        return () => clearTimeout(timer)
    }, [id, removeToast])

    const handleClose = () => removeToast(id)

    let icon: ReactElement
    let className: string

    switch (type) {
        case "info":
            icon = <BsFillWrenchAdjustableCircleFill size={48} className="h-full w-full text-main-text" />
            className = "bg-toast-info"
            break
        case "warning":
            icon = <AiFillWarning size={48} className="h-full w-full text-main-text" />
            className = "bg-toast-warning"
            break
        case "error":
            icon = <BiSolidErrorCircle size={48} className="h-full w-full text-main-text" />
            className = "bg-toast-error"
            break
    }

    return (
        <div
            className={`toast toast-${type.toLowerCase()} aspect-toast relative flex flex-row ${className} px-4 py-2 content-center justify-between items-center rounded-lg shadow-md shadow-[rgba(0,0,0,0.5)]`}
        >
            <div className="w-10 h-10 mr-1">{icon}</div>
            <div className="toast-content w-auto ml-2 text-main-text">
                <div className="flex flex-col w-full">
                    <div className="flex flex-row-reverse w-full h-min justify-between">
                        <button
                            type="button"
                            onClick={handleClose}
                            className="toast-close bg-[rgba(0,0,0,0)] h-min aspect-square p-0"
                        >
                            <GrFormClose size={20} className="text-main-text" />
                        </button>
                        <p className="font-medium uppercase h-min">{title}</p>
                    </div>
                    <p className="truncate w-full">{description}</p>
                </div>
            </div>
        </div>
    )
}

export default Toast
