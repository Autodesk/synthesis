import { useState, createContext } from "react";
import { v4 as uuidv4 } from "uuid";
import type React from "react";
import type { ReactElement, ReactNode } from "react";

export type UIProviderProps = {
	children?: ReactNode;
};

export enum CloseType {
	Accept = 0,
	Cancel = 1,
	Overwrite = 2,
}

export type UIScreenProps = Partial<{
	onClose: (closeType: CloseType) => void;
	onCancel: () => void;
	onAccept: () => void;
	htmlProps: string;
}>;

export interface UIScreen {
	id: string;
	content: ReactElement;
	props: UIScreenProps;
}

export interface Modal extends UIScreen {
	allowClickAway: boolean;
}

export interface Panel extends UIScreen {
	position: string; // TODO: create enum
}

export type OpenModalFn = (
	contents: ReactElement,
	props?: UIScreenProps,
) => string; // TODO: do we need ID for modal?
export type OpenPanelFn = (
	contents: ReactElement,
	props?: UIScreenProps,
) => string;
export type CloseModalFn = (closeType: CloseType) => void;
export type ClosePanelFn = (id: string, closeType: CloseType) => void;

export type UIContextProps = {
	modal?: Modal;
	panels: Panel[];
	openModal: OpenModalFn;
	openPanel: OpenPanelFn;
	closeModal: CloseModalFn;
	closePanel: ClosePanelFn;
};

export const UIContext = createContext<UIContextProps>({
	panels: [],
	openModal: (_content, _props = {}) => "",
	openPanel: (_content, _props = {}) => "",
	closeModal: () => {},
	closePanel: (_id) => {},
});

export const UIProvider: React.FC<UIProviderProps> = ({ children }) => {
	const [modal, setModal] = useState<Modal | undefined>(undefined);
	const [panels, setPanels] = useState<Panel[]>([]);

	// TODO: add support for modal-specific props (i.e. allowClickAway)
	const openModal: OpenModalFn = (
		content: ReactElement,
		props: UIScreenProps = {},
	) => {
		const id = uuidv4();
		const modal = {
			id,
			content,
			props,
		} as Modal;
		modal?.props.onClose?.(CloseType.Overwrite);
		setModal(modal);
		return id;
	};

	const openPanel: OpenPanelFn = (
		content: ReactElement,
		props: UIScreenProps = {},
	) => {
		const id = uuidv4();
		const panel = {
			id,
			content,
			props,
		} as Panel;
		setPanels([...panels, panel]);
		return id;
	};

	const closeModal = (closeType: CloseType) => {
		modal?.props.onClose?.(closeType);
		setModal(undefined);
	};

	const closePanel = (id: string, closeType: CloseType) => {
		const panel = panels.find((p: Panel) => p.id === id);
		panel?.props.onClose?.(closeType);
		setPanels(panels.filter((p: Panel) => p.id !== id));
	};

	return (
		<UIContext.Provider
			value={{ modal, panels, openModal, openPanel, closeModal, closePanel }}
		>
			{children}
		</UIContext.Provider>
	);
};
