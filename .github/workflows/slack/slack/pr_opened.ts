import { MessageBuilder, prEventFromContext, prNotification, prSummary } from "./index";

const webhookUrl = process.env.SLACK_WEBHOOK_URL;
const github = JSON.parse(process.env.GITHUB_DATA ?? "{}")

if (!webhookUrl) {
    console.error("SLACK_WEBHOOK_URL is required");
    process.exit(1);
}

// Actions: opened, closed, merged, reopened, assigned, unassigned, labeled, unlabeled

const send = async () => {
    const event = prEventFromContext(github)
    const message = new MessageBuilder(prSummary(event)).add(prNotification(event)).build()

    const slack_res = await fetch(webhookUrl, {
        method: 'POST',
        body: JSON.stringify(message),
        headers: {
            'Content-Type': 'application/json'
        }
    })

    console.log("Slack notification sent:", await slack_res.text())
}

send()
