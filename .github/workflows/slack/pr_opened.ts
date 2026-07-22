const webhookUrl = process.env.SLACK_WEBHOOK_URL;
const github = JSON.parse(process.env.GITHUB_DATA ?? "{}")

if (!webhookUrl) {
    console.error("SLACK_WEBHOOK_URL is required");
    process.exit(1);
}

function capitalize(str: string) {
    return str.charAt(0).toUpperCase() + str.slice(1);
}

// Actions: opened, closed, merged, reopened, assigned, unassigned, labeled, unlabeled

let image_url;
switch (github.event.action) {
    case "opened":
    case "reopened":
        image_url = `https://github.com/synthesis-adsk/github-icons/blob/main/icons/${github.event.pull_request.draft ? "pull-request-draft-gray" : "pull-request-green"}.png?raw=true`
        break
    case "merged":
        image_url = "https://github.com/synthesis-adsk/github-icons/blob/main/icons/pull-request-merged-purple.png?raw=true"
        break
    case "closed":
        image_url = "https://github.com/synthesis-adsk/github-icons/blob/main/icons/pull-request-closed-red.png?raw=true"
        break
}

const regexp = /SYNTH-\d+/g
const jira_matches = github.event.pull_request.title.match(regexp)
    ?? github.event.pull_request.body.match(regexp)
    ?? []

const payload = {
    "blocks": [
        {
            "type": "container",
            "width": "full",
            "title": {
                "type": "plain_text",
                "text": `#${github.event.number} - ${github.event.pull_request.title}`
            },
            "subtitle": {
                "type": "plain_text",
                "text": `${github.event.pull_request.draft ? 'Draft ' : ''}${capitalize(github.event.action)} by ${github.actor}`
            },
            "icon": {
                "type": "image",
                "image_url": image_url,
                "alt_text": `Pull Request ${github.event.action}`
            },
            "child_blocks": [
                {
                    "type": "section",
                    "text": {
                        "type": "mrkdwn",
                        "text": `${jira_matches.length >= 1 ? 'Jira: `' + jira_matches[0] + '`' : 'No Jira ticket attached'}`
                    },
                    "accessory": {
                        "type": "button",
                        "text": {
                            "type": "plain_text",
                            "text": "Visit",
                            "emoji": true
                        },
                        "url": github.event.pull_request.html_url
                    }
                }
            ]
        }
    ]
}

const send = async () => {
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
