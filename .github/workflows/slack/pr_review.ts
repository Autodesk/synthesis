const webhookUrl = process.env.SLACK_WEBHOOK_URL;
const github = JSON.parse(process.env.GITHUB_DATA ?? "{}")

if (!webhookUrl) {
    console.error("SLACK_WEBHOOK_URL is required");
    process.exit(1);
}

function capitalize(str: string) {
    return str.charAt(0).toUpperCase() + str.slice(1);
}

function titleCase(str: string) {
    return str
        .split("_")
        .filter(Boolean)
        .map(capitalize)
        .join(" ");
}

// Leaving a review with multiple comments causes the action to run for each comment;
// once with action "submitted", and the rest with "edited". Ignore edited and manually
// search for comments.
if (github.event.action === "edited") process.exit(0)

// States: changes_requested, approved, commented, dismissed, pending

let image_url = "https://github.com/synthesis-adsk/github-icons/blob/main/icons/comment-gray.png?raw=true";

switch (github.event.review.state) {
    case "approved":
        image_url = "https://github.com/synthesis-adsk/github-icons/blob/main/icons/check-green.png?raw=true"
        break
    case "changes_requested":
        image_url = "https://github.com/synthesis-adsk/github-icons/blob/main/icons/file-diff-red.png?raw=true"
        break
    case "dismissed":
        image_url = "https://github.com/synthesis-adsk/github-icons/blob/main/icons/x-gray.png?raw=true"
        break
    case "commented":
    default:
        image_url = "https://github.com/synthesis-adsk/github-icons/blob/main/icons/comment-gray.png?raw=true"
        break
}

const actor = github.actor
const actor_id = github.actor_id
const pr_num = github.event.pull_request.number
const pr_author = github.event.pull_request.user.login
const review_state = github.event.review.state
const review_body = github.event.review.body
const review_url = github.event.review.html_url
const review_author = github.event.review.user.login

const getPayload = (comments: any[]) => {
    const title = `#${pr_num} (${pr_author}) - ${review_state === "dismissed" ? `dismissed review from ${review_author}` : titleCase(review_state)}`
    let blocks: any[] = [
        {
            "type": "container",
            "width": "full",
            "title": {
                "type": "plain_text",
                "text": title
            },
            "subtitle": {
                "type": "plain_text",
                "text": `${titleCase(review_state)} by ${actor}`
            },
            "icon": {
                "type": "image",
                "image_url": image_url,
                "alt_text": `Review ${titleCase(review_state)}`
            },
            "child_blocks": [
                {
                    "type": "section",
                    "text": {
                        "type": "mrkdwn",
                        "text": review_body ?? "_No body provided_",
                    },
                    "accessory": {
                        "type": "button",
                        "text": {
                            "type": "plain_text",
                            "text": "Visit",
                            "emoji": true
                        },
                        "url": review_url
                    }
                }
            ]
        }
    ]

    const ctxt = {
        "type": "context",
        "elements": [
            {
                "type": "image",
                "image_url": `https://avatars.githubusercontent.com/u/${actor_id}`,
                "alt_text": "images"
            },
            {
                "type": "mrkdwn",
                "text": `*${actor}*`
            }
        ]
    }

    if (comments.length > 0) {
        blocks.push(
            {
                "type": "container",
                "title": {
                    "type": "plain_text",
                    "text": `Comments (${comments.length})`
                },
                "width": "full",
                "is_collapsible": true,
                "default_collapsed": true,
                "child_blocks": comments.flatMap(comment => ([{
                    "type": "callout",
                    "background_color": "gray",
                    "child_blocks": [
                        {
                            "type": "section",
                            "text": {
                                "type": "mrkdwn",
                                "text": `*File: _${comment.path}_*\n\n${comment.body}`
                            },
                            "accessory": {
                                "type": "button",
                                "text": {
                                    "type": "plain_text",
                                    "text": "View",
                                    "emoji": true
                                },
                                "url": comment.html_url
                            }
                        }
                    ]
                }, ctxt]))
            })
    }
    blocks.push(ctxt)

    return {
        blocks
    }
}

const send = async () => {
    let comments;

    if (github.event.action !== "dismissed") {
        // get comments
        const repo_owner = github.repository_owner
        const repo_name = github.event.repository.name
        const pr_number = github.event.pull_request.number
        const review_id = github.event.review.id

        const url = `https://api.github.com/repos/${repo_owner}/${repo_name}/pulls/${pr_number}/reviews/${review_id}/comments?per_page=100`
        const comments_res = await fetch(
            url,
            {
                headers: {
                    Authorization: `Bearer ${github.token}`,
                    Accept: "application/vnd.github+json",
                    "X-GitHub-Api-Version": "2026-03-10"
                }
            }
        )

        comments = await comments_res.json()
    }

    const payload = getPayload(comments ?? [])

    const slack_res = await fetch(webhookUrl, {
        method: 'POST',
        body: JSON.stringify(payload),
        headers: {
            'Content-Type': 'application/json'
        }
    })

    console.log("Slack notification sent:", await slack_res.text())
}

send()
