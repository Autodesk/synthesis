import {
    Block,
    button,
    container,
    context,
    image,
    ImageElement,
    mrkdwn,
    section,
    titleCase,
} from "./block-kit"

export interface Actor {
    login: string
    id: number
}

export interface PullRequest {
    number: number
    title: string
    body: string | null
    author: string
    htmlUrl: string
    draft: boolean
}

export interface ReviewInfo {
    state: string
    body: string | null
    htmlUrl: string
    author: string
}

export interface ReviewComment {
    path: string
    body: string
    htmlUrl: string
    diffHunk?: string
}

export interface PrEvent {
    action: string
    actor: Actor
    pr: PullRequest
}

export interface ReviewEvent {
    actor: Actor
    pr: PullRequest
    review: ReviewInfo
}

export interface CommentTarget {
    number: number
    title: string
    author: string
}

export interface CommentEvent {
    actor: Actor
    target: CommentTarget
    comment: { body: string; htmlUrl: string; path?: string; diffHunk?: string }
    replyTo?: { author: string; body: string }
}

/**
 * Maps the Actions context to a PrEvent
 *
 * @return PrEvent
 */
export const prEventFromContext = (github: any): PrEvent => ({
    action: github.event.action === "closed" && github.event.pull_request.merged ? "merged" : github.event.action,
    actor: { login: github.actor, id: github.actor_id },
    pr: {
        number: github.event.number,
        title: github.event.pull_request.title,
        body: github.event.pull_request.body,
        author: github.event.pull_request.user.login,
        htmlUrl: github.event.pull_request.html_url,
        draft: github.event.pull_request.draft,
    },
})

/**
 * Maps the Actions context to a ReviewEvent
 *
 * @return ReviewEvent
 */
export const reviewEventFromContext = (github: any): ReviewEvent => ({
    actor: { login: github.actor, id: github.actor_id },
    pr: {
        number: github.event.pull_request.number,
        title: github.event.pull_request.title,
        body: github.event.pull_request.body,
        author: github.event.pull_request.user.login,
        htmlUrl: github.event.pull_request.html_url,
        draft: github.event.pull_request.draft,
    },
    review: {
        state: github.event.review.state,
        body: github.event.review.body,
        htmlUrl: github.event.review.html_url,
        author: github.event.review.user.login,
    },
})

/**
 * Maps raw review-comment API objects to ReviewComment
 *
 * @return ReviewComment[]
 */
export const reviewCommentsFromApi = (comments: any[]): ReviewComment[] =>
    comments.map(c => ({ path: c.path, body: c.body, htmlUrl: c.html_url ?? "", diffHunk: c.diff_hunk }))

/**
 * Maps an issue_comment context (PR conversation comment) to a CommentEvent
 *
 * @return CommentEvent
 */
export const issueCommentEventFromContext = (github: any): CommentEvent => ({
    actor: { login: github.actor, id: github.actor_id },
    target: {
        number: github.event.issue.number,
        title: github.event.issue.title,
        author: github.event.issue.user.login,
    },
    comment: { body: github.event.comment.body, htmlUrl: github.event.comment.html_url },
})

/**
 * Maps a pull_request_review_comment context (standalone inline comment) to a CommentEvent
 *
 * @return CommentEvent
 */
export const reviewCommentEventFromContext = (github: any): CommentEvent => ({
    actor: { login: github.actor, id: github.actor_id },
    target: {
        number: github.event.pull_request.number,
        title: github.event.pull_request.title,
        author: github.event.pull_request.user.login,
    },
    comment: {
        body: github.event.comment.body,
        htmlUrl: github.event.comment.html_url,
        path: github.event.comment.path,
        diffHunk: github.event.comment.diff_hunk,
    },
})

const ICON_BASE = "https://github.com/synthesis-adsk/github-icons/blob/main/icons"

/**
 * Builds an icon asset URL from its name
 *
 * @return string
 */
const icon = (name: string): string => `${ICON_BASE}/${name}.png?raw=true`

const PR_ICONS: Record<string, string> = {
    merged: icon("pull-request-merged-purple"),
    closed: icon("pull-request-closed-red"),
}

