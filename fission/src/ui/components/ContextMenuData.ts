// DATA

export interface ContextItem {
    name: string
    func: () => void
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
    private static KEY: string = "ContextSupplierEvent"

    private _data: ContextData
    private _mouseEvent: MouseEvent

    public get data() { return this._data }
    public get mouseEvent() { return this._mouseEvent }

    private constructor(data: ContextData, mouseEvent: MouseEvent) {
        super(ContextSupplierEvent.KEY)

        this._data = data
        this._mouseEvent = mouseEvent

        window.dispatchEvent(this)
    }

    public static Dispatch(data: ContextData, mouseEvent: MouseEvent) { new ContextSupplierEvent(data, mouseEvent) }
    public static Listen(func: (e: ContextSupplierEvent) => void) { window.addEventListener(ContextSupplierEvent.KEY, func as (e: Event) => void) }
    public static RemoveListener(func: (e: ContextSupplierEvent) => void) { window.removeEventListener(ContextSupplierEvent.KEY, func as (e: Event) => void) }
}