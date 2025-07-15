import {
	Box,
	Button,
	Divider,
	Stack,
	ToggleButton,
	ToggleButtonGroup,
	Typography,
} from "@mui/material";
import type React from "react";
import {
	type ReactNode,
	useCallback,
	useContext,
	useEffect,
	useLayoutEffect,
	useMemo,
	useState,
} from "react";
import {
	type Data,
	getMirabufFiles,
	hasMirabufFiles,
	MirabufFilesStatusUpdateEvent,
	MirabufFilesUpdateEvent,
	requestMirabufFiles,
} from "@/aps/APSDataManagement";
import MirabufCachingService, {
	backUpFields,
	backUpRobots,
	canOPFS,
	type MirabufCacheInfo,
	type MirabufRemoteInfo,
	MiraType,
} from "@/mirabuf/MirabufLoader";
import { createMirabuf } from "@/mirabuf/MirabufSceneObject";
import { PAUSE_REF_ASSEMBLY_SPAWNING } from "@/systems/physics/PhysicsSystem";
import { SoundPlayer } from "@/systems/sound/SoundPlayer";
import World from "@/systems/World";
import {
	globalAddToast,
	globalOpenPanel,
} from "@/ui/components/GlobalUIControls";
import type { PanelImplProps } from "@/ui/components/Panel";
import { ProgressHandle } from "@/ui/components/ProgressNotificationData";
import {
    AddButton,
	DeleteButton,
	PositiveButton,
	RefreshButton,
	SynthesisIcons,
} from "@/ui/components/StyledComponents";
import { StateContext } from "@/ui/StateProvider";
import { CloseType, UIContext } from "@/ui/UIProvider";
import type TaskStatus from "@/util/TaskStatus";
import InitialConfigPanel from "../configuring/initial-config/InitialConfigPanel";

interface ItemCardProps {
	id: string;
	name: string;
	primaryButtonNode: ReactNode;
	primaryOnClick: () => void;
	secondaryOnClick?: () => void;
}

const ItemCard: React.FC<ItemCardProps> = ({
	id,
	name,
	primaryButtonNode,
	primaryOnClick,
	secondaryOnClick,
}) => {
	return (
		<Stack
			key={id}
			justifyContent={"space-between"}
			alignItems={"center"}
			gap={"1rem"}
			direction="row"
		>
			<Typography className="text-wrap break-all">
				{name.replace(/.mira$/, "")}
			</Typography>
			<Stack
				key={`button-box-${id}`}
				direction="row-reverse"
				gap={"0.25rem"}
				justifyContent={"center"}
				alignItems={"center"}
			>
                {AddButton(primaryOnClick)}
				{secondaryOnClick && DeleteButton(secondaryOnClick)}
			</Stack>
		</Stack>
	);
};

export type MiraManifest = {
	robots: MirabufRemoteInfo[];
	fields: MirabufRemoteInfo[];
};

function getCacheInfo(miraType: MiraType): MirabufCacheInfo[] {
	return Object.values(
		canOPFS
			? MirabufCachingService.getCacheMap(miraType)
			: miraType === MiraType.ROBOT
				? backUpRobots
				: backUpFields,
	);
}

function spawnCachedMira(
	info: MirabufCacheInfo,
	type: MiraType,
	progressHandle?: ProgressHandle,
) {
	// If spawning a field, then remove all other fields
	if (type === MiraType.FIELD) {
		World.sceneRenderer.removeAllFields();
	}

	if (!progressHandle) {
		progressHandle = new ProgressHandle(info.name ?? info.cacheKey);
	}

	World.physicsSystem.holdPause(PAUSE_REF_ASSEMBLY_SPAWNING);
	MirabufCachingService.get(info.id, type)
		.then((assembly) => {
			if (assembly) {
				createMirabuf(assembly).then((x) => {
					if (x) {
						World.sceneRenderer.registerSceneObject(x);
						progressHandle.done();

						globalOpenPanel(<InitialConfigPanel />);
					} else {
						progressHandle.fail();
					}
				});

				if (!info.name)
					MirabufCachingService.cacheInfo(
						info.cacheKey,
						type,
						assembly.info?.name ?? undefined,
					);
			} else {
				progressHandle.fail();
				console.error("Failed to spawn robot");
			}
		})
		.catch(() => progressHandle.fail())
		.finally(() => {
			setTimeout(
				() => World.physicsSystem.releasePause(PAUSE_REF_ASSEMBLY_SPAWNING),
				500,
			);
		});
}

