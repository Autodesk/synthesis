// DATA

import type { FunctionComponent } from "react"

export interface ContextItem {
    name: string
    customProps?: Record<string, unknown>
    /** Additional props forwarded to openPanel (e.g. blocking, blockingMessage) */
    panelProps?: Record<string, unknown>
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
