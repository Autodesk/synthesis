import { render, screen, waitFor } from "@testing-library/react"
import { describe, test, expect, vi, beforeEach, afterEach } from "vitest"
import Synthesis from "@/Synthesis"
import { ThemeProvider } from "@/ui/ThemeContext"
import { Theme } from "@/ui/helpers/UseThemeHelpers"

vi.mock("@/systems/World", () => ({
    default: {
        InitWorld: vi.fn(),
        DestroyWorld: vi.fn(),
        UpdateWorld: vi.fn(),
        SceneRenderer: {
            UpdateSkyboxColors: vi.fn(),
        },
        PhysicsSystem: {
            HoldPause: vi.fn(),
            ReleasePause: vi.fn(),
        },
        get isAlive() {
            return false
        },
    },
}))

vi.mock("@/aps/APS", () => ({
    default: {
        convertAuthToken: vi.fn(),
    },
}))

vi.mock("@/systems/preferences/PreferencesSystem", () => ({
    default: {
        getGlobalPreference: vi.fn((key: string) => {
            const defaults: Record<string, unknown> = {
                ReportAnalytics: false,
                RenderScoreboard: false,
                ShowViewCube: true,
                MuteAllSound: false,
                SFXVolume: 25,
            }
            return defaults[key] ?? false
        }),
        setGlobalPreference: vi.fn(),
        savePreferences: vi.fn(),
        loadPreferences: vi.fn(),
        addEventListener: vi.fn(),
    },
}))

vi.mock("@/components/Scene.tsx", () => ({
    default: () => <div data-testid="scene">Scene</div>,
}))

vi.mock("@/components/MainHUD", () => ({
    default: () => <div data-testid="main-hud">MainHUD</div>,
}))

vi.mock("@/ui/components/Skybox.tsx", () => ({
    default: () => <div data-testid="skybox">Skybox</div>,
}))

vi.mock("@/ui/components/SceneOverlay.tsx", () => ({
    default: () => <div data-testid="scene-overlay">SceneOverlay</div>,
}))

vi.mock("@/ui/components/TouchControls.tsx", () => ({
    default: () => <div data-testid="touch-controls">TouchControls</div>,
    MAX_JOYSTICK_RADIUS: 55,
    TouchControlsAxes: {
        LeftX: 1,
        LeftY: 2,
        RightX: 3,
        RightY: 4,
    },
    TouchControlsEvent: class MockTouchControlsEvent extends Event {
        public value?: boolean
        constructor(eventKey: string, value?: boolean) {
            super(eventKey)
            this.value = value
        }
        static Listen = vi.fn()
        static RemoveListener = vi.fn()
    },
    TouchControlsEventKeys: {
        PLACE_BUTTON: "PlaceButtonEvent",
        JOYSTICK: "JoystickEvent",
    },
}))

vi.mock("@/ui/components/ContextMenu.tsx", () => ({
    default: () => <div data-testid="context-menu">ContextMenu</div>,
}))

vi.mock("@/ui/components/ProgressNotification.tsx", () => ({
    default: () => <div data-testid="progress-notifications">ProgressNotifications</div>,
}))

vi.mock("@/ui/components/GlobalUIComponent.tsx", () => ({
    default: () => <div data-testid="global-ui-component">GlobalUIComponent</div>,
}))

vi.mock("@/ui/components/AnalyticsConsent.tsx", () => ({
    default: ({ onClose, onConsent }: { onClose: () => void; onConsent: () => void }) => (
        <div data-testid="analytics-consent">
            <button onClick={onConsent} data-testid="consent-accept">
                Accept
            </button>
            <button onClick={onClose} data-testid="consent-decline">
                Decline
            </button>
        </div>
    ),
}))

vi.mock("@/ui/components/WPILibConnectionStatus.tsx", () => ({
    default: () => <div data-testid="wpilib-connection-status">WPILibConnectionStatus</div>,
}))

vi.mock("framer-motion", () => ({
    AnimatePresence: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
    motion: {
        div: ({ children, ...props }: any) => <div {...props}>{children}</div>,
    },
}))

Object.defineProperty(window, "requestAnimationFrame", {
    writable: true,
    value: vi.fn((cb: FrameRequestCallback) => {
        setTimeout(cb, 16)
        return 1
    }),
})

Object.defineProperty(window, "cancelAnimationFrame", {
    writable: true,
    value: vi.fn(),
})

const mockURLSearchParams = vi.fn()
Object.defineProperty(window, "URLSearchParams", {
    writable: true,
    value: mockURLSearchParams,
})

Object.defineProperty(window, "opener", {
    writable: true,
    value: {
        convertAuthToken: vi.fn(),
    },
})

