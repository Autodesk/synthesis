/** An event to save whatever configuration interface is open when it is closed */
export class ConfigurationSavedEvent extends Event {
    public constructor() {
        super("ConfigurationSaved")

        window.dispatchEvent(this)
    }

    public static Listen(func: (e: Event) => void) {
        window.addEventListener("ConfigurationSaved", func)
    }

    public static RemoveListener(func: (e: Event) => void) {
        window.removeEventListener("ConfigurationSaved", func)
    }
}
