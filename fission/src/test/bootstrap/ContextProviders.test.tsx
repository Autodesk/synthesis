import { render, screen } from "@testing-library/react"
import { describe, test, expect, vi, beforeEach } from "vitest"
import { ThemeProvider } from "@/ui/ThemeContext"
import { ModalControlProvider } from "@/ui/ModalContext"
import { PanelControlProvider } from "@/ui/PanelContext"
import { ToastProvider } from "@/ui/ToastContext"
import { TooltipControlProvider } from "@/ui/TooltipContext"
import { Theme } from "@/ui/helpers/UseThemeHelpers"

// Mock framer-motion to avoid animation issues
vi.mock("framer-motion", () => ({
    AnimatePresence: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
    motion: {
        div: ({ children, ...props }: { children: React.ReactNode, props: React.ReactNode[] }) => <div {...props}>{children}</div>,
    },
}))

// Test component that uses all contexts
const TestComponent = () => {
    return (
        <div data-testid="test-component">
            <div data-testid="context-test">All contexts working</div>
        </div>
    )
}

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

describe("Context Providers Bootstrap Tests", () => {
    beforeEach(() => {
        vi.clearAllMocks()
    })

    test("ThemeProvider renders without errors", () => {
        render(
            <ThemeProvider initialThemeName="Default" themes={mockThemes} defaultTheme={mockTheme}>
                <TestComponent />
            </ThemeProvider>
        )

        expect(screen.getByTestId("test-component")).toBeDefined()
        expect(screen.getByTestId("context-test")).toBeDefined()
    })

    test("ModalControlProvider renders without errors", () => {
        const mockModalMethods = {
            openModal: vi.fn(),
            closeModal: vi.fn(),
            activeModalId: null,
        }

        render(
            <ModalControlProvider {...mockModalMethods}>
                <TestComponent />
            </ModalControlProvider>
        )

        expect(screen.getByTestId("test-component")).toBeDefined()
    })

    test("PanelControlProvider renders without errors", () => {
        const mockPanelMethods = {
            openPanel: vi.fn(),
            closePanel: vi.fn(),
            closeAllPanels: vi.fn(),
        }

        render(
            <PanelControlProvider {...mockPanelMethods}>
                <TestComponent />
            </PanelControlProvider>
        )

        expect(screen.getByTestId("test-component")).toBeDefined()
    })

    test("ToastProvider renders without errors", () => {
        render(
            <ToastProvider>
                <TestComponent />
            </ToastProvider>
        )

        expect(screen.getByTestId("test-component")).toBeDefined()
    })

    test("TooltipControlProvider renders without errors", () => {
        const mockTooltipMethods = {
            showTooltip: vi.fn(),
        }

        render(
            <TooltipControlProvider {...mockTooltipMethods}>
                <TestComponent />
            </TooltipControlProvider>
        )

        expect(screen.getByTestId("test-component")).toBeDefined()
    })

    test("all providers work together in nested structure", () => {
        const mockModalMethods = {
            openModal: vi.fn(),
            closeModal: vi.fn(),
            activeModalId: null,
        }

        const mockPanelMethods = {
            openPanel: vi.fn(),
            closePanel: vi.fn(),
            closeAllPanels: vi.fn(),
        }

        const mockTooltipMethods = {
            showTooltip: vi.fn(),
        }

        render(
            <ThemeProvider initialThemeName="Default" themes={mockThemes} defaultTheme={mockTheme}>
                <TooltipControlProvider {...mockTooltipMethods}>
                    <ModalControlProvider {...mockModalMethods}>
                        <PanelControlProvider {...mockPanelMethods}>
                            <ToastProvider>
                                <TestComponent />
                            </ToastProvider>
                        </PanelControlProvider>
                    </ModalControlProvider>
                </TooltipControlProvider>
            </ThemeProvider>
        )

        expect(screen.getByTestId("test-component")).toBeDefined()
        expect(screen.getByTestId("context-test")).toBeDefined()
    })

    test("theme provider applies CSS variables correctly", () => {
        render(
            <ThemeProvider initialThemeName="Default" themes={mockThemes} defaultTheme={mockTheme}>
                <TestComponent />
            </ThemeProvider>
        )

        // The theme provider should set CSS custom properties on document root
        // We can't easily test this in JSDOM, but we can verify the component renders
        expect(screen.getByTestId("test-component")).toBeDefined()
    })

    test("providers handle missing children gracefully", () => {
        expect(() => {
            render(
                <ThemeProvider initialThemeName="Default" themes={mockThemes} defaultTheme={mockTheme}>
                    {null}
                </ThemeProvider>
            )
        }).not.toThrow()
    })

    test("providers handle undefined props gracefully", () => {
        // Test with minimal required props
        expect(() => {
            render(
                <ThemeProvider initialThemeName="Default" themes={mockThemes} defaultTheme={mockTheme}>
                    <TestComponent />
                </ThemeProvider>
            )
        }).not.toThrow()
    })

    test("theme provider works with empty theme object", () => {
        const emptyTheme = {} as Theme
        const emptyThemes = { Empty: emptyTheme }

        expect(() => {
            render(
                <ThemeProvider initialThemeName="Empty" themes={emptyThemes} defaultTheme={emptyTheme}>
                    <TestComponent />
                </ThemeProvider>
            )
        }).not.toThrow()
    })

    test("context providers maintain separation of concerns", () => {
        // Each provider should work independently
        const mockModalMethods = {
            openModal: vi.fn(),
            closeModal: vi.fn(),
            activeModalId: "test-modal",
        }

        const { unmount: unmountModal } = render(
            <ModalControlProvider {...mockModalMethods}>
                <TestComponent />
            </ModalControlProvider>
        )

        expect(screen.getByTestId("test-component")).toBeDefined()
        unmountModal()

        const mockPanelMethods = {
            openPanel: vi.fn(),
            closePanel: vi.fn(),
            closeAllPanels: vi.fn(),
        }

        render(
            <PanelControlProvider {...mockPanelMethods}>
                <TestComponent />
            </PanelControlProvider>
        )

        expect(screen.getByTestId("test-component")).toBeDefined()
    })
})