const REVIEW_ICONS: Record<string, string> = {
    approved: icon("check-green"),
    changes_requested: icon("file-diff-red"),
    dismissed: icon("x-gray"),
    commented: icon("comment-gray"),
}

/**
 * Selects the PR icon URL for an event's action and draft state
 *
 * @return string
 */
const prIconUrl = (e: PrEvent): string => {
    switch (e.action) {
        case "opened":
        case "reopened":
            return icon(e.pr.draft ? "pull-request-draft-gray" : "pull-request-green")
        default:
            return PR_ICONS[e.action] ?? ""
    }
}

/**
 * Selects the review icon URL for a state, defaulting to commented
 *
 * @return string
 */
const reviewIconUrl = (state: string): string => REVIEW_ICONS[state] ?? REVIEW_ICONS.commented

/**
 * Builds the review container title line
 *
 * @return string
 */
const reviewTitle = (e: ReviewEvent): string =>
    `${e.review.state === "dismissed" ? `Dismissed review from ${e.review.author}` : titleCase(e.review.state)} - #${e.pr.number} ${e.pr.title} (${e.pr.author})`

/**
 * Builds the message text fallback for a PR event
 *
 * @return string
 */
export const prSummary = (e: PrEvent): string => `${e.actor.login}: PR #${e.pr.number} ${e.action}`

/**
 * Builds the message text fallback for a review event
 *
 * @return string
 */
export const reviewSummary = (e: ReviewEvent): string => `${e.actor.login}: ${reviewTitle(e)}`

/**
 * Builds the comment container title line
 *
 * @return string
 */
const commentTitle = (e: CommentEvent): string =>
    `Comment - #${e.target.number} ${e.target.title} (${e.target.author})`

/**
 * Builds the message text fallback for a comment event
 *
 * @return string
 */
export const commentSummary = (e: CommentEvent): string => `${e.actor.login}: ${commentTitle(e)}`

interface ParsedBody {
    text: string
    images: ImageElement[]
}

const IMG_MD = /!\[([^\]]*)\]\(\s*<?([^)>\s]+)>?(?:\s+["'][^"']*["'])?\s*\)/g
const IMG_TAG = /<img\b[^>]*?\bsrc=["']([^"']+)["'][^>]*>/gi
const IMG_URL = /https?:\/\/[^\s)]+\.(?:png|jpe?g|gif|webp)(?:\?[^\s)]*)?/gi
const IMG_ALT = /\balt=["']([^"']*)["']/i

/**
 * Splits a body into text and image blocks, extracting markdown images,
 * <img> tags, then bare image URLs
 *
 * @return ParsedBody
 */
const parseImages = (body: string): ParsedBody => {
    const images: ImageElement[] = []
    const text = body
        .replace(IMG_MD, (_m, alt, url) => (images.push(image(url, alt || "image")), ""))
        .replace(IMG_TAG, (m, url) => (images.push(image(url, m.match(IMG_ALT)?.[1] || "image")), ""))
        .replace(IMG_URL, url => (images.push(image(url, "image")), ""))
        .trim()
    return { text, images }
}

const SUGGESTION = /```suggestion\r?\n(.*?)```/gs

/**
 * Relabels GitHub ```suggestion blocks as a plain labelled code block
 *
 * @return string
 */
const formatSuggestions = (text: string): string =>
    text.replace(SUGGESTION, (_m, code) => "*Suggested change:*\n```\n" + code + "```")

/**
 * Renders the Jira section, scanning title then body for a SYNTH ticket
 *
 * @return Block
 */
const jiraSection = (pr: PullRequest): Block => {
    const regexp = /SYNTH-\d+/g
    const matches = pr.title.match(regexp) ?? pr.body?.match(regexp) ?? []
    return section(
        mrkdwn(matches.length >= 1 ? "Jira: `" + matches[0] + "`" : "No Jira ticket attached"),
        button("Visit", pr.htmlUrl),
    )
}

/**
 * Renders the PR body as a text section plus any inline images
 *
 * @return Block[]
 */
