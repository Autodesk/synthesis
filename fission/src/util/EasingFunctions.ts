/**
 * Source: https://easings.net/#easeOutQuad
 *
 * @param n Input of the easing function [0, 1]
 * @returns n^2 - 2n + 2
 */
export function easeOutQuad(n: number): number {
    return 1 - (1 - n) * (1 - n)
}
