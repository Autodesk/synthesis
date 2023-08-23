module.exports = {
    root: true,
    env: { browser: true, es2020: true },
    extends: [
        'eslint:recommended',
        'plugin:@typescript-eslint/recommended',
        'plugin:react-hooks/recommended',
        'eslint-config-prettier'
    ],
    ignorePatterns: ['dist', '.eslintrc.cjs'],
    parser: '@typescript-eslint/parser',
    plugins: ['react-refresh'],
    rules: {
        'react-refresh/only-export-components': [
            'off',
            { allowConstantExport: true },
        ],
        '@typescript-eslint/no-explicit-any': ['off']
    },
    settings: {
        'import/resolver': {
            node: {
                extensions: ['.js', '.ts', '.jsx', '.tsx', '.d.ts'],
            },
            alias: {
                extensions: ['.js', '.ts', '.jsx', '.tsx', '.d.ts'],
                map: [
                    ["@components/*", "./components/*"],
                    ["@modals/*", "./modals/*"],
                    ["@panels/*", "./panels/*"],
                    ["@assets/*", "./assets/*"]
                ]
            }
        }
    }
}
