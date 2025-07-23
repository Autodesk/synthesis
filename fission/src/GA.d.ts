import { AnalyticsPlugin } from "analytics"

declare module "@haensl/google-analytics" {
    type GaEvent = { name: string; params: { [key: string]: string | number } }
    type GaException = { description: string; fatal: boolean }

    function init(params: { [key: string]: unknown }): void
    function consent(granted: boolean): void
    function event(e: GaEvent)
    function exception(e: GaException)
    function setUserId({ id }: { id: string })
    function setUserProperty({ name, value }: { name: string; value: string })
}

declare module "@analytics/google-analytics" {
    type GoogleAnalyticsOptions = {
        /** Google Analytics site tracking Id */
        trackingId: string

        /** Enable Google Analytics debug mode */
        debug?: boolean

        /** Enable Anonymizing IP addresses sent to Google Analytics. See details below */
        anonymizeIp?: boolean

        /** Map Custom dimensions to send extra information to Google Analytics. See details below */
        customDimensions?: object

        /** Reset custom dimensions by key on analytics.page() calls. Useful for single page apps. */
        resetCustomDimensionsOnPage?: object

        /** Mapped dimensions will be set to the page & sent as properties of all subsequent events on that page. If false, analytics will only pass custom dimensions as part of individual events */
        setCustomDimensionsToPage?: boolean

        /** Custom tracker name for google analytics. Use this if you need multiple googleAnalytics scripts loaded */
        instanceName?: string

        /** Custom URL for google analytics script, if proxying calls */
        customScriptSrc?: string

        /** Additional cookie properties for configuring the ga cookie */
        cookieConfig?: object

        /** Set custom google analytic tasks */
        tasks?: object
    }
    function GoogleAnalytics(options: GoogleAnalyticsOptions): AnalyticsPlugin
    export default GoogleAnalytics
}
