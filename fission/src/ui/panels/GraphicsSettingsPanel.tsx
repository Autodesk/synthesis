import {
	Box,
	Button,
	FormControl,
	InputLabel,
	MenuItem,
	Select,
	Stack,
} from "@mui/material";
import type React from "react";
import { useEffect, useState } from "react";
import PreferencesSystem from "@/systems/preferences/PreferencesSystem";
import World from "@/systems/World";
import Checkbox from "../components/Checkbox";
import Label from "../components/Label";
import type { PanelImplProps } from "../components/Panel";
import StatefulSlider from "../components/StatefulSlider";
import {
	autoOptimizeGraphics,
	GRAPHICS_PRESETS,
	type GraphicsPreset,
} from "../helpers/GraphicsSettings";
import { useUIContext } from "../helpers/UIProviderHelpers";

const MIN_LIGHT_INTENSITY = 1;
const MAX_LIGHT_INTENSITY = 10;

const MIN_MAX_FAR = 10;
const MAX_MAX_FAR = 40;

const MIN_CASCADES = 3;
const MAX_CASCADES = 8;

const MIN_SHADOW_MAP_SIZE = 1024;

const detectCurrentPreset = (
	lightIntensity: number,
	fancyShadows: boolean,
	maxFar: number,
	cascades: number,
	shadowMapSize: number,
	antiAliasing: boolean,
): string => {
	for (const [presetName, presetSettings] of Object.entries(GRAPHICS_PRESETS)) {
		if (
			presetSettings.lightIntensity === lightIntensity &&
			presetSettings.fancyShadows === fancyShadows &&
			presetSettings.maxFar === maxFar &&
			presetSettings.cascades === cascades &&
			presetSettings.shadowMapSize === shadowMapSize &&
			presetSettings.antiAliasing === antiAliasing
		) {
			return presetName;
		}
	}
	return "Custom";
};

