import { Box, ButtonBase, Stack, Typography } from "@mui/material"
import type React from "react"
import { MdChevronLeft, MdChevronRight } from "react-icons/md"
import type { TourStep } from "./tourSteps"

const CARD_WIDTH = 265
const ARROW_SIZE = 12

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
                        width: ARROW_SIZE,
                        height: ARROW_SIZE,
                        bgcolor: "topBar.main",
                        transform: "rotate(45deg)",
                        // The popper arrow modifier positions the arrow along the main axis
                        // (left for top/bottom edges, top for left/right); we pin it to the
                        // card edge it sits on so half the square pokes out toward the anchor.
                        [arrowEdge]: -ARROW_SIZE / 2,
                    }}
                />
            )}

            <Stack direction="row" alignItems="flex-start" justifyContent="space-between" gap={1}>
                <Typography sx={{ fontWeight: 700, fontSize: 14, lineHeight: 1.2 }}>{step.title}</Typography>
                <ButtonBase
                    onClick={onSkip}
                    sx={{
                        flexShrink: 0,
                        bgcolor: "surface.main",
                        color: "topBarText.main",
                        borderRadius: "4px",
                        px: 1,
                        py: 0.25,
                        fontSize: 11,
                        fontWeight: 700,
                        "&:hover": { opacity: 0.85 },
                    }}
                >
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

                {isLast ? (
                    <ButtonBase
                        onClick={onNext}
                        sx={{
                            bgcolor: "surface.main",
                            color: "topBarText.main",
                            borderRadius: "4px",
                            px: 1,
                            py: 0.25,
                            fontSize: 11,
                            fontWeight: 700,
                            "&:hover": { opacity: 0.85 },
                        }}
                    >
                        Done
                    </ButtonBase>
                ) : (
                    <ButtonBase
                        onClick={onNext}
                        aria-label="Next step"
                        sx={{
                            borderRadius: "50%",
                            p: 0.25,
                            color: "topBarText.main",
                            "&:hover": { bgcolor: "surface.main" },
                        }}
                    >
                        <MdChevronRight size={20} />
                    </ButtonBase>
                )}
            </Stack>
        </Box>
    )
}

export default TourCard
