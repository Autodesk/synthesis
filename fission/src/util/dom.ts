export const click = (btn: number, x: number, y: number) => {
    const el = document.elementFromPoint(x, y);

    const event = new MouseEvent("click", {
        clientX: x,
        clientY: y,
        bubbles: true,
        button: btn
    })
    el?.dispatchEvent(event)
}

export const mousePosition = (x: number, y: number) => {
    const el = document.elementFromPoint(x, y);

    const event = new MouseEvent('mouseover', {
        view: window,
        bubbles: true,
        cancelable: true
    })

    el?.dispatchEvent(event);
}

export const addGlobalFunc = <T>(name: string, func: (...args: any[]) => T) => {
    (window as any)[name] = func;
}

addGlobalFunc('click', click);
addGlobalFunc('mousePosition', mousePosition);
