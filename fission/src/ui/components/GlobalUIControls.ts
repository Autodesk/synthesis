/**
 * This is where all the global references to the Global UI controls are located.
 * See GlobalUIComponent.tsx for explanation of this madness.
 */

import { ToastType } from "@/ui/ToastContext"

export let globalAddToast: (type: ToastType, title: string, description: string) => void = () => {}
export let globalOpenPanel: (panelId: string) => void = () => {}
export let globalOpenModal: (modalId: string) => void = () => {}

export function setAddToast(func: typeof globalAddToast) {
    globalAddToast = func
}
export function setOpenPanel(func: typeof globalOpenPanel) {
    globalOpenPanel = func
}
export function setOpenModal(func: typeof globalOpenModal) {
    globalOpenModal = func
}
