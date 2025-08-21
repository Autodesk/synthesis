import { globalAddToast } from "@/ui/components/GlobalUIControls"
import type { AchievementDefinition, AchievementKey, AchievementState, AchievementStats, AchievementWithState } from "./AchievementsTypes"
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
	private _stats: Map<AchievementKey, AchievementStats> = new Map()

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

	public stats(): Map<AchievementKey, AchievementStats> {
		return this._stats
	}

	public isUnlocked(key: AchievementKey): boolean {
		return this._states.has(key)
	}

	public unlock(key: AchievementKey) {
		try {
			if (this._states.has(key)) return
			const def = this._definitions.get(key)
			const now = Date.now()
			this._states.set(key, { key, unlockedAt: now })
			this.saveLocal()
			this.dispatchUpdate()

			if (def) {
				globalAddToast("success", this.makeToast(def))
			}
		} catch (e) {
			console.warn("Failed to unlock achievement", key, e)
		}
	}

	private makeToast(def: AchievementDefinition) {
		return React.createElement(
			"div",
			{ className: "flex flex-row items-center gap-2" },
			React.createElement("img", {
				src: def.imageSrc ?? "/synthesis-logo.svg",
				alt: "Achievement",
				style: { width: "24px", height: "24px", borderRadius: "4px" },
			}),
			React.createElement("div", null, `Achievement Unlocked: ${def.title}`)
		)
	}

	private saveLocal() {
		const data: SavedShape = { states: [...this._states.values()] }
		try {
			window.localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(data))
		} catch (_) {
			// ignore quota errors
		}
	}

	private loadLocal() {
		try {
			const raw = window.localStorage.getItem(LOCAL_STORAGE_KEY)
			if (!raw) return
			const data = JSON.parse(raw) as SavedShape
			for (const s of data.states ?? []) this._states.set(s.key, s)
			this.dispatchUpdate()
		} catch (_) {
			// ignore
		}
	}



	private dispatchUpdate() {
		try {
			window.dispatchEvent(new Event(ACHIEVEMENTS_UPDATED_EVENT))
		} catch (_) {
			// ignore
		}
	}

	public destroy(): void {}

	public update(_: number): void {}
}


