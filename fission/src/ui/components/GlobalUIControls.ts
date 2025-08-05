import type { AddToastFn, OpenModalFn, OpenPanelFn } from "../helpers/UIProviderHelpers"

/**
 * This is where all the global references to the Global UI controls are located.
 * See GlobalUIComponent.tsx for explanation of this madness.
 */

export let globalAddToast: AddToastFn = () => {}
export let globalOpenPanel: OpenPanelFn = () => ""
export let globalOpenModal: OpenModalFn = () => ""

export function setAddToast(func: typeof globalAddToast) {
    globalAddToast = func
}
export function setOpenPanel(func: typeof globalOpenPanel) {
    globalOpenPanel = func
}
export function setOpenModal(func: typeof globalOpenModal) {
    globalOpenModal = func
}
