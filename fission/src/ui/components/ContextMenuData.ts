// DATA

import type { FunctionComponent } from "react"

export interface ContextItem {
    name: string
    customProps?: Record<string, unknown>
    screen?: FunctionComponent
    type?: "panel" | "modal"
    func?: () => void
}

export interface ContextData {
    title: string
    items: ContextItem[]
}

export interface ContextSupplier {
    getSupplierData(): ContextData
}

// EVENTS

export class ContextSupplierEvent extends Event {
    private static readonly KEY: string = "ContextSupplierEvent"

    private _data: ContextData
    private _mousePosition: [number, number]

    public get data() {
        return this._data
    }
    public get mousePosition() {
        return this._mousePosition
    }

    private constructor(data: ContextData, mousePosition: [number, number]) {
        super(ContextSupplierEvent.KEY)

        this._data = data
        this._mousePosition = mousePosition

        window.dispatchEvent(this)
    }

    public static dispatch(data: ContextData, mousePosition: [number, number]) {
        new ContextSupplierEvent(data, mousePosition)
    }
    public static listen(func: (e: ContextSupplierEvent) => void) {
        window.addEventListener(ContextSupplierEvent.KEY, func as (e: Event) => void)
    }
    public static removeListener(func: (e: ContextSupplierEvent) => void) {
        window.removeEventListener(ContextSupplierEvent.KEY, func as (e: Event) => void)
    }
}
