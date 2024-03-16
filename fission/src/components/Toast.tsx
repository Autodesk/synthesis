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
            icon = (
                <BsFillWrenchAdjustableCircleFill
                    size={48}
                    className="h-full w-full text-main-text"
                />
            )
            className = "bg-toast-info"
            break
        case "warning":
            icon = (
                <AiFillWarning
                    size={48}
                    className="h-full w-full text-main-text"
                />
            )
            className = "bg-toast-warning"
            break
        case "error":
            icon = (
                <BiSolidErrorCircle
                    size={48}
                    className="h-full w-full text-main-text"
                />
            )
            className = "bg-toast-error"
            break
    }

    return (
        <div
            className={`toast toast-${type.toLowerCase()} relative w-full flex flex-row ${className} px-4 py-2 content-center items-center rounded-lg shadow-md shadow-slate-500`}
        >
            <button
                type="button"
                onClick={handleClose}
                className="toast-close absolute right-2 top-2"
            >
                <GrFormClose
                    size={20}
                    className="w-full h-full text-main-text"
                />
            </button>
            <div className="w-10 h-10 mr-1">{icon}</div>
            <div className="toast-content w-5/6 ml-2 text-main-text">
                <p className="font-bold uppercase">{title}</p>
                <p className="truncate">{description}</p>
            </div>
        </div>
    )
}

export default Toast
