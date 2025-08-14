import { pipeline, env, type ZeroShotClassificationPipeline } from "@xenova/transformers"

// Ensure we don't try to fetch models from a local path (Vite may return index.html)
// and avoid browser cache edge-cases that can return stale HTML instead of JSON.
env.allowLocalModels = false
env.useBrowserCache = false

let classifier: ZeroShotClassificationPipeline | null = null
let classifierLoading: Promise<ZeroShotClassificationPipeline> | null = null

export async function loadCommandClassifier(labels: string[]) {
    if (!classifier && !classifierLoading) {
        // Use a public, transformers.js-compatible MNLI model
        console.log("Loading command classifier")
        classifierLoading = pipeline("zero-shot-classification", "Xenova/mobilebert-uncased-mnli")
            .then((loaded) => {
                classifier = loaded
                console.log("Command classifier loaded")
                return loaded
            })
            .finally(() => {
                classifierLoading = null
            })
    }

    if (classifierLoading) {
        await classifierLoading
    }

    return async (query: string) => {
        if (!classifier) return null
        const result = await classifier(query, labels)
        return result
    }
}
