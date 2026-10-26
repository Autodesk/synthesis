export type ManifestFileEntry = { filename: string; hash: string; year?: number; thumbnail?: string }
export type ManifestFileType = Record<"robots" | "private" | "fields", ManifestFileEntry[]>
