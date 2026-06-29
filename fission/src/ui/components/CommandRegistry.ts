export type CommandDefinition = {
    id: string
    label: string
    description?: string
    keywords?: string[]
    perform: () => void
}

export type CommandProvider = () => CommandDefinition[]

/**
 * Central registry for commands used by the Command Palette.
 *
 * Features can register either static commands or dynamic providers. Dynamic providers are functions
 * that return a set of commands at the time of retrieval, which is useful for context-sensitive
 * commands that depend on runtime state.
 *
 * Adoption pattern:
 * - Each feature owns its registrations (e.g., in its initializer or module load).
 * - On enable/mount, call registry.registerCommand(s)/registerProvider and keep the disposer.
 * - On disable/unmount, call the disposer to unregister.
 */
class CommandRegistry {
    private static _instance: CommandRegistry | null = null

    private _staticCommands: Map<string, CommandDefinition> = new Map()
    private _providers: Set<CommandProvider> = new Set()
    private _listeners: Set<() => void> = new Set()
    private _notifyScheduled: boolean = false
    private _notifyPending: boolean = false

    static get(): CommandRegistry {
        if (!CommandRegistry._instance) {
            CommandRegistry._instance = new CommandRegistry()
        }
        return CommandRegistry._instance
    }

    /** Register a single static command. Returns an unregister function. */
    registerCommand(command: CommandDefinition): () => void {
        this._staticCommands.set(command.id, command)
        this.notify()
        return () => {
            if (this._staticCommands.get(command.id) === command) {
                this._staticCommands.delete(command.id)
                this.notify()
            }
        }
    }

    /** Register multiple static commands. Returns an unregister function. */
    registerCommands(commands: CommandDefinition[]): () => void {
        const disposers = commands.map(c => this.registerCommand(c))
        return () => {
            for (const dispose of disposers) {
                try {
                    dispose()
                } catch {
                    console.error("Error in command dispose", dispose)
                }
            }
        }
    }

    /** Register a dynamic provider. Returns an unregister function. */
    registerProvider(provider: CommandProvider): () => void {
        this._providers.add(provider)
        this.notify()
        return () => {
            if (this._providers.delete(provider)) {
                this.notify()
            }
        }
    }

    /**
     * Returns all commands: static ones plus the union of all provider results.
     * If duplicate ids exist, the last one encountered wins (provider results are applied after statics).
     */
    getCommands(): CommandDefinition[] {
        const merged = new Map<string, CommandDefinition>()
        for (const [id, cmd] of this._staticCommands) {
            merged.set(id, cmd)
        }
        for (const provider of this._providers) {
            try {
                const provided = provider() || []
                for (const cmd of provided) {
                    merged.set(cmd.id, cmd)
                }
            } catch {
                console.error("Error in command provider", provider)
            }
        }
        return Array.from(merged.values())
    }

    subscribe(listener: () => void): () => void {
        this._listeners.add(listener)
        return () => {
            this._listeners.delete(listener)
        }
    }

    private notify() {
        // Coalesce multiple rapid updates into a single microtask flush
        this._notifyPending = true
        if (this._notifyScheduled) return
        this._notifyScheduled = true
        queueMicrotask(() => {
            this._notifyScheduled = false
            if (!this._notifyPending) return
            this._notifyPending = false
            for (const l of this._listeners) {
                try {
                    l()
                } catch {
                    console.error("Error in command notify", l)
                }
            }
        })
    }
}

export default CommandRegistry