Object.defineProperty(window, "close", {
    writable: true,
    value: vi.fn(),
})

const mockTheme: Theme = {
    InteractiveElementSolid: {
        color: { r: 250, g: 162, b: 27, a: 1 },
        above: [],
    },
    InteractiveElementLeft: {
        color: { r: 207, g: 114, b: 57, a: 1 },
        above: ["Background", "BackgroundSecondary"],
    },
    InteractiveElementRight: {
        color: { r: 212, g: 75, b: 62, a: 1 },
        above: ["Background", "BackgroundSecondary"],
    },
    Background: { color: { r: 0, g: 0, b: 0, a: 1 }, above: [] },
    BackgroundSecondary: { color: { r: 18, g: 18, b: 18, a: 1 }, above: [] },
    InteractiveBackground: { color: { r: 40, g: 44, b: 47, a: 1 }, above: [] },
    MainText: {
        color: { r: 255, g: 255, b: 255, a: 1 },
        above: [
            "Background",
            "BackgroundSecondary",
            "BackgroundHUD",
            "InteractiveBackground",
            "InteractiveElementLeft",
            "InteractiveElementRight",
        ],
    },
    Scrollbar: { color: { r: 170, g: 170, b: 170, a: 1 }, above: [] },
    AcceptButton: { color: { r: 33, g: 137, b: 228, a: 1 }, above: [] },
    CancelButton: { color: { r: 248, g: 78, b: 78, a: 1 }, above: [] },
    InteractiveElementText: {
        color: { r: 255, g: 255, b: 255, a: 1 },
        above: [],
    },
    AcceptCancelButtonText: {
        color: { r: 0, g: 0, b: 0, a: 1 },
        above: ["AcceptButton", "CancelButton"],
    },
    BackgroundHUD: { color: { r: 23, g: 23, b: 23, a: 1 }, above: [] },
    InteractiveHover: { color: { r: 150, g: 150, b: 150, a: 1 }, above: [] },
    InteractiveSelect: { color: { r: 100, g: 100, b: 100, a: 1 }, above: [] },
    Icon: {
        color: { r: 255, g: 255, b: 255, a: 1 },
        above: ["Background", "BackgroundSecondary", "InteractiveBackground"],
    },
    MainHUDIcon: {
        color: { r: 255, g: 255, b: 255, a: 1 },
        above: ["BackgroundHUD"],
    },
    MainHUDCloseIcon: {
        color: { r: 0, g: 0, b: 0, a: 1 },
        above: ["InteractiveElementRight", "#ffffff"],
    },
    HighlightHover: { color: { r: 89, g: 255, b: 133, a: 1 }, above: [] },
    HighlightSelect: { color: { r: 255, g: 89, b: 133, a: 1 }, above: [] },
    SkyboxTop: { color: { r: 255, g: 255, b: 255, a: 1 }, above: [] },
    SkyboxBottom: { color: { r: 255, g: 255, b: 255, a: 1 }, above: [] },
    FloorGrid: { color: { r: 93, g: 93, b: 93, a: 1 }, above: [] },
    MatchRedAlliance: { color: { r: 180, g: 20, b: 20, a: 1 }, above: [] },
    MatchBlueAlliance: { color: { r: 20, g: 20, b: 180, a: 1 }, above: [] },
    ToastInfo: { color: { r: 126, g: 34, b: 206, a: 1 }, above: [] },
    ToastWarning: { color: { r: 234, g: 179, b: 8, a: 1 }, above: [] },
    ToastError: { color: { r: 239, g: 68, b: 68, a: 1 }, above: [] },
}

const mockThemes = {
    Default: mockTheme,
}

