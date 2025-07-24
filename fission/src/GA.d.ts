declare module "@haensl/google-analytics" {
    type GaEvent = { name: string; params: { [key: string]: string | number } }
    type GaException = { description: string; fatal: boolean }

    function init(params: { [key: string]: unknown }): void
    function consent(granted: boolean): void
    function event(e: GaEvent)
    function exception(e: GaException)
    function setUserId({ id }: { id: string })
    function setUserProperty({ name, value }: { name: string; value: unknown })
    function install()
}
