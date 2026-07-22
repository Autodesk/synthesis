// Runs before every test file. See stubs below for what each works around.

// Node (v22+) ships a broken global `localStorage` that throws unless `--localstorage-file` is
// set. Vitest's jsdom setup only overrides globals that are jsdom's own property, but jsdom's
// `localStorage` is a prototype-level getter, so Node's throwing version wins. Replace it with a
// trivial in-memory `Storage` so `MirabufLoader.ts`'s module-load-time cache check works.
class MemoryStorage implements Storage {
    private _map = new Map<string, string>()
    get length() {
        return this._map.size
    }
    clear = () => this._map.clear()
    getItem = (key: string) => this._map.get(key) ?? null
    key = (index: number) => Array.from(this._map.keys())[index] ?? null
    removeItem = (key: string) => void this._map.delete(key)
    setItem = (key: string, value: string) => void this._map.set(key, String(value))
}

Object.defineProperty(globalThis, "localStorage", {
    value: new MemoryStorage(),
    configurable: true,
    writable: true,
})

// jsdom has no Web Worker support. `WPILibTypes.ts` does `new WPILibWSWorker()` (a vite `?worker`
// import compiling to a class extending global `Worker`) unconditionally at module-load time
// (`WPILibBrain.ts` top level), reachable from `PhysicsSystem.ts` via `MirabufSceneObject.ts`.
// Stub just enough of the constructor/`EventTarget` surface for that import chain to evaluate.
// No worker actually needs to run.
class StubWorker extends EventTarget implements Worker {
    onmessage = null
    onmessageerror = null
    onerror = null
    postMessage() {}
    terminate() {}
}
globalThis.Worker = StubWorker as unknown as typeof Worker

// Real app modules (`PhysicsSystem.ts` -> ... -> `DefaultMatchModeConfigs.ts` /
// `DefaultAssetLoader.ts`) fire `fetch()` against relative "/api/..." URLs from a `static {}`
// initializer at module-load time. There's no dev server in this suite, and a relative URL has no
// base to resolve against outside a real browser document (`TypeError: Failed to parse URL from
// /api/...`). So `fetch` itself is stubbed here, matched by substring rather than full URL
// parsing, before those modules ever get imported.
const manifestsByUrlSubstring: Record<string, unknown> = {
    "/match_configs/manifest.json": { private: {}, public: {} },
    "/mira/manifest.json": {},
}

globalThis.fetch = (async (input: RequestInfo | URL) => {
    const url = typeof input === "string" ? input : input instanceof URL ? input.href : input.url
    const match = Object.entries(manifestsByUrlSubstring).find(([suffix]) => url.includes(suffix))
    if (!match) {
        return new Response(null, { status: 404 })
    }
    return new Response(JSON.stringify(match[1]), { status: 200, headers: { "Content-Type": "application/json" } })
}) as typeof fetch
