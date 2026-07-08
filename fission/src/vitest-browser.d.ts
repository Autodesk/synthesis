// vitest/browser re-exports from provider-specific adapter packages (playwright, webdriverio,
// etc.) that are not installed in this project. @vitest/browser is the installed package and
// contains the canonical LocatorSelectors/Locator types. This augmentation bridges the gap so
// vitest-browser-react's RenderResult (which extends LocatorSelectors from "vitest/browser")
// resolves getByText and the rest of the locator API.
// declare module "vitest/browser" {
//     export * from "@vitest/browser/context"
// }
