import React, { SyntheticEvent, useRef, useState } from "react"

type CustomFormatOptions = {
    prefix?: string
    suffix?: string
}

type SliderProps = {
    label?: string
    min: number
    max: number
    defaultValue: number
    step?: number
    locale?: string
    format?: Intl.NumberFormatOptions & CustomFormatOptions
}

const Slider: React.FC<SliderProps> = ({
    label,
    min,
    max,
    defaultValue,
    step,
    locale,
    format,
}) => {
    const containerRef = useRef<HTMLDivElement>(null)
    const [value, setValue] = useState(defaultValue)
    const [mouseDown, setMouseDown] = useState(false)

    // non-inclusive top
    // max += 1

    if (!locale) {
        locale = "en-us"
    }

    if (!format) {
        format = {
            maximumFractionDigits: 0,
            prefix: "",
            suffix: "",
        }
    }
    if (!format.prefix) {
        format.prefix = ""
    }
    if (!format.suffix) {
        format.suffix = ""
    }

    const getPercent = () => ((value - min) / (max - min)) * 100

    const onMouseMove = (e: SyntheticEvent) => {
        if (mouseDown) {
            const layerX = (e.nativeEvent as MouseEvent).offsetX
            const w = containerRef.current!.offsetWidth
            let percent = layerX / w
            if (step) {
                const diff = percent % step
                if (diff < step / 2) percent -= diff
                else percent += step - diff
            }
            setValue(percent * (max - min) + min)
        }
    }

    // TODO thumb is hidden
    return (
        <div className="flex flex-col select-none">
            <div className="flex flex-row justify-between">
                <p className="text-md">{label}</p>
                <p className="text-md float-right">
                    {format.prefix +
                        value.toLocaleString(locale, format) +
                        format.suffix}
                </p>
            </div>
            <div
                id="container"
                ref={containerRef}
                onMouseMove={ev => onMouseMove(ev)}
                onMouseDown={() => setMouseDown(true)}
                onMouseUp={() => setMouseDown(false)}
                className="relative w-full h-4 max-w-full cursor-pointer"
            >
                <div
                    id="background"
                    className="absolute bg-gray-500 w-full h-full rounded-lg"
                ></div>
                <div
                    id="color"
                    style={{ width: `max(calc(${getPercent()}%), 1rem)` }}
                    className="absolute bg-gradient-to-r from-orange-500 to-red-600 h-full rounded-lg"
                ></div>
                <div
                    id="handle"
                    style={{ width: `max(calc(${getPercent()}%), 1rem)` }}
                    className="hidden absolute w-4 h-4 bg-red-700 rounded-lg -translate-x-full"
                ></div>
            </div>
        </div>
    )
}

export default Slider