describe("Synthesis Component Bootstrap Tests", () => {
    beforeEach(() => {
        vi.clearAllMocks()

        mockURLSearchParams.mockImplementation((search: string) => ({
            has: vi.fn((key: string) => search.includes(key)),
            get: vi.fn((key: string) => {
                if (key === "code" && search.includes("code=")) {
                    return "test-auth-code"
                }
                return null
            }),
        }))
    })

    afterEach(() => {
        vi.clearAllTimers()
    })

    test("renders Synthesis component without crashing", () => {
        render(
            <ThemeProvider initialThemeName="Default" themes={mockThemes} defaultTheme={mockTheme}>
                <Synthesis />
            </ThemeProvider>
        )

        expect(screen.getByTestId("skybox")).toBeDefined()
        expect(screen.getByTestId("scene")).toBeDefined()
        expect(screen.getByTestId("main-hud")).toBeDefined()
        expect(screen.getByTestId("global-ui-component")).toBeDefined()
    })

    test("renders all required UI components", () => {
        render(
            <ThemeProvider initialThemeName="Default" themes={mockThemes} defaultTheme={mockTheme}>
                <Synthesis />
            </ThemeProvider>
        )

        expect(screen.getByTestId("skybox")).toBeDefined()
        expect(screen.getByTestId("scene")).toBeDefined()
        expect(screen.getByTestId("scene-overlay")).toBeDefined()
        expect(screen.getByTestId("touch-controls")).toBeDefined()
        expect(screen.getByTestId("context-menu")).toBeDefined()
        expect(screen.getByTestId("main-hud")).toBeDefined()
        expect(screen.getByTestId("progress-notifications")).toBeDefined()
        expect(screen.getByTestId("global-ui-component")).toBeDefined()
        expect(screen.getByTestId("wpilib-connection-status")).toBeDefined()
    })

    test("handles authentication code in URL parameters", () => {
        mockURLSearchParams.mockImplementation((search: string) => ({
            has: vi.fn((key: string) => search.includes(key)),
            get: vi.fn((key: string) => {
                if (key === "code" && search.includes("code=")) {
                    return "test-auth-code"
                }
                return null
            }),
        }))

        render(
            <ThemeProvider initialThemeName="Default" themes={mockThemes} defaultTheme={mockTheme}>
                <Synthesis />
            </ThemeProvider>
        )

        expect(screen.getByTestId("scene")).toBeDefined()
        expect(screen.getByTestId("main-hud")).toBeDefined()
    })

    test("initializes main menu modal on component mount", async () => {
        render(
            <ThemeProvider initialThemeName="Default" themes={mockThemes} defaultTheme={mockTheme}>
                <Synthesis />
            </ThemeProvider>
        )

        await waitFor(() => {
            expect(screen.getByTestId("scene")).toBeDefined()
        })
    })

    test("shows analytics consent popup when required", async () => {
        render(
            <ThemeProvider initialThemeName="Default" themes={mockThemes} defaultTheme={mockTheme}>
                <Synthesis />
            </ThemeProvider>
        )

        expect(screen.getByTestId("scene")).toBeDefined()
        expect(screen.getByTestId("main-hud")).toBeDefined()
    })

    test("does not show analytics consent in development", async () => {
        Object.defineProperty(import.meta, "env", {
            value: { DEV: true },
            configurable: true,
        })

        render(
            <ThemeProvider initialThemeName="Default" themes={mockThemes} defaultTheme={mockTheme}>
                <Synthesis />
            </ThemeProvider>
        )

        expect(screen.queryByTestId("analytics-consent")).toBeNull()
    })

    test("provides all required contexts to child components", () => {
        render(
            <ThemeProvider initialThemeName="Default" themes={mockThemes} defaultTheme={mockTheme}>
                <Synthesis />
            </ThemeProvider>
        )

        expect(screen.getByTestId("global-ui-component")).toBeDefined()
        expect(screen.getByTestId("main-hud")).toBeDefined()
    })

    test("handles cleanup on unmount", () => {
        const { unmount } = render(
            <ThemeProvider initialThemeName="Default" themes={mockThemes} defaultTheme={mockTheme}>
                <Synthesis />
            </ThemeProvider>
        )

        unmount()

        expect(window.cancelAnimationFrame).toHaveBeenCalled()
    })

    test("applies theme correctly", () => {
        render(
            <ThemeProvider initialThemeName="Default" themes={mockThemes} defaultTheme={mockTheme}>
                <Synthesis />
            </ThemeProvider>
        )

        expect(screen.getByTestId("scene")).toBeDefined()
    })

    test("handles missing URL parameters gracefully", () => {
        mockURLSearchParams.mockImplementation(() => ({
            has: vi.fn(() => false),
            get: vi.fn(() => null),
        }))

        render(
            <ThemeProvider initialThemeName="Default" themes={mockThemes} defaultTheme={mockTheme}>
                <Synthesis />
            </ThemeProvider>
        )

        expect(window.opener.convertAuthToken).not.toHaveBeenCalled()
        expect(window.close).not.toHaveBeenCalled()

        expect(screen.getByTestId("scene")).toBeDefined()
    })

    test("initializes with correct modal and panel state", () => {
        render(
            <ThemeProvider initialThemeName="Default" themes={mockThemes} defaultTheme={mockTheme}>
                <Synthesis />
            </ThemeProvider>
        )

        const panelContainer = screen.queryByTestId("panels-container")
        expect(panelContainer).toBeNull()

        expect(screen.getByTestId("scene")).toBeDefined()
        expect(screen.getByTestId("main-hud")).toBeDefined()
    })
})
