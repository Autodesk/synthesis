import { globalAddToast } from "@/ui/components/GlobalUIControls"
import type { AchievementDefinition, AchievementKey, AchievementState, AchievementWithState } from "./AchievementsTypes"
import WorldSystem from "../WorldSystem"
import React from "react"

export const ACHIEVEMENTS_UPDATED_EVENT = "AchievementsUpdatedEvent"

const LOCAL_STORAGE_KEY = "synthesis_achievements_v1"

type SavedShape = {
	states: AchievementState[]
}

export default class AchievementsSystem extends WorldSystem {
	private _definitions: Map<AchievementKey, AchievementDefinition> = new Map()
	private _states: Map<AchievementKey, AchievementState> = new Map()

	public constructor() {
		super()
		this.registerDefaultDefinitions()
		this.loadLocal()
	}

	public register(defs: AchievementDefinition[]) {
		for (const d of defs) this._definitions.set(d.key, d)
	}

	private registerDefaultDefinitions() {
		this.register([
			{
				key: "first_login_aps",
				title: "Connected",
				description: "Sign in with Autodesk APS",
				imageSrc: "/synthesis-logo.svg",
			},
			{
				key: "first_robot_spawn",
				title: "Engineer in the Making",
				description: "Spawn your first robot",
				imageSrc: "/synthesis-logo.svg",
			},
			{
				key: "first_field_spawn",
				title: "Welcome to the Field",
				description: "Spawn your first field",
				imageSrc: "/synthesis-logo.svg",
			},
			{
				key: "mystery_hidden",
				title: "Hidden",
				description: "???",
				imageSrc: "/synthesis-logo.svg",
				hidden: true,
			},
		])
	}

	public list(): AchievementWithState[] {
		return [...this._definitions.values()].map(def => ({ ...def, state: this._states.get(def.key) }))
	}

	public unlock(key: AchievementKey) {
		if (!this._definitions.has(key)) {
			console.warn("Attempted to unlock unknown achievement", key)
			return
		}
		try {
			if (this._states.has(key)) return
			const def = this._definitions.get(key)
			const now = Date.now()
			this._states.set(key, { key, unlockedAt: now })
			this.saveLocal()
			this.dispatchUpdate()

			if (def) {
				globalAddToast("default", this.makeToast(def))
			}
		} catch (e) {
			console.warn("Failed to unlock achievement", key, e)
		}
	}

	private makeToast(def: AchievementDefinition) {
		return React.createElement(
			"div",
			{
				"data-achievement-toast": true,
				className: "flex flex-row items-center gap-3 px-2 py-1",
				style: {
					minWidth: 340,
					maxWidth: 520,
					minHeight: 84,
					userSelect: "none",
					MozUserSelect: "none",
					msUserSelect: "none",
					WebkitUserSelect: "none",
				},
			},
			React.createElement("div", {
				className: "w-1 self-stretch rounded-md bg-emerald-500",
				style: { marginRight: 4 },
			}),
			React.createElement("img", {
				src: def.imageSrc ?? "/synthesis-logo.svg",
				alt: "Achievement",
				style: { width: 56, height: 56, borderRadius: 8, objectFit: "cover" },
				draggable: false,
			}),
			React.createElement(
				"div",
				{ className: "flex flex-col" },
				React.createElement(
					"div",
					{ className: "text-emerald-400 text-[12px] uppercase tracking-wider font-semibold drop-shadow" },
					"Achievement Unlocked"
				),
				React.createElement(
					"div",
					{ className: "text-white text-[15px] font-semibold drop-shadow" },
					def.title
				),
				def.description
					? React.createElement(
						"div",
						{ className: "text-zinc-200 text-[12px] drop-shadow" },
						def.description
					  )
					: null
			)
		)
	}

	private saveLocal() {
		const data: SavedShape = { states: [...this._states.values()] }
		try {
			window.localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(data))
		} catch (e) {
			console.warn("Failed to save achievements", e)
		}
	}

	private loadLocal() {
		try {
			const raw = window.localStorage.getItem(LOCAL_STORAGE_KEY)
			if (!raw) return
			const data = JSON.parse(raw) as SavedShape
			for (const s of data.states ?? []) this._states.set(s.key, s)
			this.dispatchUpdate()
		} catch (e) {
			console.warn("Failed to load achievements", e)
		}
	}

	private dispatchUpdate() {
		try {
			window.dispatchEvent(new Event(ACHIEVEMENTS_UPDATED_EVENT))
		} catch (e) {
			console.warn("Failed to dispatch achievements update", e)
		}
	}

	public destroy(): void {}

	public update(_: number): void {}
}
