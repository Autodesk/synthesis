import { afterAll, describe, expect, test } from "bun:test"
import { mkdtemp, rm, writeFile } from "node:fs/promises"
import { tmpdir } from "node:os"
import { join } from "node:path"

type Review = {
    id: number
    state: string
    submitted_at: string
    user: { login: string }
}

type Scenario = {
    action: "review_requested" | "submitted"
    fork?: boolean
    requestedReviewer?: string
    requestedReviewers?: string[]
    review?: { id: number; state: string }
    reviews: Review[]
    sourceEvent: "pull_request" | "pull_request_review"
    title?: string
}

type WebhookRequest = {
    body: unknown
    token: string | null
}

const temporaryDirectories: string[] = []

afterAll(async () => {
    await Promise.all(temporaryDirectories.map(directory => rm(directory, { force: true, recursive: true })))
})

const execute = async (scenario: Scenario) => {
    const webhookRequests: WebhookRequest[] = []
    const server = Bun.serve({
        hostname: "127.0.0.1",
        port: 0,
        async fetch(request) {
            const url = new URL(request.url)

            if (url.pathname === "/repos/Autodesk/synthesis/pulls/42") {
                return Response.json({
                    head: {
                        ref: "feature",
                        repo: {
                            fork: scenario.fork ?? false,
                            full_name: "Autodesk/synthesis",
                        },
                    },
                    number: 42,
                    title: scenario.title ?? "SYNTH-123 Test PR",
                })
            }

            if (url.pathname === "/repos/Autodesk/synthesis/pulls/42/reviews") return Response.json(scenario.reviews)

            if (url.pathname === "/repos/Autodesk/synthesis/pulls/42/requested_reviewers") {
                return Response.json({
                    teams: [],
                    users: (scenario.requestedReviewers ?? []).map(login => ({
                        login,
                    })),
                })
            }

            if (url.pathname === "/jira" && request.method === "POST") {
                webhookRequests.push({
                    body: await request.json(),
                    token: request.headers.get("X-Automation-Webhook-Token"),
                })
                return new Response(null, { status: 200 })
            }

            return new Response("not found", { status: 404 })
        },
    })

    const temporaryDirectory = await mkdtemp(join(tmpdir(), "jira-review-status-test-"))
    temporaryDirectories.push(temporaryDirectory)
    const eventPath = join(temporaryDirectory, "event.json")
    await writeFile(
        eventPath,
        JSON.stringify({
            action: scenario.action,
            pull_request: { number: 42 },
            repository: { full_name: "Autodesk/synthesis" },
            requested_reviewer: scenario.requestedReviewer ? { login: scenario.requestedReviewer } : undefined,
            review: scenario.review,
        })
    )

    try {
        const child = Bun.spawn([process.execPath, join(import.meta.dir, "sync-pr-review-status.ts")], {
            env: {
                ...process.env,
                GITHUB_API_URL: `http://127.0.0.1:${server.port}`,
                GITHUB_EVENT_PATH: eventPath,
                GITHUB_REPOSITORY: "Autodesk/synthesis",
                GITHUB_TOKEN: "test-github-token",
                JIRA_AUTOMATION_WEBHOOK_TOKEN: "test-webhook-token",
                JIRA_AUTOMATION_WEBHOOK_URL: `http://127.0.0.1:${server.port}/jira`,
                JIRA_SOURCE_EVENT_NAME: scenario.sourceEvent,
                JIRA_SOURCE_HEAD_BRANCH: "feature",
                JIRA_SOURCE_HEAD_REPOSITORY: "Autodesk/synthesis",
            },
            stderr: "pipe",
            stdout: "pipe",
        })
        const [exitCode, stderr, stdout] = await Promise.all([
            child.exited,
            new Response(child.stderr).text(),
            new Response(child.stdout).text(),
        ])

        return {
            exitCode,
            stderr: stderr.trim(),
            stdout: stdout.trim(),
            webhookRequests,
        }
    } finally {
        server.stop(true)
    }
}

