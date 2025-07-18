export const click = (btn: number, x: number, y: number) => {
    const el = document.elementFromPoint(x, y)

    const event = new MouseEvent("click", {
        clientX: x,
        clientY: y,
        bubbles: true,
        button: btn,
    })
    el?.dispatchEvent(event)
}

export const mousePosition = (x: number, y: number) => {
    const el = document.elementFromPoint(x, y)

    const event = new MouseEvent("mouseover", {
        view: window,
        bubbles: true,
        cancelable: true,
    })

    el?.dispatchEvent(event)
}

// biome-ignore-start lint/suspicious/noExplicitAny: We need to index a generic object
export const addGlobalFunc = <T>(name: string, func: (...args: any[]) => T) => {
    // biome-ignore format: The semicolon is not necessary
    (window as any)[name] = func
}
// biome-ignore-end lint/suspicious/noExplicitAny: We need to index a generic object

addGlobalFunc("click", click)
addGlobalFunc("mousePosition", mousePosition)
