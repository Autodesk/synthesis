export interface PlainText {
    type: "plain_text"
    text: string
    emoji?: boolean
}

export interface Mrkdwn {
    type: "mrkdwn"
    text: string
}

export type TextObject = PlainText | Mrkdwn

export interface ImageElement {
    type: "image"
    image_url: string
    alt_text: string
}

export interface Button {
    type: "button"
    text: PlainText
    url: string
}

export interface SectionBlock {
    type: "section"
    text: TextObject
    accessory?: Button
}

export interface ContextBlock {
    type: "context"
    elements: (Mrkdwn | ImageElement)[]
}

export interface ContainerBlock {
    type: "container"
    width?: string
    title: PlainText
    subtitle?: PlainText
    icon?: ImageElement
    is_collapsible?: boolean
    default_collapsed?: boolean
    child_blocks: Block[]
}

export interface CalloutBlock {
    type: "callout"
    background_color: string
    child_blocks: Block[]
}

export type Block = SectionBlock | ContextBlock | ContainerBlock | CalloutBlock | ImageElement

export interface Message {
    blocks: Block[]
    text?: string
}

export const plainText = (text: string, emoji?: boolean): PlainText =>
    emoji === undefined ? { type: "plain_text", text } : { type: "plain_text", text, emoji }

/**
 * Mrkdwn uses a reduced Markdown spec, hence the name.
 */
export const mrkdwn = (text: string): Mrkdwn => ({ type: "mrkdwn", text })

export const image = (image_url: string, alt_text: string): ImageElement =>
    ({ type: "image", image_url, alt_text })

export const button = (text: string, url: string): Button =>
    ({ type: "button", text: plainText(text, true), url })

/**
 * Displays text, possibly alongside elements.
 * https://docs.slack.dev/reference/block-kit/blocks/section-block
 */
export const section = (text: TextObject, accessory?: Button): SectionBlock =>
    accessory ? { type: "section", text, accessory } : { type: "section", text }

/**
 * Provides contextual info, which can include both images and text.
 * https://docs.slack.dev/reference/block-kit/blocks/context-block
 */
export const context = (elements: (Mrkdwn | ImageElement)[]): ContextBlock =>
    ({ type: "context", elements })

export const callout = (background_color: string, child_blocks: Block[]): CalloutBlock =>
    ({ type: "callout", background_color, child_blocks })

export interface ContainerOptions {
    title: string
    subtitle?: string
    icon?: ImageElement
    width?: string
    is_collapsible?: boolean
    default_collapsed?: boolean
    child_blocks: Block[]
}

/**
 * A general-purpose wrapper for grouping child blocks together, with a configurable size.
 * https://docs.slack.dev/reference/block-kit/blocks/container-block
 */
export const container = (o: ContainerOptions): ContainerBlock => ({
    type: "container",
    width: o.width,
    title: plainText(o.title),
    subtitle: o.subtitle !== undefined ? plainText(o.subtitle) : undefined,
    icon: o.icon,
    is_collapsible: o.is_collapsible,
    default_collapsed: o.default_collapsed,
    child_blocks: o.child_blocks,
})

export class MessageBuilder {
    private blocks: Block[] = []

    constructor(private summary?: string) { }

    add(...blocks: Block[]): this {
        this.blocks.push(...blocks)
        return this
    }

    build(): Message {
        return { blocks: this.blocks, text: this.summary }
    }
}

/**
 * Uppercases the first character of a string
 *
 * @return string
 */
export function capitalize(str: string): string {
    return str.charAt(0).toUpperCase() + str.slice(1)
}

/**
 * Converts a snake_case string to Title Case
 *
 * @return string
 */
export function titleCase(str: string): string {
    return str
        .split("_")
        .filter(Boolean)
        .map(capitalize)
        .join(" ")
}
