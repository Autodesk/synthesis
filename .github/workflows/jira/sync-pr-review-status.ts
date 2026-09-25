type PullRequest = {
    body?: string | null
    head?: { ref?: string; repo?: { fork?: boolean } | null }
    html_url?: string
    number?: number
    title?: string
}

type Review = {
    id?: number
    state?: string
    submitted_at?: string | null
    user?: { login?: string }
}

type GitHubContext = {
    event?: {
        action?: string
        pull_request?: PullRequest
        requested_reviewer?: { login?: string }
        review?: { state?: string; user?: { login?: string } }
    }
    event_name?: string
    repository?: { name?: string; owner?: { login?: string } }
}

type JiraEvent = "changes_requested" | "review_requested"

type JiraUpdate = {
    event: JiraEvent
    issueKeys: string[]
    pullRequest: PullRequest
    reviewer?: string
    targetStatus: "Addressing Feedback" | "In Review"
}

const context = JSON.parse(process.env.GITHUB_DATA ?? "{}") as GitHubContext
const webhookUrl = process.env.JIRA_AUTOMATION_WEBHOOK_URL
const webhookToken = process.env.JIRA_AUTOMATION_WEBHOOK_TOKEN
const githubToken = process.env.GITHUB_TOKEN

const normalizeLogin = (login: string | undefined) => login?.toLowerCase()

const reviewTimestamp = (review: Review) => {
    const timestamp = Date.parse(review.submitted_at ?? "")
    return Number.isNaN(timestamp) ? review.id ?? 0 : timestamp
}

const latestReviewsByReviewer = (reviews: Review[]) => {
    const latest = new Map<string, Review>()

    for (const review of reviews) {
        const reviewer = normalizeLogin(review.user?.login)
        if (!reviewer || !review.state || review.state.toUpperCase() === "PENDING") continue

        const previous = latest.get(reviewer)
        if (!previous || reviewTimestamp(review) > reviewTimestamp(previous))
            latest.set(reviewer, review)
    }

    return latest
}

const issueKeysFor = (pullRequest: PullRequest) =>
    [pullRequest.title, pullRequest.body, pullRequest.head?.ref]
        .filter((value): value is string => Boolean(value))
        .flatMap((value) => value.match(/\bSYNTH-\d+\b/gi) ?? [])
        .map((key) => key.toUpperCase())
        .filter((key, index, keys) => keys.indexOf(key) === index)

const pullRequestReviews = async (pullRequest: PullRequest) => {
    const owner = context.repository?.owner?.login
    const repository = context.repository?.name
    const number = pullRequest.number

    if (!owner || !repository || !number || !githubToken)
        throw new Error("GitHub pull request context or token is unavailable")

    const reviews: Review[] = []
    for (let page = 1; ; page += 1) {
        const response = await fetch(
            `https://api.github.com/repos/${owner}/${repository}/pulls/${number}/reviews?per_page=100&page=${page}`,
            {
                headers: {
                    Accept: "application/vnd.github+json",
                    Authorization: `Bearer ${githubToken}`,
                    "X-GitHub-Api-Version": "2026-03-10",
                },
            },
        )

        if (!response.ok)
            throw new Error(
                `Unable to read pull request reviews: GitHub returned ${response.status}`,
            )

        const pageReviews = (await response.json()) as Review[]
        reviews.push(...pageReviews)
        if (pageReviews.length < 100) return reviews
    }
}

const reviewRequestUpdate = async (pullRequest: PullRequest) => {
    const requestedReviewer = normalizeLogin(context.event?.requested_reviewer?.login)
    if (!requestedReviewer) return null

    const latestReviews = latestReviewsByReviewer(await pullRequestReviews(pullRequest))
    const requestedReview = latestReviews.get(requestedReviewer)

    if (requestedReview?.state?.toUpperCase() !== "CHANGES_REQUESTED") return null

    const otherOutstandingChangeRequests = [...latestReviews.entries()].filter(
        ([reviewer, review]) =>
            reviewer !== requestedReviewer &&
            review.state?.toUpperCase() === "CHANGES_REQUESTED",
    )

    if (otherOutstandingChangeRequests.length > 0) {
        console.info(
            "Keeping Jira work items in Addressing Feedback because other reviewers still have changes requested.",
        )
        return null
    }

    return {
        event: "review_requested" as const,
        pullRequest,
        reviewer: requestedReviewer,
        targetStatus: "In Review" as const,
    }
}

const getUpdate = async () => {
    const pullRequest = context.event?.pull_request
    if (!pullRequest) return null

    if (
        context.event_name === "pull_request_review" &&
        context.event?.review?.state?.toLowerCase() === "changes_requested"
    )
        return {
            event: "changes_requested" as const,
            pullRequest,
            reviewer: context.event.review.user?.login,
            targetStatus: "Addressing Feedback" as const,
        }

    if (
        context.event_name === "pull_request" &&
        context.event.action === "review_requested"
    )
        return reviewRequestUpdate(pullRequest)

    return null
}

const sendToJira = async (update: JiraUpdate) => {
    if (!webhookUrl || !webhookToken) {
        if (update.pullRequest.head?.repo?.fork) {
            console.info("Skipping Jira status sync for a fork pull request because secrets are unavailable.")
            return
        }

        throw new Error(
            "JIRA_AUTOMATION_WEBHOOK_URL and JIRA_AUTOMATION_WEBHOOK_TOKEN must be configured as repository secrets.",
        )
    }

    await Promise.all(
        update.issueKeys.map(async (issueKey) => {
            const response = await fetch(webhookUrl, {
                body: JSON.stringify({
                    event: update.event,
                    issueKey,
                    pullRequest: {
                        number: update.pullRequest.number,
                        title: update.pullRequest.title,
                        url: update.pullRequest.html_url,
                    },
                    reviewer: update.reviewer,
                    targetStatus: update.targetStatus,
                }),
                headers: {
                    "Content-Type": "application/json",
                    "X-Automation-Webhook-Token": webhookToken,
                },
                method: "POST",
            })

            if (!response.ok)
                throw new Error(
                    `Jira webhook for ${issueKey} returned ${response.status}`,
                )

            console.info(
                `Requested ${update.targetStatus} for ${issueKey} from pull request #${update.pullRequest.number}.`,
            )
        }),
    )
}

const update = await getUpdate()
if (!update) process.exit(0)

const issueKeys = issueKeysFor(update.pullRequest)
if (issueKeys.length === 0) {
    console.info("No SYNTH work item key found in the pull request title, body, or branch name.")
    process.exit(0)
}

await sendToJira({ ...update, issueKeys })
