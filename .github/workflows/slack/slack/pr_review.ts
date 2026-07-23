import { MessageBuilder, Message, commentNotification, commentSummary, issueCommentEventFromContext, reviewCommentEventFromContext, reviewCommentsFromApi, reviewEventFromContext, reviewNotification, reviewSummary } from "./index";

const webhookUrl = process.env.SLACK_WEBHOOK_URL;
const github = JSON.parse(process.env.GITHUB_DATA ?? "{}")

if (!webhookUrl) {
    console.error("SLACK_WEBHOOK_URL is required");
    process.exit(1);
}

// Leaving a review with multiple comments causes the action to run for each comment;
// once with action "submitted", and the rest with "edited". Ignore edited and manually
// search for comments.
if (github.event.action === "edited") process.exit(0)

// States: changes_requested, approved, commented, dismissed, pending

const post = async (message: Message) => {
    const slack_res = await fetch(webhookUrl, {
        method: 'POST',
        body: JSON.stringify(message),
        headers: {
            'Content-Type': 'application/json'
        }
    })

    console.log("Slack notification sent:", await slack_res.text())
}

const ghGet = (url: string) => fetch(url, {
    headers: {
        Authorization: `Bearer ${github.token}`,
        Accept: "application/vnd.github+json",
        "X-GitHub-Api-Version": "2026-03-10"
    }
})

const send = async () => {
    if (github.event_name === "issue_comment") {
        if (!github.event.issue.pull_request) process.exit(0)
        const event = issueCommentEventFromContext(github)
        return post(new MessageBuilder(commentSummary(event)).add(...commentNotification(event)).build())
    }

    if (github.event_name === "pull_request_review_comment") {
        const event = reviewCommentEventFromContext(github)

        const reply_to_id = github.event.comment.in_reply_to_id
        if (reply_to_id) {
            const repo_owner = github.repository_owner
            const repo_name = github.event.repository.name
            const pr_number = github.event.pull_request.number

            const url = `https://api.github.com/repos/${repo_owner}/${repo_name}/pulls/${pr_number}/comments?per_page=100`
            const res = await ghGet(url)

            if (res.ok) {
                const all: any[] = await res.json()
                // in_reply_to_id = thread root; want newest reply before mine
                const parent = all
                    .filter(c => (c.id === reply_to_id || c.in_reply_to_id === reply_to_id) && c.id !== github.event.comment.id)
                    .sort((a, b) => a.id - b.id)
                    .at(-1)
                if (parent?.user && parent.body != null) {
                    event.replyTo = { author: parent.user.login, body: parent.body }
                }
            }
        }

        return post(new MessageBuilder(commentSummary(event)).add(...commentNotification(event)).build())
    }

    let comments;

    if (github.event.action !== "dismissed") {
        const repo_owner = github.repository_owner
        const repo_name = github.event.repository.name
        const pr_number = github.event.pull_request.number
        const review_id = github.event.review.id

        const url = `https://api.github.com/repos/${repo_owner}/${repo_name}/pulls/${pr_number}/reviews/${review_id}/comments?per_page=100`
        const comments_res = await ghGet(url)

        comments = await comments_res.json()
    }

    // lone bodyless comment = single/reply wrapper, already sent; batches (>1) kept
    if (github.event.review.state === "commented" && !github.event.review.body && Array.isArray(comments) && comments.length === 1) process.exit(0)

    const event = reviewEventFromContext(github)
    return post(new MessageBuilder(reviewSummary(event))
        .add(...reviewNotification(event, reviewCommentsFromApi(comments ?? [])))
        .build())
}

send()
