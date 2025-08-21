export type AchievementKey = string

export interface AchievementDefinition {
	key: AchievementKey
	title: string
	description: string
	imageSrc?: string
	/** If true, title/image/description are hidden until unlocked */
	hidden?: boolean
}

export interface AchievementState {
	key: AchievementKey
	unlockedAt?: number
}

export interface AchievementWithState extends AchievementDefinition {
	state?: AchievementState
}

export type AchievementsSavePayload = {
	userEmail?: string
	achievements: AchievementState[]
}


