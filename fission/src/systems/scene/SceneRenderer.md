# Scene Renderer Documentation

The Scene Renderer is our interface for rendering within the Canvas. This is primarily done via ThreeJS.

The SceneRenderer manages a series of visual ThreeJs objects such as the cameras, lighting managers, and the skybox.

The SceneRenderer also manages the components of this world, known as SceneObjects, the most common of which is the [MirabufSceneObject](../../mirabuf/MirabufSceneObject.ts), representing objects parsed from the [mirabuf file format](https://mirabuf.dev). Each MirabufSceneObject holds references to each of the Jolt bodies that make it up and are managed by the [Physics System](../physics/PhysicsSystem.md).
