import { useContext, useEffect } from "react";
import { UIContext } from "./UIProvider";
import type React from "react";
import type { ReactElement, ReactNode } from "react";

export type UIRendererProps = {
};

export const UIRenderer: React.FC<UIRendererProps> = () => {
	const { modal, openModal } = useContext(UIContext);
	useEffect(() => {
		console.log(
			`opening test modal ${openModal(<div className="absolute">test</div>, {
				onClose: () => {
					console.log("closed test modal");
				},
			})}`,
		);
	}, []);
	console.log(modal);
	return (
		<>
			{modal?.content}
		</>
	);
};
