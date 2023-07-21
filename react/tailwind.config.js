/** @type {import('tailwindcss').Config} */
export default {
    content: [
        "./index.html",
        "./src/**/*.{js,jsx,ts,tsx}",
    ],
    theme: {
        extend: {
            animation: {
                'rotate-half': 'rotate-half 0.1s ease-in-out'
            },
            keyframes: {
                'rotate-half': {
                    '0%': { transform: 'rotate(0deg)' },
                    '100%': { transform: 'rotate(180deg)' }
                }
            }
        },
    },
    plugins: [],
}

