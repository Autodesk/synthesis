# Fission

Fission is Synthesis' web-based robotics simulator. This app is hosted [on our website](https://synthesis.github.com/fission/), in addition to a closed [beta version](https://synthesis.autodesk.com/beta/).

## Setup & Building

### Requirements

1. Bun (v1.2.20 recommended)
   - Yarn, NPM, or any other package managers work just as well.
2. Node.js (v20.10.0 recommended)
   - Needed for running the development server.

### Setup

You can either run the `init` command or run the following commands detailed in the "Specific Steps" section below:

```bash
bun i && bun run init
```

<details>
<summary>Specific Steps</summary>

To install all dependencies:

```bash
bun i
```

[Download the production asset pack](https://synthesis.autodesk.com/Downloadables/assetpack.zip), then unzip it.
Make sure that the `Downloadables` directory is placed inside the public directory like so:

```
/fission/public/Downloadables/
```

Alternatively for development, you can download and install the asset pack for whatever branch you're operating on with [Git LFS]. This can be accomplished with the `assetpack` script:

```bash
bun run assetpack
```

We use [Playwright](https://playwright.dev/) for testing with simulated browsers. The package is installed with the rest of the dependencies; however, be sure to install the playwright browsers with the following command:

```bash
bunx playwright install
```

or

```bash
bun run playwright:install
```

</details>

### Environment Configuration

In `vite.config.ts` you'll find a number of constants that can be used to match Synthesis to your development environment.

## Running & Testing

### Development Server

You can use the `dev` command to run the development server. This will open a server on port 3000 and open your default browser at the hosted endpoint.

```bash
bun run dev
```

### Unit Testing

We use a combination of Vitest and Playwright for running our unit tests. A number of the unit tests rely on the asset pack data and may time out due to download speeds. By default, the unit test command uses a Chromium browser.

```bash
bun run test
```

## Packaging

### Web Packaging

We have two packaging commands: one for compiling dev for attachment to the in-development endpoint, and another for the release endpoint.

Release:

```bash
bun run build:prod
```

In-development:

```bash
bun run build:dev
```

You can alternatively run the default build command for your own hosting:

```bash
bun run build
```

### Electron Packaging

We also give you the option to package Synthesis with electron. This will not give a performance boost, but it will allow Synthesis to work offline (make sure to also launch the app and download all the robot/field files you want to use).

To package the app run:

```bash
bun run electron:publish
```

The packaged app will be located in the `/fission/out` directory.

## Core Systems

These core systems make up the lion's share of the fission source code. Each system manages a different aspect of the simulated world

- [World](/fission/src/systems/World.md)
- [Scene Renderer](/fission/src/systems/scene/SceneRenderer.md)
- [Physics System](/fission/src/systems/physics/PhysicsSystem.md)
- [Input System](/fission/src/systems/input/InputSystem.md)
- [Simulation System](/fission/src/systems/simulation/SimulationSystem.md)

## Package Scripts

| Script               | Description                                                                                                                                     |
| -------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| `init`               | Runs the initialization commands to install all dependencies, assets, and unit testing browsers.                                                |
| `host`               | Starts the development server used for testing and exposes it to the network. Supports hot-reloading (though finicky with WASM module loading). |
| `dev`                | Starts the development server used for testing. Supports hot-reloading (though finicky with WASM module loading).                               |
| `test`               | Runs the unit tests via Vitest.                                                                                                                 |
| `build`              | Builds the project into its packaged form. Uses the root base path.                                                                             |
| `build:prod`         | Builds the project into its packaged form. Uses the `/fission/` base path.                                                                      |
| `build:dev`          | Builds the project into its packaged form. Uses the `/fission-closed/` base path.                                                               |
| `preview`            | Runs the built project for preview locally before deploying.                                                                                    |
| `lint`               | Runs the Biome linter without applying fixes.                                                                                                   |
| `lint:fix`           | Runs the Biome linter and applies fixes.                                                                                                        |
| `fmt`                | Runs the Biome formatter without applying fixes.                                                                                                |
| `fmt:fix`            | Runs the Biome formatter and applies fixes.                                                                                                     |
| `style`              | Runs the `lint` and `fmt` commands.                                                                                                             |
| `style:fix`          | Runs the `lint:fix` and `fmt:fix` commands.                                                                                                     |
| `assetpack`          | Downloads the assetpack and unzips/installs it in the correct location.                                                                         |
| `assetpack:update`   | Downloads the assetpack and unzips/installs it in the correct location, replacing the old directory if it exists.                               |
| `playwright:install` | Downloads the Playwright browsers.                                                                                                              |
| `electron:make`      | Builds Synthesis as an electron application.                                                                                                    |
| `electron:start`     | Starts Synthesis as an electron application.                                                                                                    |
| `electron:package`   | Packages Synthesis as an electron application.                                                                                                  |
| `electron:publish`   | Publishes Synthesis as an electron application.                                                                                                 |