const ImportMirabufPanel: React.FC<PanelImplProps> = ({ panel, parent }) => {
	const { addToast, closePanel, openModal } = useContext(UIContext);
	const { unconfirmedImport, configurationType, setConfigurationType } =
		useContext(StateContext);

	const [cachedRobots, setCachedRobots] = useState(
		getCacheInfo(MiraType.ROBOT),
	);
	const [cachedFields, setCachedFields] = useState(
		getCacheInfo(MiraType.FIELD),
	);

	const [manifest, setManifest] = useState<MiraManifest | undefined>();
	const [viewType, setViewType] = useState<MiraType>(MiraType.ROBOT);

	const [filesStatus, setFilesStatus] = useState<TaskStatus>({
		isDone: false,
		message: "Waiting on APS...",
	});
	const [files, setFiles] = useState<Data[] | undefined>(undefined);

	useEffect(() => {
		const updateFilesStatus = (e: Event) => {
			setFilesStatus((e as MirabufFilesStatusUpdateEvent).status);
		};

		const updateFiles = (e: Event) => {
			setFiles((e as MirabufFilesUpdateEvent).data);
		};

		window.addEventListener(
			MirabufFilesStatusUpdateEvent.EVENT_KEY,
			updateFilesStatus,
		);
		window.addEventListener(MirabufFilesUpdateEvent.EVENT_KEY, updateFiles);

		return () => {
			window.removeEventListener(
				MirabufFilesStatusUpdateEvent.EVENT_KEY,
				updateFilesStatus,
			);
			window.removeEventListener(
				MirabufFilesUpdateEvent.EVENT_KEY,
				updateFiles,
			);
		};
	});

	useEffect(() => {
		if (!hasMirabufFiles()) {
			requestMirabufFiles();
		} else {
			setFiles(getMirabufFiles());
		}
	}, []);

	useLayoutEffect(() => {
		if (unconfirmedImport) {
			closePanel(panel!.id, CloseType.Cancel);
			globalAddToast(
				"warning",
				"You're already importing a model!\nConfirm that one before importing another.",
			);
			return;
		}
		// TODO: validate behaviour
		if (parent) closePanel(parent.id, CloseType.Cancel);
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, []);

	// Get Default Mirabuf Data, Load into manifest.
	useEffect(() => {
		// To remove the prettier warning
		const x = async () => {
			fetch(`/api/mira/manifest.json`)
				.then((x) => x.json())
				.then((x) => {
					const map = MirabufCachingService.getCacheMap(MiraType.ROBOT);
					const robots: MirabufRemoteInfo[] = [];
					for (const src of x.robots) {
						if (typeof src === "string") {
							const str = `/api/mira/robots/${src}`;
							if (!map[str]) robots.push({ displayName: src, src: str });
						} else {
							if (!map[src.src])
								robots.push({ displayName: src.displayName, src: src.src });
						}
					}
					const fields: MirabufRemoteInfo[] = [];
					for (const src of x.fields) {
						if (typeof src === "string") {
							const str = `/api/mira/fields/${src}`;
							if (!map[str]) fields.push({ displayName: src, src: str });
						} else {
							if (!map[src.src])
								fields.push({ displayName: src.displayName, src: src.src });
						}
					}
					setManifest({
						robots,
						fields,
					});
				});
		};
		x();
	}, []);

	// Select a mirabuf assembly from the cache.
	const selectCache = useCallback(
		(info: MirabufCacheInfo, type: MiraType) => {
			spawnCachedMira(info, type);

			// TODO:
			// showTooltip("controls", [
			//     { control: "WASD", description: "Drive" },
			//     { control: "E", description: "Intake" },
			//     { control: "Q", description: "Dispense" },
			// ])

			if (panel) closePanel(panel.id, CloseType.Cancel);
		},
		[closePanel, panel],
	);

	// Cache a selected remote mirabuf assembly, load from cache.
	const selectRemote = useCallback(
		(info: MirabufRemoteInfo, type: MiraType) => {
			const status = new ProgressHandle(info.displayName);
			status.update("Downloading from Synthesis...", 0.05);

			MirabufCachingService.cacheRemote(info.src, type)
				.then((cacheInfo) => {
					if (cacheInfo) {
						spawnCachedMira(cacheInfo, type, status);
					} else {
						status.fail("Failed to cache");
					}
				})
				.catch(() => status.fail());

			if (panel) closePanel(panel.id, CloseType.Cancel);
		},
		[closePanel, panel],
	);

	// Cache a selected remote mirabuf assembly, without load.
	const cacheRemoteOnly = useCallback(
		(info: MirabufRemoteInfo, type: MiraType) => {
			const status = new ProgressHandle(info.displayName);
			status.update("Downloading from Synthesis...", 0.05);

			MirabufCachingService.cacheRemote(info.src, type)
				.then((cacheInfo) => {
					if (cacheInfo) {
						status.done();
					} else {
						status.fail("Failed to cache");
					}
				})
				.catch(() => status.fail());
		},
		[],
	);

	const selectAPS = useCallback(
		(data: Data, type: MiraType) => {
			const status = new ProgressHandle(data.attributes.displayName ?? data.id);
			status.update("Downloading from APS...", 0.05);

			MirabufCachingService.cacheAPS(data, type)
				.then((cacheInfo) => {
					if (cacheInfo) {
						spawnCachedMira(cacheInfo, type, status);
					} else {
						status.fail("Failed to cache");
					}
				})
				.catch(() => status.fail());

			if (panel) closePanel(panel.id, CloseType.Cancel);
		},
		[closePanel, panel],
	);

	// Generate Item cards for cached robots.
	const cachedRobotElements = useMemo(
		() =>
			cachedRobots
				.sort((a, b) => a.name?.localeCompare(b.name ?? "") ?? -1)
				.map((info) =>
					ItemCard({
						name: info.name || info.cacheKey || "Unnamed Robot",
						id: info.id,
						primaryButtonNode: SynthesisIcons.ADD_LARGE,
						primaryOnClick: () => {
							console.log(`Selecting cached robot: ${info.cacheKey}`);
							selectCache(info, MiraType.ROBOT);
						},
						secondaryOnClick: () => {
							console.log(`Deleting cache of: ${info.cacheKey}`);
							MirabufCachingService.remove(
								info.cacheKey,
								info.id,
								MiraType.ROBOT,
							);

							setCachedRobots(getCacheInfo(MiraType.ROBOT));
						},
					}),
				),
		[cachedRobots, selectCache, setCachedRobots],
	);

	// Generate Item cards for cached fields.
	const cachedFieldElements = useMemo(
		() =>
			cachedFields
				.sort((a, b) => a.name?.localeCompare(b.name ?? "") ?? -1)
				.map((info) =>
					ItemCard({
						name: info.name || info.cacheKey || "Unnamed Field",
						id: info.id,
						primaryButtonNode: SynthesisIcons.ADD_LARGE,
						primaryOnClick: () => {
							console.log(`Selecting cached field: ${info.cacheKey}`);
							selectCache(info, MiraType.FIELD);
						},
						secondaryOnClick: () => {
							console.log(`Deleting cache of: ${info.cacheKey}`);
							MirabufCachingService.remove(
								info.cacheKey,
								info.id,
								MiraType.FIELD,
							);

							setCachedFields(getCacheInfo(MiraType.FIELD));
						},
					}),
				),
		[cachedFields, selectCache, setCachedFields],
	);

	// Generate Item cards for remote robots.
	const remoteRobotElements = useMemo(() => {
		const remoteRobots = manifest?.robots.filter(
			(path) => !cachedRobots.some((info) => info.cacheKey.includes(path.src)),
		);
		return remoteRobots
			?.sort((a, b) => a.displayName.localeCompare(b.displayName))
			.map((path) =>
				ItemCard({
					name: path.displayName,
					id: path.src,
					primaryButtonNode: SynthesisIcons.DOWNLOAD_LARGE,
					primaryOnClick: () => {
						console.log(`Selecting remote: ${path}`);
						selectRemote(path, MiraType.ROBOT);
					},
				}),
			);
	}, [manifest?.robots, cachedRobots, selectRemote]);

	// Generate Item cards for remote fields.
	const remoteFieldElements = useMemo(() => {
		const remoteFields = manifest?.fields.filter(
			(path) => !cachedFields.some((info) => info.cacheKey.includes(path.src)),
		);
		return remoteFields
			?.sort((a, b) => a.displayName.localeCompare(b.displayName))
			.map((path) =>
				ItemCard({
					name: path.displayName,
					id: path.src,
					primaryButtonNode: SynthesisIcons.DOWNLOAD_LARGE,
					primaryOnClick: () => {
						console.log(`Selecting remote: ${path}`);
						selectRemote(path, MiraType.FIELD);
					},
				}),
			);
	}, [manifest?.fields, cachedFields, selectRemote]);

	function downloadAllRemote(cached: MirabufCacheInfo[]): () => void {
		// eslint-disable-next-line react-hooks/rules-of-hooks
		return useCallback(() => {
			const miraType: MiraType | undefined = cached[0]?.miraType;
			const property = miraType === MiraType.ROBOT ? "robots" : "fields";
			const remotes = manifest ? manifest[property] : [];

			remotes
				.filter(
					(path) => !cached.some((info) => info.cacheKey.includes(path.src)),
				)
				.forEach((path) => cacheRemoteOnly(path, miraType));

			if (panel) closePanel(panel.id, CloseType.Cancel);
			// eslint-disable-next-line react-hooks/exhaustive-deps
		}, [manifest, cached, cacheRemoteOnly, closePanel, panel]);
	}

	const downloadAllRemoteRobots = downloadAllRemote(cachedRobots);
	const downloadAllRemoteFields = downloadAllRemote(cachedFields);

	// Generate Item cards for APS robots and fields.
	const hubElements = useMemo(
		() =>
			files
				?.sort((a, b) =>
					a.attributes.displayName!.localeCompare(b.attributes.displayName!),
				)
				.map((file) =>
					ItemCard({
						name: `${file.attributes.displayName!.replace(".mira", "")}${file.attributes.versionNumber !== undefined ? ` (v${file.attributes.versionNumber})` : ""}`,
						id: file.id,
						primaryButtonNode: SynthesisIcons.DOWNLOAD_LARGE,
						primaryOnClick: () => {
							console.debug(file.raw);
							selectAPS(file, viewType);
						},
					}),
				),
		[files, selectAPS, viewType],
	);
	useEffect(() => {
		setViewType(
			configurationType === "ROBOTS" ? MiraType.ROBOT : MiraType.FIELD,
		);
		setConfigurationType("ROBOTS");
	}, []);
	return (
		<Stack direction="column" gap={2} className="overflow-y-auto">
			<ToggleButtonGroup
				value={viewType}
				exclusive
				onChange={(_, v) => {
					if (v != null) {
						setViewType(v);
					}
				}}
				{...SoundPlayer.buttonSoundEffects()}
				sx={{
					alignSelf: "center",
				}}
			>
				<ToggleButton value={MiraType.ROBOT}>Robots</ToggleButton>
				<ToggleButton value={MiraType.FIELD}>Fields</ToggleButton>
			</ToggleButtonGroup>
			{viewType === MiraType.ROBOT ? (
				<>
					<Typography
						variant="h4"
						className="text-center mt-[4pt] mb-[2pt] mx-[5%]"
					>
						{cachedRobotElements
							? `${cachedRobotElements.length} Saved Robot${cachedRobotElements.length === 1 ? "" : "s"}`
							: "Loading Saved Robots"}
					</Typography>
					<Divider />
					{cachedRobotElements}
				</>
			) : (
				<>
					<Typography
						variant="h4"
						className="text-center mt-[4pt] mb-[2pt] mx-[5%]"
					>
						{cachedFieldElements
							? `${cachedFieldElements.length} Saved Field${cachedFieldElements.length == 1 ? "" : "s"}`
							: "Loading Saved Fields"}
					</Typography>
					<Divider />
					{cachedFieldElements}
				</>
			)}
			<Stack
				direction="row"
				key={`remote-label-container`}
				gap={"0.25rem"}
				justifyContent={"center"}
				alignItems={"center"}
			>
				<Typography
					variant="h4"
					className="text-center mt-[4pt] mb-[2pt] mx-[5%]"
				>
					{hubElements
						? `${hubElements.length} Remote Asset${hubElements.length === 1 ? "" : "s"}`
						: filesStatus.message}
				</Typography>
				{hubElements &&
					filesStatus.isDone &&
					RefreshButton(() => requestMirabufFiles())}
			</Stack>
			<Divider />
			{hubElements}
			{viewType === MiraType.ROBOT ? (
				<>
					<Typography
						variant="h4"
						className="text-center mt-[4pt] mb-[2pt] mx-[5%]"
					>
						{remoteRobotElements
							? `${remoteRobotElements.length} Default Robot${remoteRobotElements.length === 1 ? "" : "s"}`
							: "Loading Default Robots"}
					</Typography>
					<Divider />
					{remoteRobotElements}
					<Stack justifyContent="center" mt={1}>
						<PositiveButton onClick={downloadAllRemoteRobots}>
							Download All
						</PositiveButton>
					</Stack>
				</>
			) : (
				<>
					<Typography
						variant="h4"
						className="text-center mt-[4pt] mb-[2pt] mx-[5%]"
					>
						{remoteFieldElements
							? `${remoteFieldElements.length} Default Field${remoteFieldElements.length === 1 ? "" : "s"}`
							: "Loading Default Fields"}
					</Typography>
					<Divider />
					{remoteFieldElements}
					<Stack justifyContent="center" mt={1}>
						<PositiveButton onClick={downloadAllRemoteFields}>
							Download All
						</PositiveButton>
					</Stack>
				</>
			)}
			<Box alignSelf={"center"}>
				{/* TODO: modals */}
				<Button onClick={() => /*openModal("import-local-mirabuf")*/ undefined}>
					Import from File
				</Button>
			</Box>
		</Stack>
	);
};

export default ImportMirabufPanel;
