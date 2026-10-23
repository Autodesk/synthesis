import { afterAll, describe, expect, test } from "bun:test"
import { mkdtemp, rm, writeFile } from "node:fs/promises"
import { tmpdir } from "node:os"
import { join } from "node:path"

type Scenario = {
    action: "submitted" | "review_requested"
    requestedReviewer?: string
    requestedReviewers?: string[]
    review?: { id: number; state: string }
    reviews: { id: number; state: string; user: { login: string } }[]
    sourceEvent: "pull_request" | "pull_request_review"
}

const temporaryDirectories: string[] = []

afterAll(async () => {
    await Promise.all(temporaryDirectories.map(directory => rm(directory, { force: true, recursive: true })))
})

const execute = async (scenario: Scenario) => {
    const webhookRequests: { body: unknown; token: string | null }[] = []
    const server = Bun.serve({
        hostname: "127.0.0.1",
        port: 0,
        async fetch(request) {
            const url = new URL(request.url)

            if (url.pathname === "/repos/Autodesk/synthesis/pulls/42") {
                return Response.json({
                    head: {
                        ref: "feature",
                        repo: { fork: false, full_name: "Autodesk/synthesis" },
                    },
                    number: 42,
                    title: "fix: [SYNTH-123] Test PR",
                })
            }

            if (url.pathname === "/repos/Autodesk/synthesis/pulls/42/reviews") return Response.json(scenario.reviews)

            if (url.pathname === "/repos/Autodesk/synthesis/pulls/42/requested_reviewers") {
                return Response.json({
                    teams: [],
                    users: (scenario.requestedReviewers ?? []).map(login => ({ login })),
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

        return { exitCode, stderr: stderr.trim(), stdout: stdout.trim(), webhookRequests }
    } finally {
        server.stop(true)
    }
}

describe("Jira pull request review status synchronization", () => {
    test("requests In Progress when the change requester is not requested again", async () => {
        const result = await execute({
            action: "submitted",
            review: { id: 1, state: "changes_requested" },
            reviews: [{ id: 1, state: "CHANGES_REQUESTED", user: { login: "alice" } }],
            sourceEvent: "pull_request_review",
        })

        expect(result.exitCode).toBe(0)
        expect(result.stderr).toBe("")
        expect(result.webhookRequests[0]?.body).toEqual({
            event: "changes_requested",
            issueKey: "SYNTH-123",
            targetStatus: "In Progress",
        })
    })

    test("requests In Progress from the submitted change request event", async () => {
        const result = await execute({
            action: "submitted",
            review: { id: 1, state: "changes_requested" },
            reviews: [],
            sourceEvent: "pull_request_review",
        })

        expect(result.exitCode).toBe(0)
        expect(result.stderr).toBe("")
        expect(result.webhookRequests).toEqual([
            {
                body: { event: "changes_requested", issueKey: "SYNTH-123", targetStatus: "In Progress" },
                token: "test-webhook-token",
            },
        ])
    })

    test("requests nothing when the change requester later approved", async () => {
        const result = await execute({
            action: "submitted",
            review: { id: 1, state: "changes_requested" },
            reviews: [
                { id: 1, state: "CHANGES_REQUESTED", user: { login: "alice" } },
                { id: 2, state: "APPROVED", user: { login: "alice" } },
            ],
            sourceEvent: "pull_request_review",
        })

        expect(result.exitCode).toBe(0)
        expect(result.webhookRequests).toEqual([])
    })

    test("requests nothing when the submitted change request was dismissed", async () => {
        const result = await execute({
            action: "submitted",
            review: { id: 1, state: "changes_requested" },
            reviews: [{ id: 1, state: "DISMISSED", user: { login: "alice" } }],
            sourceEvent: "pull_request_review",
        })

        expect(result.exitCode).toBe(0)
        expect(result.webhookRequests).toEqual([])
    })

    test("requests Reopened when the change requester is already requested again", async () => {
        const result = await execute({
            action: "submitted",
            requestedReviewers: ["alice"],
            review: { id: 1, state: "changes_requested" },
            reviews: [{ id: 1, state: "CHANGES_REQUESTED", user: { login: "alice" } }],
            sourceEvent: "pull_request_review",
        })

        expect(result.exitCode).toBe(0)
        expect(result.webhookRequests[0]?.body).toEqual({
            event: "review_requested",
            issueKey: "SYNTH-123",
            targetStatus: "Reopened",
        })
    })

    test("requests Reopened when a change-requested reviewer is requested again", async () => {
        const result = await execute({
            action: "review_requested",
            requestedReviewer: "alice",
            requestedReviewers: ["alice"],
            reviews: [{ id: 1, state: "CHANGES_REQUESTED", user: { login: "alice" } }],
            sourceEvent: "pull_request",
        })

        expect(result.exitCode).toBe(0)
        expect(result.stderr).toBe("")
        expect(result.webhookRequests[0]?.body).toEqual({
            event: "review_requested",
            issueKey: "SYNTH-123",
            targetStatus: "Reopened",
        })
    })
})
