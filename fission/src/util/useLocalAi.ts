import { pipeline, env, type ZeroShotClassificationPipeline } from "@xenova/transformers"

// Ensure we don't try to fetch models from a local path (Vite may return index.html)
// and avoid browser cache edge-cases that can return stale HTML instead of JSON.
env.allowLocalModels = false
env.useBrowserCache = false

let classifier: ZeroShotClassificationPipeline | null = null

export async function loadCommandClassifier(labels: string[]) {
    if (!classifier) {
        // Use a public, transformers.js-compatible MNLI model
        classifier = await pipeline("zero-shot-classification", "Xenova/mobilebert-uncased-mnli")
    }

    return async (query: string) => {
        if (!classifier) return null
        const result = await classifier(query, labels)
        return result
    }
}
