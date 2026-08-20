Fission is primarily composed of a series of systems, each one (somewhat) independently managing an aspect of Fission’s functionality.

Each system besides the World System extend the WorldSystem abstract class, implementing the functions setup and destroy. When we discuss “Systems” in this document, we are referring to objects that extend WorldSystem in this way. There are many folders, files, and classes within the src/systems directory, many of which are simple used by Systems.

## World

`src/systems/World.ts`

The World System is a relatively simple system responsible for managing all other systems, by initializing them, and then updating them each time the World System is updated.

## Scene Renderer

`src/systems/sccene/SceneRenderer.ts`

The Scene Renderer is responsible for rendering the contents of the Fission simulation using ThreeJS. In some (limited) sense, it is the top-level system, as it owns every Scene Object.

### Scene Object

`src/systems/scene/SceneObject.ts`

Scene Objects are the primary physical and visual construct in Fission. They represent entire independently moving objects in the scene.

The most common kind of Scene Object is the Mirabuf Scene Object.

> ![NOTE]
> The infamous Game Piece Asset Support ticket aims to turn game pieces into Scene Objects, which would operate independently from their fields.

### Mirabuf Scene Object

`src/systems/mirabuf/MirabufSceneObject.ts`

Mirabuf Scene Objects are a subclass of Scene Object, instances of which represent specific objects that were imported via the MirabufParser, such as fields or robots.