const reviewedAt = "2026-09-25T12:00:00Z"
const review = (id: number, login: string, state: string): Review => ({
    id,
    state,
    submitted_at: reviewedAt,
    user: { login },
})

describe("Jira pull request review status synchronization", () => {
    test("requests Addressing Feedback for a current change request", async () => {
        const result = await execute({
            action: "submitted",
            review: { id: 1, state: "changes_requested" },
            reviews: [review(1, "alice", "CHANGES_REQUESTED")],
            sourceEvent: "pull_request_review",
        })

        expect(result.exitCode).toBe(0)
        expect(result.stderr).toBe("")
        expect(result.webhookRequests).toEqual([
            {
                body: { event: "changes_requested", issueKey: "SYNTH-123" },
                token: "test-webhook-token",
            },
        ])
    })

    test("requests In Review when all change request reviewers are requested again", async () => {
        const result = await execute({
            action: "review_requested",
            requestedReviewer: "alice",
            requestedReviewers: ["alice"],
            reviews: [review(1, "alice", "CHANGES_REQUESTED")],
            sourceEvent: "pull_request",
        })

        expect(result.exitCode).toBe(0)
        expect(result.webhookRequests[0]?.body).toEqual({
            event: "review_requested",
            issueKey: "SYNTH-123",
        })
    })

    test("ignores later comments when finding the requested reviewer's decision", async () => {
        const result = await execute({
            action: "review_requested",
            requestedReviewer: "alice",
            requestedReviewers: ["alice"],
            reviews: [review(1, "alice", "CHANGES_REQUESTED"), review(2, "alice", "COMMENTED")],
            sourceEvent: "pull_request",
        })

        expect(result.exitCode).toBe(0)
        expect(result.webhookRequests[0]?.body).toEqual({
            event: "review_requested",
            issueKey: "SYNTH-123",
        })
    })

    test("keeps Addressing Feedback when another reviewer comments after requesting changes", async () => {
        const result = await execute({
            action: "review_requested",
            requestedReviewer: "alice",
            requestedReviewers: ["alice"],
            reviews: [
                review(1, "alice", "CHANGES_REQUESTED"),
                review(2, "bob", "CHANGES_REQUESTED"),
                review(3, "bob", "COMMENTED"),
            ],
            sourceEvent: "pull_request",
        })

        expect(result.exitCode).toBe(0)
        expect(result.webhookRequests[0]?.body).toEqual({
            event: "changes_requested",
            issueKey: "SYNTH-123",
        })
    })

    test("reconciles a delayed change request event to the current In Review state", async () => {
        const result = await execute({
            action: "submitted",
            requestedReviewers: ["alice"],
            review: { id: 1, state: "changes_requested" },
            reviews: [review(1, "alice", "CHANGES_REQUESTED")],
            sourceEvent: "pull_request_review",
        })

        expect(result.exitCode).toBe(0)
        expect(result.webhookRequests[0]?.body).toEqual({
            event: "review_requested",
            issueKey: "SYNTH-123",
        })
    })

    test("uses GitHub review order when decision timestamps are equal", async () => {
        const result = await execute({
            action: "review_requested",
            requestedReviewer: "alice",
            requestedReviewers: ["alice"],
            reviews: [review(1, "alice", "CHANGES_REQUESTED"), review(2, "alice", "APPROVED")],
            sourceEvent: "pull_request",
        })

        expect(result.exitCode).toBe(0)
        expect(result.webhookRequests).toEqual([])
    })

    test("does not send Jira secrets for fork pull requests", async () => {
        const result = await execute({
            action: "submitted",
            fork: true,
            review: { id: 1, state: "changes_requested" },
            reviews: [review(1, "alice", "CHANGES_REQUESTED")],
            sourceEvent: "pull_request_review",
        })

        expect(result.exitCode).toBe(0)
        expect(result.stdout).toContain("source repository is a fork")
        expect(result.webhookRequests).toEqual([])
    })
})
