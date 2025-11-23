export enum HelpOptionType {
    YouTube = "YouTube",
    Codelab = "Codelab",
    Website = "Website",
    Discord = "Discord"
}

export type HelpOption = {
    text: string
    link: string
    type: HelpOptionType
}

export const HELP_OPTION_SPAWN_ASSET_YOUTUBE: HelpOption = {
    text: "How to Spawn a Robot/Field",
    link: "https://youtu.be/u608dgHAc2s?si=25tbYWZpHNxGdVzU",
    type: HelpOptionType.YouTube
}

export const HELP_OPTION_CONFIGURE_YOUTUBE: HelpOption = {
    text: "How to Configure a Robot/Field",
    link: "https://youtu.be/hHf-7Ojl-fE?si=6xpjRkiFwjrosDyd",
    type: HelpOptionType.YouTube
}

export const HELP_OPTION_DISCORD: HelpOption = {
    text: "Synthesis Community",
    link: "https://discord.gg/AnGhEyAn",
    type: HelpOptionType.Discord
}

export const HELP_OPTION_EXPORT_CODELAB: HelpOption = {
    text: "Exporter Guide",
    link: "https://synthesis.autodesk.com/codelab/FusionExporterCodelab/#0",
    type: HelpOptionType.Codelab
}

export const HELP_OPTION_ALL_TUTORIALS: HelpOption = {
    text: "All Tutorials",
    link: "https://synthesis.autodesk.com/tutorials.html",
    type: HelpOptionType.Website
}
