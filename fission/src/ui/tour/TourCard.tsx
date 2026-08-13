import { Box, ButtonBase, Stack, Typography } from "@mui/material"
import type React from "react"
import { MdChevronLeft, MdChevronRight } from "react-icons/md"
import type { TourStep } from "./tourSteps"

const CARD_WIDTH = 265
const PILL_SX = {
    bgcolor: "surface.main",
    color: "topBarText.main",
    borderRadius: "4px",
    px: 1,
    py: 0.25,
    fontSize: 11,
    fontWeight: 700,
    "&:hover": { opacity: 0.85 },
} as const
/** Length of the pointer's base, running along the card edge. */
const ARROW_BASE = 16
/** How far the pointer's tip protrudes past the card edge toward the anchor. */
const ARROW_HEIGHT = 8

/**
 * Per-edge geometry for the triangular pointer.
 *
 * We carve a real triangle with `clipPath` rather than rotating a square: the MUI Popper
 * `arrow` modifier positions this element with an inline `transform: translate(...)`, which
 * would clobber any `transform: rotate(...)` we set (inline style beats the emotion class),
 * leaving a square poking out. `clipPath` never touches `transform`, so the two never fight.
 *
 * For each edge the base sits flush against the card and the tip points toward the anchor;
 * `width`/`height` size the bounding box (base spans the edge, height is the protrusion) and
 * the negative `offset` pulls the box fully outside that edge.
 */
const ARROW_GEOMETRY: Record<
    "top" | "bottom" | "left" | "right",
    { clipPath: string; width: number; height: number; offset: Record<string, number> }
> = {
    top: {
        clipPath: "polygon(50% 0%, 0% 100%, 100% 100%)",
        width: ARROW_BASE,
        height: ARROW_HEIGHT,
        offset: { top: -ARROW_HEIGHT },
    },
    bottom: {
        clipPath: "polygon(50% 100%, 0% 0%, 100% 0%)",
        width: ARROW_BASE,
        height: ARROW_HEIGHT,
        offset: { bottom: -ARROW_HEIGHT },
    },
    left: {
        clipPath: "polygon(0% 50%, 100% 0%, 100% 100%)",
        width: ARROW_HEIGHT,
        height: ARROW_BASE,
        offset: { left: -ARROW_HEIGHT },
    },
    right: {
        clipPath: "polygon(100% 50%, 0% 0%, 0% 100%)",
        width: ARROW_HEIGHT,
        height: ARROW_BASE,
        offset: { right: -ARROW_HEIGHT },
    },
}

interface TourCardProps {
    step: TourStep
    stepIndex: number
    total: number
    onNext: () => void
    onPrev: () => void
    onSkip: () => void
    /** Popper arrow element ref. Omit for a centered (anchorless) card. */
    setArrowRef?: (el: HTMLElement | null) => void
    /** Which card edge the pointer sits on. Omit to hide the pointer. */
    arrowEdge?: "top" | "bottom" | "left" | "right"
    nextDisabled?: boolean
}

/**
 * The onboarding callout card (Figma 357-18): title, body, Skip pill, a "n of N"
 * counter and `<` / `>` navigation, plus an optional triangular pointer for anchored
 * steps. Purely presentational - all state lives in the TourProvider.
 */
const TourCard: React.FC<TourCardProps> = ({
    step,
    stepIndex,
    total,
    onNext,
    onPrev,
    onSkip,
    setArrowRef,
    arrowEdge,
    nextDisabled = false,
}) => {
    const isFirst = stepIndex === 0
    const isLast = stepIndex === total - 1

    return (
        <Box
            sx={{
                position: "relative",
                width: CARD_WIDTH,
                bgcolor: "topBar.main",
                color: "topBarText.main",
                borderRadius: "9px",
                p: 2,
                boxShadow: "0 6px 20px rgba(0,0,0,0.35)",
                pointerEvents: "auto",
                userSelect: "none",
            }}
        >
            {arrowEdge && (
                <Box
                    ref={setArrowRef}
                    sx={{
                        position: "absolute",
                        bgcolor: "topBar.main",
                        // The popper arrow modifier centers this element along the card edge (via an
                        // inline transform); the clip-path + offset below draw the triangle protruding
                        // from that edge. See ARROW_GEOMETRY for why we clip rather than rotate.
                        clipPath: ARROW_GEOMETRY[arrowEdge].clipPath,
                        width: ARROW_GEOMETRY[arrowEdge].width,
                        height: ARROW_GEOMETRY[arrowEdge].height,
                        ...ARROW_GEOMETRY[arrowEdge].offset,
                    }}
                />
            )}

            <Stack direction="row" alignItems="flex-start" justifyContent="space-between" gap={1}>
                <Typography sx={{ fontWeight: 700, fontSize: 14, lineHeight: 1.2 }}>{step.title}</Typography>
                <ButtonBase onClick={onSkip} sx={{ ...PILL_SX, flexShrink: 0 }}>
                    Skip
                </ButtonBase>
            </Stack>

            <Typography sx={{ mt: 1, fontSize: 12.5, lineHeight: 1.35, opacity: 0.9 }}>{step.body}</Typography>

            <Stack direction="row" alignItems="center" justifyContent="space-between" sx={{ mt: 1.5 }}>
                <ButtonBase
                    onClick={onPrev}
                    disabled={isFirst}
                    aria-label="Previous step"
                    sx={{
                        borderRadius: "50%",
                        p: 0.25,
                        color: "topBarText.main",
                        opacity: isFirst ? 0.25 : 1,
                        "&:hover": { bgcolor: "surface.main" },
                    }}
                >
                    <MdChevronLeft size={20} />
                </ButtonBase>

                <Typography sx={{ fontSize: 11, fontWeight: 700, opacity: 0.9 }}>
                    {stepIndex + 1} of {total}
                </Typography>

                <ButtonBase
                    onClick={onNext}
                    aria-disabled={nextDisabled}
                    aria-label="Next step"
                    sx={{
                        ...PILL_SX,
                        bgcolor: "primary.main",
                        color: "primary.contrastText",
                        gap: 0.25,
                        pr: isLast ? 1 : 0.5,
                        ...(nextDisabled && { opacity: 0.4, "&:hover": { opacity: 0.4 } }),
                    }}
                >
                    {isLast ? "Done" : "Next"}
                    {!isLast && <MdChevronRight size={14} />}
                </ButtonBase>
            </Stack>
        </Box>
    )
}

export default TourCard
