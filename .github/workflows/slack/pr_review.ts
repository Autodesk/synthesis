import {
    commentNotification,
    commentSummary,
    issueCommentEventFromContext,
    type Message,
    MessageBuilder,
    reviewCommentEventFromContext,
    reviewCommentsFromApi,
    reviewEventFromContext,
    reviewNotification,
    reviewSummary,
} from "./index"

const webhookUrl = process.env.SLACK_WEBHOOK_URL
const github = JSON.parse(process.env.GITHUB_DATA ?? "{}")

if (!webhookUrl) {
    console.error("SLACK_WEBHOOK_URL is required")
    process.exit(1)
}

// Leaving a review with multiple comments causes the action to run for each comment;
// once with action "submitted", and the rest with "edited". Ignore edited and manually
// search for comments.
if (github.event.action === "edited") process.exit(0)

// States: changes_requested, approved, commented, dismissed, pending

const post = async (message: Message) => {
    const slack_res = await fetch(webhookUrl, {
        method: "POST",
        body: JSON.stringify(message),
        headers: {
            "Content-Type": "application/json",
        },
    })

    console.log("Slack notification sent:", await slack_res.text())
}

const ghGet = (url: string) =>
    fetch(url, {
        headers: {
            Authorization: `Bearer ${github.token}`,
            Accept: "application/vnd.github+json",
            "X-GitHub-Api-Version": "2026-03-10",
        },
    })

const send = async () => {
    if (github.event_name === "issue_comment") {
        if (!github.event.issue.pull_request) process.exit(0)
        const event = issueCommentEventFromContext(github)
        return post(
            new MessageBuilder(commentSummary(event))
                .add(...commentNotification(event))
                .build(),
        )
    }

    const repo_owner = github.repository_owner
    const repo_name = github.event.repository.name
    const pr_number = github.event.pull_request.number
    const pr_url = `https://api.github.com/repos/${repo_owner}/${repo_name}/pulls/${pr_number}`

    if (github.event_name === "pull_request_review_comment") {
        const event = reviewCommentEventFromContext(github)
        const review_id = github.event.comment.pull_request_review_id
        const reply_to_id = github.event.comment.in_reply_to_id

        // real review renders this comment in its collapsible
        if (review_id) {
            const res = await ghGet(`${pr_url}/reviews/${review_id}`)

            if (res.ok) {
                const review: any = await res.json()
                if (review.body || review.state?.toLowerCase() !== "commented")
                    process.exit(0)
            }
        }

        if (review_id || reply_to_id) {
            const res = await ghGet(`${pr_url}/comments?per_page=100`)

            if (res.ok) {
                const all: any[] = await res.json()
                const siblings = all.filter(
                    (c) => c.pull_request_review_id === review_id,
                )
                // wrapper holds one comment; more = batched review
                if (siblings.length > 1) process.exit(0)
                // in_reply_to_id = thread root; want newest before mine
                const parent = all
                    .filter(
                        (c) =>
                            (c.id === reply_to_id ||
                                c.in_reply_to_id === reply_to_id) &&
                            c.id !== github.event.comment.id,
                    )
                    .sort((a, b) => a.id - b.id)
                    .at(-1)
                if (reply_to_id && parent?.user && parent.body != null) {
                    event.replyTo = { author: parent.user.login, body: parent.body }
                }
            }
        }

        return post(
            new MessageBuilder(commentSummary(event))
                .add(...commentNotification(event))
                .build(),
        )
    }

    let comments = null

    if (github.event.action !== "dismissed") {
        const review_id = github.event.review.id

        const comments_res = await ghGet(
            `${pr_url}/reviews/${review_id}/comments?per_page=100`,
        )

        comments = await comments_res.json()
    }

    // lone bodyless comment = wrapper, already sent; batches kept
    if (
        github.event.review.state === "commented" &&
        !github.event.review.body &&
        Array.isArray(comments) &&
        comments.length === 1
    )
        process.exit(0)

    const event = reviewEventFromContext(github)

    // dismiss message isn't in the payload
    if (github.event.action === "dismissed") {
        const url = `https://api.github.com/repos/${repo_owner}/${repo_name}/issues/${pr_number}/timeline?per_page=100`
        const res = await ghGet(url)

        if (res.ok) {
            const events: any[] = await res.json()
            const dismissal = events
                .filter(
                    (e) =>
                        e.event === "review_dismissed" &&
                        e.dismissed_review?.review_id === github.event.review.id,
                )
                .at(-1)
            if (dismissal?.dismissed_review?.dismissal_message)
                event.review.body = dismissal.dismissed_review.dismissal_message
        }
    }

    return post(
        new MessageBuilder(reviewSummary(event))
            .add(...reviewNotification(event, reviewCommentsFromApi(comments ?? [])))
            .build(),
    )
}

send()
