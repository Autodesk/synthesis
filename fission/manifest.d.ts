export type ManifestFileType = Record<
    "robots" | "private" | "fields" | "components",
    { filename: string; hash: string }[]
>
