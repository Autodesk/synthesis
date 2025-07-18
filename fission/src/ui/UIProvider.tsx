import type { VariantType } from "notistack";
import { useSnackbar } from "notistack";
import type React from "react";
import type { ReactElement, ReactNode } from "react";
import { createContext, useCallback, useState } from "react";
import { v4 as uuidv4 } from "uuid";

export type UIProviderProps = {
	children?: ReactNode;
};

export enum CloseType {
	Accept = 0,
	Cancel = 1,
	Overwrite = 2,
}

export interface UIScreenProps<T> {
	onClose?: (closeType: CloseType) => void;
	onCancel?: () => void;
	onAccept?: (x?: T) => void;
	onBeforeAccept?: () => T;
	htmlProps?: string;
	hideCancel?: boolean;
	hideAccept?: boolean;
    cancelText?: string
    acceptText?: string
}

export interface ModalProps<T> extends UIScreenProps<T> {
	allowClickAway?: boolean;
}

export interface PanelProps<T> extends UIScreenProps<T> {}

export interface UIScreen<T> {
	id: string;
	parent: UIScreen<unknown>;
	content: ReactElement;
	props: UIScreenProps<T>;
}

export type PanelPosition =
	| "top-left"
	| "top"
	| "top-right"
	| "left"
	| "center"
	| "right"
	| "bottom-left"
	| "bottom"
	| "bottom-right";

export interface Modal<T> extends UIScreen<T> {}

export interface Panel<T> extends UIScreen<T> {
	position: PanelPosition;
}

export type OpenModalFn = <T>(
	contents: ReactElement,
	parent?: UIScreen<T>,
	props?: ModalProps<T>,
) => string;
export type OpenPanelFn = <T>(
	contents: ReactElement,
	parent?: UIScreen<T>,
	position?: PanelPosition,
	props?: UIScreenProps<T>,
) => string;
export type CloseModalFn = (closeType: CloseType) => void;
export type ClosePanelFn = (id: string, closeType: CloseType) => void;
export type AddToastFn = (variant: VariantType, ...contents: string[]) => void;

export type UIContextProps = {
	modal?: Modal<unknown>;
	panels: Panel<unknown>[];
	openModal: OpenModalFn;
	openPanel: OpenPanelFn;
	closeModal: CloseModalFn;
	closePanel: ClosePanelFn;
	addToast: AddToastFn;
};

export const UIContext = createContext<UIContextProps>({
	panels: [],
	openModal: (
		_content,
		_parent,
		_props = { hideAccept: false, hideCancel: false },
	) => "",
	openPanel: (
		_content,
		_parent,
		_position = "center",
		_props = { hideAccept: false, hideCancel: false },
	) => "",
	closeModal: () => {},
	closePanel: (_id) => {},
	addToast: (_variant, _msg) => "",
});

export const UIProvider: React.FC<UIProviderProps> = ({ children }) => {
	const [modal, setModal] = useState<Modal<unknown> | undefined>(undefined);
	const [panels, setPanels] = useState<Panel<unknown>[]>([]);

	const { enqueueSnackbar } = useSnackbar();

	const openModal: OpenModalFn = useCallback(
		<T,>(
			content: ReactElement,
			parent?: UIScreen<T>,
			props: ModalProps<T> = {
				hideAccept: false,
				hideCancel: false,
                acceptText: "Accept",
                cancelText: "Cancel",
			},
		) => {
			const id = uuidv4();
			const newModal = {
				id,
				parent,
				content,
				props,
			} as Modal<T>;
			modal?.props.onClose?.(CloseType.Overwrite);
			setModal(newModal as Modal<unknown>);
			return id;
		},
		[modal],
	);

	const openPanel: OpenPanelFn = useCallback(
		<T,>(
			content: ReactElement,
			parent?: UIScreen<T>,
			position: PanelPosition = "center",
			props: UIScreenProps<T> = {
				hideAccept: false,
				hideCancel: false,
                acceptText: "Accept",
                cancelText: "Cancel",
			},
		) => {
			const id = uuidv4();
			const panel = {
				id,
				parent,
				content,
				position,
				props,
			} as Panel<T>;
			setPanels([...panels, panel as Panel<unknown>]);
			return id;
		},
		[panels],
	);

	const closeCallbacks = <T,>(
		elem: Panel<T> | Modal<T>,
		closeType: CloseType,
	) => {
		elem.props.onClose?.(closeType);
		switch (closeType) {
			case CloseType.Accept: {
				const beforeAcceptResult = elem.props.onBeforeAccept?.();
				elem.props.onAccept?.(beforeAcceptResult);
				break;
			}
			case CloseType.Cancel:
				elem.props.onCancel?.();
				break;
			default:
				break;
		}
	};

	const closeModal = useCallback(
		<T,>(closeType: CloseType) => {
			if (modal) closeCallbacks<T>(modal as Modal<T>, closeType);
			setModal(undefined);
		},
		[modal],
	);

	const closePanel = useCallback(
		(id: string, closeType: CloseType) => {
			setPanels((p) => {
				const panel = p.find((p: Panel<unknown>) => p.id === id);
				if (panel) closeCallbacks(panel, closeType);
				return p.filter((pnl: Panel<unknown>) => pnl.id !== id);
			});
		},
		[panels],
	);

	const addToast = useCallback(
		(variant: VariantType, ...contents: string[]) => {
			enqueueSnackbar(contents.join("\n"), { variant });
		},
		[],
	);

	return (
		<UIContext.Provider
			value={{
				modal,
				panels,
				openModal,
				openPanel,
				closeModal,
				closePanel,
				addToast,
			}}
		>
			{children}
		</UIContext.Provider>
	);
};
