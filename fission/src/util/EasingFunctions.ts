/**
 * Source: https://easings.net/#easeOutQuad
 *
 * @param n Input of the easing function [0, 1]
 * @returns -(n^2) + 2n
 */
export function easeOutQuad(n: number): number {
    if (n < 0 || n > 1) console.error(`easeOutQuad: input ${n} is outside the expected range [0, 1]`)
    return 1 - (1 - n) * (1 - n)
}
