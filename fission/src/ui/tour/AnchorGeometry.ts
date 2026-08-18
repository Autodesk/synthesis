export function clippingAncestors(element: HTMLElement): HTMLElement[] {
    const clippers: HTMLElement[] = []
    for (let parent = element.parentElement; parent; parent = parent.parentElement) {
        if (getComputedStyle(parent).overflow !== "visible") clippers.push(parent)
    }
    return clippers
}

export function visibleRect(element: HTMLElement, clippers = clippingAncestors(element)): DOMRect | null {
    const view = document.documentElement
    let { top, left, right, bottom } = element.getBoundingClientRect()
    top = Math.max(top, 0)
    left = Math.max(left, 0)
    bottom = Math.min(bottom, view.clientHeight)
    right = Math.min(right, view.clientWidth)

    for (const clipper of clippers) {
        const box = clipper.getBoundingClientRect()
        const innerTop = box.top + clipper.clientTop
        const innerLeft = box.left + clipper.clientLeft
        top = Math.max(top, innerTop)
        left = Math.max(left, innerLeft)
        bottom = Math.min(bottom, innerTop + clipper.clientHeight)
        right = Math.min(right, innerLeft + clipper.clientWidth)
    }

    return bottom <= top || right <= left ? null : new DOMRect(left, top, right - left, bottom - top)
}
