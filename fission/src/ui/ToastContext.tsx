import React, { createContext, useState, useContext, useCallback, ReactNode } from "react"
import Toast from "@/components/Toast"
import { AnimatePresence, motion } from "framer-motion"

export type ToastType = "info" | "warning" | "error"

export type ToastData = {
    id: string
    type: ToastType
    title: string
    description: string
}

type ToastContextType = {
    toasts: ToastData[]
    addToast: (type: ToastType, title: string, description: string) => void
    removeToast: (toastId: string) => void
}

const ToastContext = createContext<ToastContextType | null>(null)

export const useToastContext = () => {
    const context = useContext(ToastContext)
    if (!context) throw new Error("useToastContext must be used within a ToastProvider")
    return context
}

export const ToastProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    const [toasts, setToasts] = useState<ToastData[]>([])

    const addToast = useCallback((type: ToastType, title: string, description: string) => {
        // divide by 10 so that it's harder to have duplicates? could make smaller or remove
        const id = "toast-" + Math.floor(Date.now() / 10).toString()
        const newToast: ToastData = {
            id,
            type,
            title,
            description,
        }
        setToasts(prevToasts => {
            if (prevToasts.some(t => t.id == id)) return prevToasts
            return [...prevToasts, newToast]
        })
    }, [])

    const removeToast = useCallback((toastId: string) => {
        setToasts(prevToasts => prevToasts.filter(t => t.id !== toastId))
    }, [])

    return <ToastContext.Provider value={{ toasts, addToast, removeToast }}>{children}</ToastContext.Provider>
}

export const ToastContainer: React.FC = () => {
    const { toasts } = useToastContext()

    return (
        <div className="absolute right-[5pt] bottom-[5pt] pl-8 pb-8 w-min h-fit overflow-hidden flex flex-col gap-2">
            <AnimatePresence>
                {toasts.length > 0 &&
                    toasts.map(t => (
                        <motion.div
                            layout
                            initial={{
                                translateX: "100%",
                                opacity: 0,
                            }}
                            animate={{
                                translateX: "0%",
                                opacity: 1,
                            }}
                            exit={{
                                translateX: "100%",
                                opacity: 0,
                            }}
                            transition={{
                                type: "spring",
                                stiffness: 300,
                                damping: 28,
                            }}
                            key={t.id}
                            className="w-fit"
                        >
                            <Toast id={t.id} type={t.type} title={t.title} description={t.description} />
                        </motion.div>
                    ))}
            </AnimatePresence>
        </div>
    )
}
