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
    private static instance: CommandRegistry | null = null

    private staticCommands: Map<string, CommandDefinition> = new Map()
    private providers: Set<CommandProvider> = new Set()
    private listeners: Set<() => void> = new Set()
    private notifyScheduled: boolean = false
    private notifyPending: boolean = false

    static get(): CommandRegistry {
        if (!CommandRegistry.instance) {
            CommandRegistry.instance = new CommandRegistry()
        }
        return CommandRegistry.instance
    }

    /** Register a single static command. Returns an unregister function. */
    registerCommand(command: CommandDefinition): () => void {
        this.staticCommands.set(command.id, command)
        this.notify()
        return () => {
            if (this.staticCommands.get(command.id) === command) {
                this.staticCommands.delete(command.id)
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
        this.providers.add(provider)
        this.notify()
        return () => {
            if (this.providers.delete(provider)) {
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
        for (const [id, cmd] of this.staticCommands) {
            merged.set(id, cmd)
        }
        for (const provider of this.providers) {
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
        this.listeners.add(listener)
        return () => {
            this.listeners.delete(listener)
        }
    }

    private notify() {
        // Coalesce multiple rapid updates into a single microtask flush
        this.notifyPending = true
        if (this.notifyScheduled) return
        this.notifyScheduled = true
        queueMicrotask(() => {
            this.notifyScheduled = false
            if (!this.notifyPending) return
            this.notifyPending = false
            for (const l of this.listeners) {
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
