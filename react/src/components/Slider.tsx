import React, { useRef, useState } from 'react';

type SliderProps = {
    min: number;
    max: number;
    defaultValue: number;
    step?: number;
    format?: string;
}

const Slider: React.FC<SliderProps> = ({ children, min, max, defaultValue, step, format }) => {
    const containerRef = useRef(null);
    const [value, setValue] = useState(defaultValue);
    const [mouseDown, setMouseDown] = useState(false);

    const getPercent = () => value / max * 100;

    const onMouseMove = (e) => {
        console.log(e);
        if (mouseDown) {
            let layerX = e.nativeEvent.layerX;
            let w = containerRef.current.offsetWidth;
            setValue(layerX / w);
            console.log(value);
        }
    }

    // TODO thumb is hidden
    return (
        <div id="container" ref={containerRef} className="relative w-full h-4" onMouseMove={(ev) => onMouseMove(ev)} onMouseDown={() => setMouseDown(true)} onMouseUp={() => setMouseDown(false)}>
            <div id="background" className="absolute bg-gray-500 w-full h-full rounded-lg"></div>
            <div id="color" className="absolute bg-gradient-to-r from-orange-500 to-red-600 h-full rounded-lg" style={{ width: `calc(100% * ${getPercent()})` }}></div>
            <div id="handle" className="hidden absolute w-4 h-4 bg-red-700 rounded-lg -translate-x-full" style={{ left: `calc(100% * ${getPercent()})` }}></div>
        </div>
    )
}

export default Slider;
