import React, { ReactNode } from "react"
import { usePanelControlContext } from "../PanelContext"

export type OpenLocation =
    | "top-left"
    | "top"
    | "top-right"
    | "left"
    | "center"
    | "right"
    | "bottom-left"
    | "bottom"
    | "bottom-right"

type LocationOptions = { className: string; styles: { [key: string]: string } }

const getLocationClasses = (openLocation: OpenLocation, sidePadding: number): LocationOptions => {
    // Can't use tailwind left-[custom] because it won't generate the classes since we're using them dynamically with string templating
    const paddingStyle = `${sidePadding}px`
    switch (openLocation) {
        case "top-left":
            return {
                className: ``,
                styles: { left: paddingStyle, top: paddingStyle },
            }
        case "top":
            return {
                className: `left-1/2 -translate-x-1/2`,
                styles: { top: paddingStyle },
            }
        case "top-right":
            return {
                className: ``,
                styles: { right: paddingStyle, top: paddingStyle },
            }
        case "left":
            return {
                className: `top-1/2 -translate-y-1/2`,
                styles: { left: paddingStyle },
            }
        case "center":
            return {
                className: `left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2`,
                styles: {},
            }
        case "right":
            return {
                className: `top-1/2 -translate-y-1/2`,
                styles: { right: paddingStyle },
            }
        case "bottom-left":
            return {
                className: ``,
                styles: { left: paddingStyle, bottom: paddingStyle },
            }
        case "bottom":
            return {
                className: `left-1/2 -translate-x-1/2`,
                styles: { bottom: paddingStyle },
            }
        case "bottom-right":
            return {
                className: ``,
                styles: { right: paddingStyle, bottom: paddingStyle },
            }
    }
}

export type PanelPropsImpl = {
    panelId: string
    openLocation?: OpenLocation
    sidePadding?: number
}

type PanelProps = {
    panelId: string
    openLocation?: OpenLocation
    sidePadding?: number
    name?: string
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

const Panel: React.FC<PanelProps> = ({
    children,
    name,
    icon,
    panelId,
    openLocation,
    sidePadding,
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
    const { closePanel } = usePanelControlContext()
    const iconEl: ReactNode = typeof icon === "string" ? <img src={icon} className="w-6" alt="Icon" /> : icon
    openLocation ||= "center"
    sidePadding ||= 16
    const locationClasses = getLocationClasses(openLocation, sidePadding)
    return (
        <div>
            <div
                className={`absolute ${locationClasses.className} ${className || ""} bg-background text-main-text m-auto border-5 rounded-2xl shadow-sm shadow-slate-800`}
                style={locationClasses.styles}
                key={"panel-" + panelId}
            >
                {name && (
                    <div id="header" className="flex items-center gap-8 h-16">
                        <span className="flex justify-center align-center ml-8 text-icon">{iconEl && iconEl}</span>
                        <h1 className="text-3xl inline-block align-middle whitespace-nowrap mr-10">{name}</h1>
                    </div>
                )}
                <div
                    id="content"
                    className={`${contentClassName || ""} ${
                        !contentClassName?.includes("mx") ? "mx-16" : ""
                    } flex flex-col gap-4 max-h-[75vh]`}
                >
                    {children}
                </div>
                {(cancelEnabled || middleEnabled || acceptEnabled) && (
                    <div id="footer" className="flex justify-between mx-10 py-8 text-accept-cancel-button-text">
                        {cancelEnabled && (
                            <input
                                type="button"
                                value={cancelName || "Cancel"}
                                onClick={() => {
                                    closePanel(panelId)
                                    if (!cancelBlocked && onCancel) onCancel()
                                }}
                                className={`${
                                    cancelBlocked ? "bg-interactive-background" : "bg-cancel-button"
                                } rounded-md cursor-pointer px-4 py-1 font-bold duration-100 hover:brightness-90`}
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
                                } rounded-md cursor-pointer px-4 py-1 font-bold duration-100 hover:brightness-90`}
                            />
                        )}
                        {acceptEnabled && (
                            <input
                                type="button"
                                value={acceptName || "Accept"}
                                onClick={() => {
                                    closePanel(panelId)
                                    if (!acceptBlocked && onAccept) onAccept()
                                }}
                                className={`${
                                    acceptBlocked ? "bg-interactive-background" : "bg-accept-button"
                                } rounded-md cursor-pointer px-4 py-1 font-bold duration-100 hover:brightness-90`}
                            />
                        )}
                    </div>
                )}
            </div>
        </div>
    )
}
// <div
//     id={panelId}
//     className={`${locationClasses} ${className} w-fit h-fit bg-background text-main-text m-auto border-5 rounded-2xl shadow-sm shadow-slate-800`}
// >
//         </div>

export default Panel
