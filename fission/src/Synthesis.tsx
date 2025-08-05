import { AnimatePresence } from "framer-motion";
// <<<<<<< HEAD
// import type React from "react"
// import { type ReactElement, useCallback, useEffect, useRef, useState } from "react"
// =======
import { SnackbarProvider } from "notistack";
import { useCallback, useEffect, useRef, useState } from "react";
// >>>>>>> origin
import MainHUD from "@/components/MainHUD";
import Scene from "@/components/Scene.tsx";
import World from "@/systems/World.ts";
// <<<<<<< HEAD
// import { useModalManager } from "@/ui/helpers/UseModalManager.tsx"
// import { usePanelManager } from "@/ui/helpers/UsePanelManager.tsx"
// import { useTheme } from "@/ui/helpers/UseThemeHelpers.tsx"
// import { ModalControlProvider } from "@/ui/ModalContext"
// import { PanelControlProvider } from "@/ui/PanelContext"
// import ImportMirabufPanel from "@/ui/panels/mirabuf/ImportMirabufPanel.tsx"
// import { ToastContainer, ToastProvider } from "@/ui/ToastContext"
// import {
//     TOOLTIP_DURATION,
//     type TooltipControl,
//     TooltipControlProvider,
//     type TooltipType,
//     useTooltipManager,
// } from "@/ui/TooltipContext"
// =======
import { UIRenderer } from "@/ui/UIRenderer.tsx";
// >>>>>>> origin
import PreferencesSystem from "./systems/preferences/PreferencesSystem.ts";
import AnalyticsConsent from "./ui/components/AnalyticsConsent.tsx";
import ContextMenu from "./ui/components/ContextMenu.tsx";
import DragModeIndicator from "./ui/components/DragModeIndicator.tsx";
import GlobalUIComponent from "./ui/components/GlobalUIComponent.tsx";
import { globalOpenModal } from "./ui/components/GlobalUIControls.ts";
import ProgressNotifications from "./ui/components/ProgressNotification.tsx";
import SceneOverlay from "./ui/components/SceneOverlay.tsx";
import WPILibConnectionStatus from "./ui/components/WPILibConnectionStatus.tsx";
// <<<<<<< HEAD
import { applyInitialGraphicsSettings } from "./ui/helpers/GraphicsSettings.ts";
// import APSManagementModal from "./ui/modals/APSManagementModal.tsx"
// import AssignNewSchemeModal from "./ui/modals/configuring/theme-editor/AssignNewSchemeModal.tsx"
// import NewInputSchemeModal from "./ui/modals/configuring/theme-editor/NewInputSchemeModal.tsx"
// import ConfigurePanel from "./ui/panels/configuring/assembly-config/ConfigurePanel.tsx"
// import CameraSelectionPanel from "./ui/panels/configuring/CameraSelectionPanel.tsx"
// import ChooseInputSchemePanel from "./ui/panels/configuring/ChooseInputSchemePanel.tsx"
// import InitialConfigPanel from "./ui/panels/configuring/initial-config/InitialConfigPanel.tsx"
// import MatchModeConfigPanel from "./ui/panels/configuring/MatchModeConfigPanel.tsx"
// import DebugPanel from "./ui/panels/DebugPanel.tsx"
// import DeveloperToolPanel from "./ui/panels/DeveloperToolPanel.tsx"
// import GraphicsSettings from "./ui/panels/GraphicsSettingsPanel.tsx"
// import AutoTestPanel from "./ui/panels/simulation/AutoTestPanel.tsx"
// import WiringPanel from "./ui/panels/simulation/WiringPanel.tsx"
// import WSViewPanel from "./ui/panels/WSViewPanel.tsx"

// const Synthesis: React.FC = () => {
//     const { openModal, closeModal, getActiveModalElement, registerModal, activeModalId } =
//         useModalManager(initialModals)
//     const { openPanel, closePanel, closeAllPanels, getActivePanelElements } = usePanelManager(initialPanels)
//     const { showTooltip } = useTooltipManager()
// =======
import MainMenuModal from "./ui/modals/MainMenuModal.tsx";
import { StateProvider } from "./ui/StateProvider.tsx";
import { ThemeProvider } from "./ui/ThemeProvider.tsx";
import { UIProvider } from "./ui/UIProvider.tsx";

// >>>>>>> origin

function Synthesis() {
	const [consentPopupDisable, setConsentPopupDisable] = useState<boolean>(true);

	const mainLoopHandle = useRef(0);
	// <<<<<<< HEAD
	//     registerModal("main-menu", {
	//         id: "main-menu",
	//         component: (
	//             <MainMenuModal
	//                 key="main-menu"
	//                 modalId="main-menu"
	//                 startSingleplayerCallback={() => {
	//                     World.initWorld()

	//                     applyInitialGraphicsSettings()

	//                     if (!PreferencesSystem.getGlobalPreference("ReportAnalytics") && !import.meta.env.DEV) {
	//                         setConsentPopupDisable(false)
	//                     }

	//                     const mainLoop = () => {
	//                         mainLoopHandle.current = requestAnimationFrame(mainLoop)
	//                         World.updateWorld()
	//                     }
	//                     mainLoop()

	//                     World.sceneRenderer.updateSkyboxColors(defaultTheme)
	//                 }}
	//             />
	//         ),
	//     })
	// =======
	// >>>>>>> origin

	useEffect(() => {
		const urlParams = new URLSearchParams(document.location.search);
		if (urlParams.has("code")) {
			window.opener.convertAuthToken(urlParams.get("code"));
			window.close();
			return;
		}
		const startSingleplayerCallback = () => {
			World.initWorld();

			applyInitialGraphicsSettings();

			if (
				!PreferencesSystem.getGlobalPreference("ReportAnalytics") &&
				!import.meta.env.DEV
			) {
				setConsentPopupDisable(false);
			}

			const mainLoop = () => {
				mainLoopHandle.current = requestAnimationFrame(mainLoop);
				World.updateWorld();
			};

			mainLoop();
		};
		globalOpenModal(MainMenuModal, { startSingleplayerCallback });
		// Cleanup
		return () => {
			// TODO: Teardown literally everything
			cancelAnimationFrame(mainLoopHandle.current);
			World.destroyWorld();
			// World.SceneRenderer.RemoveAllSceneObjects();
		};
	}, []);

	const onConsent = useCallback(() => {
		setConsentPopupDisable(true);
		PreferencesSystem.setGlobalPreference("ReportAnalytics", true);
		PreferencesSystem.savePreferences();
	}, []);

	const onDisableConsent = useCallback(() => {
		setConsentPopupDisable(true);
	}, []);

	return (
		<AnimatePresence key={"animate-presence"}>
			<ThemeProvider>
				<SnackbarProvider
					maxSnack={5}
					anchorOrigin={{ horizontal: "right", vertical: "bottom" }}
				>
					<StateProvider>
						<UIProvider>
							<GlobalUIComponent />
							<Scene
								useStats={import.meta.env.DEV}
								key="scene-in-toast-provider"
							/>
							<SceneOverlay />
							<ContextMenu />
							<MainHUD key={"main-hud"} />
							<UIRenderer />
							<ProgressNotifications key={"progress-notifications"} />
							<WPILibConnectionStatus />
							<DragModeIndicator />

							{!consentPopupDisable && (
								<AnalyticsConsent
									onClose={onDisableConsent}
									onConsent={onConsent}
								/>
							)}
						</UIProvider>
					</StateProvider>
				</SnackbarProvider>
			</ThemeProvider>
		</AnimatePresence>
	);
}

export default Synthesis;
