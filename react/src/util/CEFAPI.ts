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