const GraphicsSettingsPanel: React.FC<PanelImplProps<void, void>> = ({
	panel,
}) => {
	const { configureScreen } = useUIContext();
	const [reload, setReload] = useState<boolean>(false);
	const [lightIntensity, setLightIntensity] = useState<number>(
		PreferencesSystem.getGraphicsPreferences().lightIntensity,
	);
	const [fancyShadows, setFancyShadows] = useState<boolean>(
		PreferencesSystem.getGraphicsPreferences().fancyShadows,
	);
	const [maxFar, setMaxFar] = useState<number>(
		PreferencesSystem.getGraphicsPreferences().maxFar,
	);
	const [cascades, setCascades] = useState<number>(
		PreferencesSystem.getGraphicsPreferences().cascades,
	);
	const [shadowMapSize, setShadowMapSize] = useState<number>(
		PreferencesSystem.getGraphicsPreferences().shadowMapSize,
	);
	const [antiAliasing, setAntiAliasing] = useState<boolean>(
		PreferencesSystem.getGraphicsPreferences().antiAliasing,
	);

	const initialPreset = detectCurrentPreset(
		PreferencesSystem.getGraphicsPreferences().lightIntensity,
		PreferencesSystem.getGraphicsPreferences().fancyShadows,
		PreferencesSystem.getGraphicsPreferences().maxFar,
		PreferencesSystem.getGraphicsPreferences().cascades,
		PreferencesSystem.getGraphicsPreferences().shadowMapSize,
		PreferencesSystem.getGraphicsPreferences().antiAliasing,
	);

	const [selectedPreset, setSelectedPreset] = useState<string>(initialPreset);

	const updatePresetFromSettings = (
		newLightIntensity: number,
		newFancyShadows: boolean,
		newMaxFar: number,
		newCascades: number,
		newShadowMapSize: number,
		newAntiAliasing: boolean,
	) => {
		const detectedPreset = detectCurrentPreset(
			newLightIntensity,
			newFancyShadows,
			newMaxFar,
			newCascades,
			newShadowMapSize,
			newAntiAliasing,
		);
		if (detectedPreset !== selectedPreset) {
			setSelectedPreset(detectedPreset);
		}
	};

	const updateCSMSettingsAndPreset = (
		newLightIntensity: number,
		newFancyShadows: boolean,
		newMaxFar: number,
		newCascades: number,
		newShadowMapSize: number,
		newAntiAliasing: boolean,
	) => {
		World.sceneRenderer.changeCSMSettings({
			lightIntensity: newLightIntensity,
			fancyShadows: newFancyShadows,
			maxFar: newMaxFar,
			cascades: newCascades,
			shadowMapSize: newShadowMapSize,
			antiAliasing: newAntiAliasing,
		});
		updatePresetFromSettings(
			newLightIntensity,
			newFancyShadows,
			newMaxFar,
			newCascades,
			newShadowMapSize,
			newAntiAliasing,
		);
	};

	const applyPreset = (preset: GraphicsPreset) => {
		const settings = GRAPHICS_PRESETS[preset];
		const previousAntiAliasing = antiAliasing;

		setLightIntensity(settings.lightIntensity);
		setFancyShadows(settings.fancyShadows);
		setMaxFar(settings.maxFar);
		setCascades(settings.cascades);
		setShadowMapSize(settings.shadowMapSize);
		setAntiAliasing(settings.antiAliasing);
		setSelectedPreset(preset);

		// Apply settings to scene renderer immediately
		World.sceneRenderer.changeLighting(settings.fancyShadows);
		World.sceneRenderer.setLightIntensity(settings.lightIntensity);

		World.sceneRenderer.changeCSMSettings({
			lightIntensity: settings.lightIntensity,
			fancyShadows: settings.fancyShadows,
			maxFar: settings.maxFar,
			cascades: settings.cascades,
			shadowMapSize: settings.shadowMapSize,
			antiAliasing: settings.antiAliasing,
		});

		if (previousAntiAliasing !== settings.antiAliasing) {
			setReload(true);
		}
	};

	const handleAntiAliasingChange = (checked: boolean) => {
		setAntiAliasing(checked);
		setReload(true);
		updatePresetFromSettings(
			lightIntensity,
			fancyShadows,
			maxFar,
			cascades,
			shadowMapSize,
			checked,
		);
	};

	useEffect(() => {
		const onBeforeAccept = () => {
			PreferencesSystem.getGraphicsPreferences().fancyShadows = fancyShadows;
			PreferencesSystem.getGraphicsPreferences().lightIntensity =
				lightIntensity;
			PreferencesSystem.getGraphicsPreferences().maxFar = maxFar;
			PreferencesSystem.getGraphicsPreferences().cascades = cascades;
			PreferencesSystem.getGraphicsPreferences().shadowMapSize = shadowMapSize;
			PreferencesSystem.getGraphicsPreferences().antiAliasing = antiAliasing;

			PreferencesSystem.savePreferences();

			if (reload) window.location.reload();
		};
		const onCancel = () => {
			// Revert all settings to original preferences
			const originalPrefs = PreferencesSystem.getGraphicsPreferences();
			World.sceneRenderer.setLightIntensity(originalPrefs.lightIntensity);
			World.sceneRenderer.changeLighting(originalPrefs.fancyShadows);
			World.sceneRenderer.changeCSMSettings({
				lightIntensity: originalPrefs.lightIntensity,
				fancyShadows: originalPrefs.fancyShadows,
				maxFar: originalPrefs.maxFar,
				cascades: originalPrefs.cascades,
				shadowMapSize: originalPrefs.shadowMapSize,
				antiAliasing: originalPrefs.antiAliasing,
			});
		};

		if (panel) {
			configureScreen(
				panel,
				{ title: "Graphics Settings", position: "center" },
				{ onBeforeAccept, onCancel },
			);
		}
	}, [
		fancyShadows,
		lightIntensity,
		maxFar,
		cascades,
		shadowMapSize,
		antiAliasing,
		reload,
		panel,
		configureScreen,
	]);

	return (
		<Stack gap={2}>
			<Label size="md">Graphics Presets</Label>
			<FormControl fullWidth>
				<InputLabel>Preset</InputLabel>
				<Select
					value={selectedPreset}
					label="Preset"
					onChange={(e) => {
						const value = e.target.value as string;
						if (value === "Fast" || value === "Balanced" || value === "Fancy") {
							applyPreset(value as GraphicsPreset);
						} else if (value === "Custom") {
							setSelectedPreset("Custom");
						}
					}}
				>
					<MenuItem value="Fast">Fast</MenuItem>
					<MenuItem value="Balanced">Balanced</MenuItem>
					<MenuItem value="Fancy">Fancy</MenuItem>
					<MenuItem value="Custom">Custom</MenuItem>
				</Select>
			</FormControl>
			<Box alignSelf="center">
				<Button
					onClick={() => {
						const preset = autoOptimizeGraphics("short");
						applyPreset(preset);
					}}
					variant="contained"
				>
					Auto Optimize
				</Button>
			</Box>

			<StatefulSlider
				key={`light-intensity-${selectedPreset}`}
				label="Light Intensity"
				min={MIN_LIGHT_INTENSITY}
				max={MAX_LIGHT_INTENSITY}
				defaultValue={lightIntensity}
				valueLabelFormat={(val, _idx) => val.toFixed(2)}
				onChange={(value) => {
					const newValue = value as number;
					setLightIntensity(newValue);
					World.sceneRenderer.setLightIntensity(newValue);
					updatePresetFromSettings(
						newValue,
						fancyShadows,
						maxFar,
						cascades,
						shadowMapSize,
						antiAliasing,
					);
				}}
				step={0.25}
			/>
			<Checkbox
				label="Fancy Shadows"
				checked={fancyShadows}
				onClick={(checked) => {
					setFancyShadows(checked);
					World.sceneRenderer.changeLighting(checked);
					World.sceneRenderer.setLightIntensity(lightIntensity);
					updatePresetFromSettings(
						lightIntensity,
						checked,
						maxFar,
						cascades,
						shadowMapSize,
						antiAliasing,
					);
				}}
			/>
			{fancyShadows && (
				<>
					<StatefulSlider
						key={`max-far-${selectedPreset}`}
						label="Max Far"
						min={MIN_MAX_FAR}
						max={MAX_MAX_FAR}
						defaultValue={maxFar}
						onChange={(value) => {
							const newValue = value as number;
							setMaxFar(newValue);
							updateCSMSettingsAndPreset(
								lightIntensity,
								fancyShadows,
								newValue,
								cascades,
								shadowMapSize,
								antiAliasing,
							);
						}}
						step={1}
					/>
					<StatefulSlider
						key={`cascades-${selectedPreset}`}
						label="Cascade Count"
						min={MIN_CASCADES}
						max={MAX_CASCADES}
						defaultValue={cascades}
						onChange={(value) => {
							const newValue = value as number;
							setCascades(newValue);
							updateCSMSettingsAndPreset(
								lightIntensity,
								fancyShadows,
								maxFar,
								newValue,
								shadowMapSize,
								antiAliasing,
							);
						}}
						step={1}
					/>
					<StatefulSlider
						key={`shadow-map-size-${selectedPreset}`}
						label="Shadow Map Size"
						min={MIN_SHADOW_MAP_SIZE}
						max={World.sceneRenderer.renderer.capabilities.maxTextureSize}
						defaultValue={shadowMapSize}
						onChange={(value) => {
							const newValue = value as number;
							setShadowMapSize(newValue);
							updateCSMSettingsAndPreset(
								lightIntensity,
								fancyShadows,
								maxFar,
								cascades,
								newValue,
								antiAliasing,
							);
						}}
						step={1024}
					/>
					<Box alignSelf="center">
						<Button
							onClick={() => {
								setShadowMapSize(4096);
								setMaxFar(30);
								setLightIntensity(5);
								setCascades(4);

								World.sceneRenderer.changeCSMSettings({
									shadowMapSize: 4096,
									maxFar: 30,
									lightIntensity: 5,
									fancyShadows,
									cascades: 4,
									antiAliasing,
								});
							}}
						>
							Reset Default
						</Button>
					</Box>
				</>
			)}
			<Label size="sm">Requires Browser Refresh</Label>
			<Checkbox
				label="Anti-Aliasing"
				checked={antiAliasing}
				onClick={handleAntiAliasingChange}
			/>
		</Stack>
	);
};

export default GraphicsSettingsPanel;
