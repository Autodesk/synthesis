/** @type {import("prettier").Options} */
const config = {
    trailingComma: "es5",
    tabWidth: 4,
    semi: false,
    singleQuote: false,
    quoteProps: "consistent",
    jsxSingleQuote: false,
    bracketSpacing: true,
    bracketSameLine: false,
    arrowParens: "avoid",
    printWidth: 120,
    overrides: [
        {
            files: "*.json",
            options: {
                tabWidth: 2,
            },
        },
    ],
    endOfLine: "auto",
}

export default config
