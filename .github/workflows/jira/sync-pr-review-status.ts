type PullRequest = {
    head?: { repo?: { fork?: boolean } | null }
    number?: number
    title?: string
}

type Review = {
    id?: number
    state?: string
    submitted_at?: string | null
    user?: { login?: string }
}

type GitHubEvent = {
    action?: string
    pull_request?: PullRequest
    requested_reviewer?: { login?: string }
    review?: { state?: string }
}

type JiraEvent = "changes_requested" | "review_requested"

type JiraUpdate = {
    event: JiraEvent
    issueKey: string
    pullRequest: PullRequest
    targetStatus: "Addressing Feedback" | "In Review"
}

type StatusUpdate = Omit<JiraUpdate, "issueKey">

const eventPath = process.env.GITHUB_EVENT_PATH
if (!eventPath) throw new Error("GITHUB_EVENT_PATH is unavailable")

const event = JSON.parse(await Bun.file(eventPath).text()) as GitHubEvent
const [owner, repository, unexpectedRepositoryPath] = (
    process.env.GITHUB_REPOSITORY ?? ""
).split("/")
const webhookUrl = process.env.JIRA_AUTOMATION_WEBHOOK_URL
const webhookToken = process.env.JIRA_AUTOMATION_WEBHOOK_TOKEN
const githubToken = process.env.GITHUB_TOKEN

if (!owner || !repository || unexpectedRepositoryPath)
    throw new Error("GITHUB_REPOSITORY must be in the form owner/repository")

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

const issueKeyFor = (pullRequest: PullRequest) => {
    const issueKeys = [
        ...new Set(
            (pullRequest.title?.match(/\bSYNTH-\d+\b/gi) ?? [])
                .map((key) => key.toUpperCase()),
        ),
    ]

    if (issueKeys.length === 1) return issueKeys[0]

    if (issueKeys.length === 0)
        console.info("No SYNTH work item key found in the pull request title.")
    else
        console.info(
            "Multiple SYNTH work item keys found in the pull request title; refusing to transition Jira.",
        )

    return null
}

const pullRequestReviews = async (pullRequest: PullRequest) => {
    const number = pullRequest.number

    if (!number || !githubToken)
        throw new Error("GitHub pull request number or token is unavailable")

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
    const requestedReviewer = normalizeLogin(event.requested_reviewer?.login)
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
        targetStatus: "In Review" as const,
    }
}

const getUpdate = async (): Promise<StatusUpdate | null> => {
    const pullRequest = event.pull_request
    if (!pullRequest) return null

    if (
        process.env.GITHUB_EVENT_NAME === "pull_request_review" &&
        event.review?.state?.toLowerCase() === "changes_requested"
    )
        return {
            event: "changes_requested" as const,
            pullRequest,
            targetStatus: "Addressing Feedback" as const,
        }

    if (
        process.env.GITHUB_EVENT_NAME === "pull_request" &&
        event.action === "review_requested"
    )
        return reviewRequestUpdate(pullRequest)

    return null
}

const sendToJira = async (update: JiraUpdate) => {
    if (update.pullRequest.head?.repo?.fork !== false) {
        console.info(
            "Skipping Jira status sync because the pull request source repository is a fork or is unavailable.",
        )
        return
    }

    if (!webhookUrl || !webhookToken)
        throw new Error(
            "JIRA_AUTOMATION_WEBHOOK_URL and JIRA_AUTOMATION_WEBHOOK_TOKEN must be configured as repository secrets.",
        )

    const response = await fetch(webhookUrl, {
        body: JSON.stringify({
            event: update.event,
            issueKey: update.issueKey,
        }),
        headers: {
            "Content-Type": "application/json",
            "X-Automation-Webhook-Token": webhookToken,
        },
        method: "POST",
    })

    if (!response.ok)
        throw new Error(`Jira webhook for ${update.issueKey} returned ${response.status}`)

    console.info(
        `Requested ${update.targetStatus} for ${update.issueKey} from pull request #${update.pullRequest.number}.`,
    )
}

const update = await getUpdate()
if (update) {
    const issueKey = issueKeyFor(update.pullRequest)
    if (issueKey) await sendToJira({ ...update, issueKey })
}
