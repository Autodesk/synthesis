# Simulation System Documentation

The Simulation System articulates dynamic elements of the scene via the Physics System. At its core there are 3 main components:

#### Driver

Drivers are mostly write-only. They take in values to know how to articulate the physics objects and constraints.

#### Stimulus

Stimuli are mostly read-only. They read values from given physics objects and constraints.

#### Brain

Brains are the controllers of the mechanisms. They use a combination of Drivers and Stimuli to control a given mechanism.

For basic user control of the mechanisms, we'll have a Synthesis Brain. We hope to have an additional brain by the end of Summer 2024: the WPIBrain for facilitating WPILib code control over the mechanisms inside of Synthesis.
