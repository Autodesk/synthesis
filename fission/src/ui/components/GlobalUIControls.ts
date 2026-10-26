import type { AddToastFn, CloseModalFn, OpenModalFn, OpenPanelFn } from "../helpers/UIProviderHelpers"

/**
 * This is where all the global references to the Global UI controls are located.
 */

export let globalAddToast: AddToastFn = () => {}
export let globalOpenPanel: OpenPanelFn = () => null
export let globalOpenModal: OpenModalFn = () => null
export let globalCloseModal: CloseModalFn = () => {}

export function setAddToast(func: typeof globalAddToast) {
    globalAddToast = func
}
export function setOpenPanel(func: typeof globalOpenPanel) {
    globalOpenPanel = func
}
export function setOpenModal(func: typeof globalOpenModal) {
    globalOpenModal = func
}

export function setCloseModal(func: typeof globalCloseModal) {
    globalCloseModal = func
}
