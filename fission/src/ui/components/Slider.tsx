import React from "react"
import { Slider as BaseSlider } from "@mui/base/Slider"
import { Mark } from "@mui/base/useSlider"
import Label, { LabelSize } from "./Label"
import { LabelWithTooltip } from "./StyledComponents"

type CustomFormatOptions = {
    prefix?: string
    suffix?: string
}

type SliderProps = {
    label: string
    min: number
    max: number
    value: number
    onChange?: (event: Event, value: number | number[], activeThumb: number) => void
    marks?: Mark[] | boolean
    step?: number
    locale?: string
    format?: Intl.NumberFormatOptions & CustomFormatOptions
    tooltipText?: string
}

const Slider: React.FC<SliderProps> = ({
    label,
    min,
    max,
    value,
    onChange,
    step,
    marks,
    locale,
    format,
    tooltipText,
}) => {
    locale ||= "en-us"
    format ||= {
        maximumFractionDigits: (1.0 / (step == 0 ? 1 : step ?? 1)).toString().length - 1,
        prefix: "",
        suffix: "",
    }
    format.prefix ||= ""
    format.suffix ||= ""

    return (
        <>
            <div className="flex flex-row justify-between w-full">
                {tooltipText ? LabelWithTooltip(label, tooltipText) : <Label size={LabelSize.Small}>{label}</Label>}
                <Label size={LabelSize.Small}>
                    {format.prefix + value.toLocaleString(locale, format) + format.suffix}
                </Label>
            </div>
            <BaseSlider
                onChange={onChange}
                value={value}
                min={min}
                max={max}
                step={step}
                marks={marks}
                slotProps={{
                    root: ownerState => {
                        return {
                            className: `h-1 w-full inline-flex items-center relative touch-none ${ownerState.disabled ? "text-slate-200 dark:text-slate-200" : "cursor-pointer text-[#343A40] dark:text-[#343A40]"}
                            transform transition-transform hover:scale-[1.01] active:scale-[1.02]`,
                        }
                    },
                    rail: {
                        className: `block absolute w-full h-[12px] rounded-full bg-interactive-background`,
                    },
                    track: {
                        className: `block absolute h-[12px] rounded-full bg-gradient-to-r from-interactive-element-left to-interactive-element-right`,
                    },
                    thumb: {
                        className: `display-none`,
                    },
                }}
            />
        </>
    )
}

// <div className="flex flex-col select-none">
//     <div className="flex flex-row justify-between">
//         <p className="text-sm">{label}</p>
//         <p className="text-sm float-right">
//             {format.prefix +
//                 value.toLocaleString(locale, format) +
//                 format.suffix}
//         </p>
//     </div>
//     <div
//         id="container"
//         ref={containerRef}
//         onMouseMove={ev => onMouseMove(ev)}
//         onMouseDown={() => setMouseDown(true)}
//         onMouseUp={() => setMouseDown(false)}
//         className="relative w-full h-4 max-w-full cursor-pointer"
//     >
//         <div
//             id="background"
//             className="absolute bg-interactive-background w-full h-full rounded-lg"
//         ></div>
//         <div
//             id="color"
//             style={{ width: `max(calc(${getPercent()}%), 1rem)` }}
//             className="absolute bg-gradient-to-r from-interactive-element-left to-interactive-element-right h-full rounded-lg"
//         ></div>
//         <div
//             id="handle"
//             style={{ width: `max(calc(${getPercent()}%), 1rem)` }}
//             className="hidden absolute w-4 h-4 bg-interactive-element-right rounded-lg -translate-x-full"
//         ></div>
//     </div>
// </div>

export default Slider
