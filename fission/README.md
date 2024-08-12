# Fission

Fission is Synthesis' web-based robotics simulator. This app is hosted [on our website](https://synthesis.github.com/fission/), in addition to a closed, in-development version [here](https://synthesis.autodesk.com/beta/).

## Setup & Building

### Requirements

1. NPM (v10.2.4 recommended)
   - Yarn, Bun, or any other package managers work just as well.
2. NodeJS (v20.10.0 recommended)
   - Needed for running the development server.

### Setup

You can either run the `init` command or run the following commands details in "Specific Steps":

```bash
npm i && npm init
```

<details>
<summary>Specific Steps</summary>

To install all dependencies:

```bash
npm i
```

For the asset pack that will be available in production, download the asset pack [here](https://synthesis.autodesk.com/Downloadables/assetpack.zip) and unzip it.
Make sure that the Downloadables directory is placed inside of the public directory like so:

```
/fission/public/Downloadables/
```

This can be accomplished with the `assetpack` npm script:

```bash
npm run assetpack
```

We use [Playwright](https://playwright.dev/) for testing consistency. The package is installed with the rest of the dependencies; however, be sure to install the playwright browsers with the following command:

```bash
npx playwright install
```
or
```bash
npm run playwright:install
```

</details>

### Environment Configuration

In `vite.config.ts` you'll find a number of constants that can be used to tune the project to match your development environment.

## Running & Testing

### Development Server

You can use the `dev` command to run the development server. This will open a server on port 3000 and open your default browser at the hosted endpoint.

```bash
npm run dev
```

### Unit Testing

We use a combination of Vitest and Playwright for running our unit tests. A number of the unit tests rely on the asset pack data and may timeout due to download speeds. By default, the unit test command uses a chromium browser.

```bash
npm run test
```

## Packaging

We have two commands, one for compiling dev for attachment to the in-development endpoint, and another for the release endpoint:

Release:
```bash
npm run build:prod
```

In-development:
```bash
npm run build:dev
```

You can alternatively run the default build command for your own hosting:

```bash
npm run build
```

## Core Systems

These core systems make up the bulk of the vital technologies to make Synthesis work. The idea is that these systems will serve as a
jumping off point for anything requiring real-time simulation.

### World

The world serves as a hub for all of the core systems. It is a static class that handles system update execution order and lifetime.

### Scene Renderer

The Scene Renderer is our interface with rendering within the Canvas. This is primarily done via ThreeJS, however can be extended later on.

### Physics System

This Physics System is our interface with Jolt, ensuring objects are properly handled and provides utility functions that are more custom fit to our purposes.

[Jolt Physics Architecture](https://jrouwe.github.io/JoltPhysics/)

### Input System

### UI System

## Additional Systems

These systems will extend off of the core systems to build out features in Synthesis.

### Simulation System

The Simulation System articulates dynamic elements of the scene via the Physics System. At it's core there are 3 main components:

#### Driver

Drivers are mostly write-only. They take in values to know how to articulate the physics objects and contraints.

#### Stimulus

Stimuli are mostly read-only. They read values from given physics objects and constraints.

#### Brain

Brains are the controllers of the mechanisms. They use a combination of Drivers and Stimuli to control a given mechanism.

For basic user control of the mechanisms, we'll have a Synthesis Brain. By the end of Summer 2024, I hope to have an additional brain, the WPIBrain for facilitating WPILib code control over the mechanisms inside of Synthesis.

## NPM Scripts

| Script               | Description                                                                                                             |
| -------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| `init`               | Runs the initialization commands to install all dependencies, assets, and unit testing browsers.                        |
| `dev`                | Starts the development server used for testing. Supports hotloading (though finicky with WASM module loading).          |
| `test`               | Runs the unit tests via vitest.                                                                                         |
| `build`              | Builds the project into it's packaged form. Uses root base path.                                                        |
| `build:prod`         | Builds the project into it's packaged form. Uses the `/fission/` base path.                                             |
| `preview`            | Runs the built project for preview locally before deploying.                                                            |
| `lint`               | Runs eslint on the project.                                                                                             |
| `lint:fix`           | Attempts to fix issues found with eslint.                                                                               |
| `prettier`           | Runs prettier on the project as a check.                                                                                |
| `prettier:fix`       | Runs prettier on the project to fix any issues with formating. **DO NOT USE**, I don't like the current format it uses. |
| `format`             | Runs `prettier:fix` and `lint:fix`. **Do not use** for the same reasons as `prettier:fix`.                              |
| `assetpack`          | Downloads the assetpack and unzips/installs it in the correct location.                                                 |
| `playwright:install` | Downloads the playwright browsers.                                                                                      |