const prBodyBlocks = (body: string | null): Block[] => {
    if (!body) return []
    const { text, images } = parseImages(body)
    return [...(text ? [section(mrkdwn(text))] : []), ...images]
}

/**
 * Renders the Slack block for a PR opened/closed/merged notification
 *
 * @return Block
 */
export const prNotification = (e: PrEvent): Block =>
    container({
        width: "full",
        title: `#${e.pr.number} - ${e.pr.title}`,
        subtitle: `${e.pr.draft ? "Draft " : ""}${titleCase(e.action)} by ${e.actor.login}`,
        icon: image(prIconUrl(e), `Pull Request ${titleCase(e.action)}`),
        child_blocks: [...prBodyBlocks(e.pr.body), jiraSection(e.pr)],
    })

/**
 * Renders the actor attribution as a context block (avatar + name)
 *
 * @return Block
 */
const attribution = (actor: Actor): Block =>
    context([
        image(`https://avatars.githubusercontent.com/u/${actor.id}`, "GitHub Profile Picture"),
        mrkdwn(`*${actor.login}*`),
    ])

/**
 * Renders a single review comment followed by its attribution
 *
 * @return Block[]
 */
const commentBlock = (actor: Actor, c: ReviewComment): Block[] => {
    const { text, images } = parseImages(c.body)
    const header = `*File: _${c.path}_*`
    const hunk = c.diffHunk ? "```\n" + c.diffHunk + "\n```" : ""
    const body = formatSuggestions(text)
    return [
        section(mrkdwn([header, hunk, body].filter(Boolean).join("\n\n")), button("View", c.htmlUrl)),
        ...images,
        attribution(actor),
    ]
}

/**
 * Renders the collapsible container grouping all review comments
 *
 * @return Block
 */
const commentGroup = (actor: Actor, comments: ReviewComment[]): Block =>
    container({
        width: "full",
        title: `Comments (${comments.length})`,
        is_collapsible: true,
        default_collapsed: true,
        child_blocks: comments.flatMap(c => commentBlock(actor, c)),
    })

/**
 * Renders the Slack blocks for a review notification: container, optional
 * comment group, and trailing attribution
 *
 * @return Block[]
 */
export const reviewNotification = (e: ReviewEvent, comments: ReviewComment[]): Block[] => {
    const { text, images } = e.review.body ? parseImages(e.review.body) : { text: "", images: [] }
    return [
        container({
            width: "full",
            title: reviewTitle(e),
            subtitle: `${titleCase(e.review.state)} by ${e.actor.login}`,
            icon: image(reviewIconUrl(e.review.state), `Review ${titleCase(e.review.state)}`),
            child_blocks: [
                section(mrkdwn(text || "_No body provided_"), button("Visit", e.review.htmlUrl)),
                ...images,
            ],
        }),
        ...(comments.length > 0 ? [commentGroup(e.actor, comments)] : []),
        attribution(e.actor),
    ]
}

/**
 * Renders the Slack blocks for a PR comment notification: container +
 * attribution. Shows the file header for inline comments
 *
 * @return Block[]
 */
export const commentNotification = (e: CommentEvent): Block[] => {
    const { text, images } = parseImages(e.comment.body)
    const header = e.comment.path ? `*File: _${e.comment.path}_*` : ""
    const hunk = e.comment.diffHunk ? "```\n" + e.comment.diffHunk + "\n```" : ""
    const quote = e.replyTo ? e.replyTo.body.split("\n").map(l => "> " + l).join("\n") : ""
    const parts = [header, hunk, quote, formatSuggestions(text)].filter(Boolean)
    const bodyText = parts.length ? parts.join("\n\n") : "_No body provided_"
    const subtitle = e.replyTo ? `${e.actor.login} replied to ${e.replyTo.author}` : `Comment by ${e.actor.login}`
    return [
        container({
            width: "full",
            title: commentTitle(e),
            subtitle,
            icon: image(REVIEW_ICONS.commented, "Comment"),
            child_blocks: [
                section(mrkdwn(bodyText), button("Visit", e.comment.htmlUrl)),
                ...images,
            ],
        }),
        attribution(e.actor),
    ]
}
