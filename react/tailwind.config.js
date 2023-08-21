/** @type {import('tailwindcss').Config} */
export default {
    content: [
        "./index.html",
        "./src/**/*.{js,jsx,ts,tsx}",
    ],
    theme: {
        extend: {
            colors: {
                'interactive-element-solid': 'var(--interactive-element-solid)',
                'interactive-element-left': 'var(--interactive-element-left)',
                'interactive-element-right': 'var(--interactive-element-right)',
                'background': 'var(--background)',
                'background-secondary': 'var(--background-secondary)',
                'interactive-background': 'var(--interactive-background)',
                'background-hud': 'var(--background-hud)',
                'interactive-hover': 'var(--interactive-hover)',
                'interactive-select': 'var(--interactive-select)',
                'main-text': 'var(--main-text)',
                'scrollbar': 'var(--scrollbar)',
                'accept-button': 'var(--accept-button)',
                'cancel-button': 'var(--cancel-button)',
                'interactive-element-text': 'var(--interactive-element-text)',
                'icon': 'var(--icon)',
                'main-hud-icon': 'var(--main-hud-icon)',
                'main-hud-close-icon': 'var(--main-hud-close-icon)',
                'highlight-hover': 'var(--highlight-hover)',
                'highlight-select': 'var(--highlight-select)',
                'skybox-top': 'var(--skybox-top)',
                'skybox-bottom': 'var(--skybox-bottom)',
                'floor-grid': 'var(--floor-grid)',
                'accept-cancel-button-text': 'var(--accept-cancel-button-text)',
                'match-red-alliance': 'var(--match-red-alliance)',
                'match-blue-alliance': 'var(--match-blue-alliance)',
            }
        },
    },
    plugins: [],
}

