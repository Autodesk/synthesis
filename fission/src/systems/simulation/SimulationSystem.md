# Simulation System Documentation

The Simulation System articulates dynamic elements of the scene via the Physics System. At its core there are 3 main components:

#### Driver

Drivers are mostly write-only. They take in values to know how to articulate the physics objects and constraints.

#### Stimulus

Stimuli are mostly read-only. They read values from given physics objects and constraints.

#### Brain

Brains are the controllers of the mechanisms. They use a combination of Drivers and Stimuli to control a given mechanism.

The [Synthesis Brain](./synthesis_brain/SynthesisBrain.ts) exists for basic user control of the mechanisms, while the [WPILib Brain](./wpilib_brain/WPILibBrain.ts) facilitates controlling mechanisms over a websocket connection with a WPILib HALSim instance.
