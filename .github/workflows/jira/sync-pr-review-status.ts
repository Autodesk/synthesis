type Repository = {
    fork?: boolean
    full_name?: string
}

type PullRequest = {
    head?: { ref?: string; repo?: Repository | null }
    number?: number
    title?: string
}

type Review = {
    id?: number
    state?: string
    user?: { login?: string }
}

type ReviewRequests = {
    users?: { login?: string }[]
}

type GitHubEvent = {
    action?: string
    pull_request?: { number?: number }
    repository?: Repository
    requested_reviewer?: { login?: string }
    review?: { id?: number; state?: string }
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
const repositoryFullName = process.env.GITHUB_REPOSITORY
const sourceEventName = process.env.JIRA_SOURCE_EVENT_NAME
const sourceHeadBranch = process.env.JIRA_SOURCE_HEAD_BRANCH
const sourceHeadRepository = process.env.JIRA_SOURCE_HEAD_REPOSITORY
const githubApiUrl = (process.env.GITHUB_API_URL ?? "https://api.github.com").replace(/\/$/, "")
const webhookUrl = process.env.JIRA_AUTOMATION_WEBHOOK_URL
const webhookToken = process.env.JIRA_AUTOMATION_WEBHOOK_TOKEN
const githubToken = process.env.GITHUB_TOKEN

if (!eventPath) throw new Error("GITHUB_EVENT_PATH is unavailable")
if (!repositoryFullName) throw new Error("GITHUB_REPOSITORY is unavailable")
if (!sourceEventName) throw new Error("JIRA_SOURCE_EVENT_NAME is unavailable")
if (!sourceHeadBranch) throw new Error("JIRA_SOURCE_HEAD_BRANCH is unavailable")
if (!sourceHeadRepository) throw new Error("JIRA_SOURCE_HEAD_REPOSITORY is unavailable")
if (!githubToken) throw new Error("GITHUB_TOKEN is unavailable")

const [owner, repository, unexpectedRepositoryPath] = repositoryFullName.split("/")
if (!owner || !repository || unexpectedRepositoryPath)
    throw new Error("GITHUB_REPOSITORY must be in the form owner/repository")

const event = JSON.parse(await Bun.file(eventPath).text()) as GitHubEvent

if (event.repository?.full_name !== repositoryFullName)
    throw new Error("The recorded event belongs to a different repository")

const normalizeLogin = (login: string | undefined) => login?.toLowerCase()

const reviewDecisionStates = new Set(["APPROVED", "CHANGES_REQUESTED", "DISMISSED"])

const latestReviewDecisionsByReviewer = (reviews: Review[]) => {
    const latest = new Map<string, Review>()

    // GitHub returns reviews in chronological order. COMMENTED and PENDING
    // reviews do not replace a reviewer's approval or change request.
    for (const review of reviews) {
        const reviewer = normalizeLogin(review.user?.login)
        const state = review.state?.toUpperCase()
        if (!reviewer || !state || !reviewDecisionStates.has(state)) continue

        latest.set(reviewer, review)
    }

    return latest
}

const issueKeyFor = (pullRequest: PullRequest) => {
    const issueKeys = [...new Set((pullRequest.title?.match(/\bSYNTH-\d+\b/gi) ?? []).map(key => key.toUpperCase()))]

    if (issueKeys.length === 1) return issueKeys[0]

    if (issueKeys.length === 0) console.info("No SYNTH work item key found in the pull request title.")
    else console.info("Multiple SYNTH work item keys found in the pull request title; refusing to transition Jira.")

    return null
}

const githubApi = async <T>(path: string): Promise<T> => {
    const response = await fetch(`${githubApiUrl}/repos/${owner}/${repository}${path}`, {
        headers: {
            Accept: "application/vnd.github+json",
            Authorization: `Bearer ${githubToken}`,
            "X-GitHub-Api-Version": "2026-03-10",
        },
    })

    if (!response.ok) throw new Error(`GitHub API request failed with status ${response.status}`)

    return (await response.json()) as T
}

const pullRequestFor = async (number: number) => {
    if (!Number.isSafeInteger(number) || number < 1) throw new Error("The recorded pull request number is invalid")

    const pullRequest = await githubApi<PullRequest>(`/pulls/${number}`)
    if (pullRequest.number !== number) throw new Error("GitHub returned a different pull request number")
    if (pullRequest.head?.ref !== sourceHeadBranch || pullRequest.head.repo?.full_name !== sourceHeadRepository)
        throw new Error("The recorded event does not match the workflow run source")

    return pullRequest
}

const pullRequestReviews = async (number: number) => {
    const reviews: Review[] = []
    for (let page = 1; ; page += 1) {
        const pageReviews = await githubApi<Review[]>(`/pulls/${number}/reviews?per_page=100&page=${page}`)
        reviews.push(...pageReviews)
        if (pageReviews.length < 100) return reviews
    }
}

const pullRequestReviewRequests = async (number: number) =>
    githubApi<ReviewRequests>(`/pulls/${number}/requested_reviewers?per_page=100`)

const getUpdate = async (): Promise<StatusUpdate | null> => {
    if (
        sourceEventName === "pull_request_review" &&
        (event.action !== "submitted" || event.review?.state?.toLowerCase() !== "changes_requested")
    )
        return null

    if (sourceEventName === "pull_request" && (event.action !== "review_requested" || !event.requested_reviewer?.login))
        return null

    const recordedPullRequestNumber = event.pull_request?.number
    if (!recordedPullRequestNumber) return null

    const pullRequest = await pullRequestFor(recordedPullRequestNumber)
    const reviews = await pullRequestReviews(recordedPullRequestNumber)
    const latestReviews = latestReviewDecisionsByReviewer(reviews)

    if (sourceEventName === "pull_request_review") {
        const reviewId = event.review.id
        if (!Number.isSafeInteger(reviewId) || reviewId < 1) throw new Error("The recorded review ID is invalid")

        const review = reviews.find(candidate => candidate.id === reviewId)
        if (review?.state?.toUpperCase() !== "CHANGES_REQUESTED") return null
    }

    if (sourceEventName === "pull_request") {
        const requestedReviewer = normalizeLogin(event.requested_reviewer?.login)
        if (!requestedReviewer || latestReviews.get(requestedReviewer)?.state?.toUpperCase() !== "CHANGES_REQUESTED")
            return null
    }

    const reviewRequests = await pullRequestReviewRequests(recordedPullRequestNumber)
    const requestedReviewers = new Set(
        (reviewRequests.users ?? [])
            .map(reviewer => normalizeLogin(reviewer.login))
            .filter((reviewer): reviewer is string => Boolean(reviewer))
    )
    const changeRequestReviewers = [...latestReviews.entries()]
        .filter(([, review]) => review.state?.toUpperCase() === "CHANGES_REQUESTED")
        .map(([reviewer]) => reviewer)

    if (changeRequestReviewers.length === 0) return null

    if (changeRequestReviewers.some(reviewer => !requestedReviewers.has(reviewer))) {
        return {
            event: "changes_requested",
            pullRequest,
            targetStatus: "Addressing Feedback",
        }
    }

    return {
        event: "review_requested",
        pullRequest,
        targetStatus: "In Review",
    }
}

const sendToJira = async (update: JiraUpdate) => {
    const sourceRepository = update.pullRequest.head?.repo
    if (sourceRepository?.fork !== false || sourceRepository.full_name !== repositoryFullName) {
        console.info(
            "Skipping Jira status sync because the pull request source repository is a fork or is unavailable."
        )
        return
    }

    if (!webhookUrl || !webhookToken)
        throw new Error(
            "JIRA_AUTOMATION_WEBHOOK_URL and JIRA_AUTOMATION_WEBHOOK_TOKEN must be configured as repository secrets."
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

    if (!response.ok) throw new Error(`Jira webhook for ${update.issueKey} returned ${response.status}`)

    console.info(
        `Requested ${update.targetStatus} for ${update.issueKey} from pull request #${update.pullRequest.number}.`
    )
}

const update = await getUpdate()
if (update) {
    const issueKey = issueKeyFor(update.pullRequest)
    if (issueKey) await sendToJira({ ...update, issueKey })
}
