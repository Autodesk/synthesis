/**
 * This is where all the global references to the Global UI controls are located.
 * See GlobalUIComponent.tsx for explanation of this madness.
 */

import { ToastType } from "@/ui/ToastContext";

export let Global_AddToast: ((type: ToastType, title: string, description: string) => void) | undefined = undefined
export let Global_OpenPanel: ((panelId: string) => void) | undefined = undefined
export let Global_OpenModal: ((modalId: string) => void) | undefined = undefined

export function setAddToast(func: typeof Global_AddToast) { Global_AddToast = func }
export function setOpenPanel(func: typeof Global_OpenPanel) { Global_OpenPanel = func }
export function setOpenModal(func: typeof Global_OpenModal) { Global_OpenModal = func}
