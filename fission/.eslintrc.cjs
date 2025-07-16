module.exports = {
  root: true,
  env: {
    browser: true,
    es2020: true,
  },
  extends: [
    'eslint:recommended',
    'plugin:@typescript-eslint/recommended',
    'plugin:react-hooks/recommended',
  ],
  ignorePatterns: [
    'dist',
    '.eslintrc.cjs',
    'src/proto/mirabuf.*',
    'src/samples/*',
  ],
  parser: '@typescript-eslint/parser',
  plugins: ['react-refresh'],
  rules: {
    'react-refresh/only-export-components': [
      'warn',
      { allowConstantExport: true },
    ],
    '@typescript-eslint/no-unused-vars': [
      'warn',
      {
        argsIgnorePattern: '^_',
        varsIgnorePattern: '^_',
        caughtErrorsIgnorePattern: '^_',
      },
    ],
    '@typescript-eslint/naming-convention': [
      'warn',
      {
        selector: ['variable', 'parameter'],
        modifiers: ['unused'],
        format: ['camelCase'],
        leadingUnderscore: 'require',
        trailingUnderscore: 'forbid',
      },
      {
        selector: ['function', 'method', 'classMethod'],
        format: ['camelCase'],
        leadingUnderscore: 'allow',
        trailingUnderscore: 'forbid',
      },
      {
        selector: ['memberLike'],
        modifiers: ['private'],
        format: ['camelCase'],
        leadingUnderscore: 'require',
        trailingUnderscore: 'forbid',
      },
      {
        selector: ['variable'],
        modifiers: ['const'],
        format: ['PascalCase', 'camelCase', 'UPPER_CASE'],
      },
      {
        selector: ['typeProperty'],
        format: null,
      },
      {
        selector: ['classProperty'],
        modifiers: ['static', 'readonly'],
        format: ['UPPER_CASE'],
        leadingUnderscore: 'forbid',
        trailingUnderscore: 'forbid',
      },
      {
        selector: ['objectLiteralProperty'],
        modifiers: ['requiresQuotes'],
        format: null,
      },
      {
        selector: ['objectLiteralProperty'],
        format: ['camelCase', 'UPPER_CASE', 'snake_case', 'PascalCase'],
        leadingUnderscore: 'allow',
        trailingUnderscore: 'forbid',
      },
      {
        selector: ['property'],
        modifiers: ['requiresQuotes'],
        format: null,
      },
      {
        selector: ['typeLike'],
        format: ['PascalCase'],
        leadingUnderscore: 'forbid',
        trailingUnderscore: 'forbid',
      },
      {
        selector: ['import'],
        format: ['PascalCase', 'camelCase'],
        leadingUnderscore: 'forbid',
        trailingUnderscore: 'forbid',
      },
      {
        selector: ['class'],
        format: ['PascalCase'],
        leadingUnderscore: 'forbid',
        trailingUnderscore: 'forbid',
      },
      {
        selector: ['enumMember'],
        format: ['UPPER_CASE'],
        leadingUnderscore: 'forbid',
        trailingUnderscore: 'forbid',
      },
      {
        selector: ['default'],
        format: ['camelCase'],
        leadingUnderscore: 'allow',
        trailingUnderscore: 'forbid',
      },
    ],
  },
}
