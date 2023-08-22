import Tooltip from "@/components/Tooltip"
import { AnimatePresence, motion } from "framer-motion"
import React, {
    ReactNode,
    createContext,
    useCallback,
    useContext,
    useState
} from "react"

export type TooltipControl = { control: string, description: string }
export type TooltipType = "controls"

type TooltipControlContextType = {
    showTooltip: (
        type: TooltipType,
        duration: number,
        controls?: TooltipControl[]
    ) => void
    children?: ReactNode
}

const TooltipControlContext = createContext<TooltipControlContextType | null>(null)

export const useTooltipControlContext = () => {
    const context = useContext(TooltipControlContext)
    if (!context)
        throw new Error(
            "useTooltipControlContext must be used within a TooltipControlProvider"
        )
    return context
}

let tooltip: ReactNode;

export const TooltipControlProvider: React.FC<TooltipControlContextType> = ({
    children,
    ...methods
}) => {
    return (
        <TooltipControlContext.Provider value={methods}>
            <AnimatePresence>
                {tooltip && (
                    <motion.div
                        initial={{
                            scale: 0, opacity: 0, y: -150,
                        }}
                        animate={{
                            scale: 1, opacity: 1, y: 0,
                        }}
                        exit={{
                            scale: 0, opacity: 0, y: -150,
                        }}
                        transition={{ type: 'spring', stiffness: 300, damping: 25 }}
                        style={{ translateX: "-50%" }}
                        className="absolute left-1/2 top-2"
                        key="tooltip"
                    >
                        {tooltip}
                    </motion.div>
                )}
                {children}
            </AnimatePresence>
        </TooltipControlContext.Provider>
    )
}

export const useTooltipManager = () => {
    const [duration, setDuration] = useState<number>(0);
    const [timeout, setTimeoutState] = useState<NodeJS.Timeout | null>(null);

    const showTooltip = useCallback(
        (type: TooltipType, duration: number, controls?: TooltipControl[]) => {
            tooltip = (
                <Tooltip type={type} controls={controls} />
            )
            setDuration(duration)

            console.log(tooltip, duration)

            if (timeout !== null) {
                clearTimeout(timeout);
            }

            const newTimeout = setTimeout(() => {
                console.log("Hiding tooltip")
                tooltip = undefined;
                setDuration(0);
            }, duration)
            setTimeoutState(newTimeout);
        }, [timeout])

    // useEffect(() => {
    //     return () => {
    //         if (timeout !== null) {
    //             clearTimeout(timeout);
    //             setTimeoutState(null);
    //         }
    //         tooltip = undefined;
    //         setDuration(0);
    //     }
    // }, [duration, timeout]);

    return {
        showTooltip,
    }
}
