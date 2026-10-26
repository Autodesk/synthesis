import EventSystem from "@/systems/EventSystem.ts"

let nextHandleId = 0

export enum ProgressHandleStatus {
    IN_PROGRESS = 0,
    DONE = 1,
    ERROR = 2,
}

export class ProgressHandle {
    public readonly handleId: number
    public readonly title: string
    public message: string = ""
    public progress: number = 0.0
    public status: ProgressHandleStatus = ProgressHandleStatus.IN_PROGRESS

    public constructor(title: string) {
        this.handleId = nextHandleId++
        this.title = title

        this.push()
    }

    public update(message: string, progress: number, status?: ProgressHandleStatus) {
        this.message = message
        this.progress = progress
        if (status) {
            this.status = status
        }

        this.push()
    }

    public fail(message?: string) {
        this.update(message ?? "Failed", 1, ProgressHandleStatus.ERROR)
    }

    public done(message?: string) {
        this.update(message ?? "Done", 1, ProgressHandleStatus.DONE)
    }

    public push() {
        EventSystem.dispatch("ProgressEvent", this)
    }
}

export const URDFImportProgressBar = {
    LOAD_MESHES: 0.2,
    BUILD_PARTS: 0.7,
    BUILD_HIERARCHY: 0.8,
    MIRABUF_INSTANCE: 0.85,
} as const satisfies Record<string, number>
