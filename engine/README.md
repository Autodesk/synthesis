# Synthesis Simulator Engine

The simulator engine is what the exported fields and robots are loaded into. In real-time, it simulates a real world physics environment for robots to interact with fields or other robots.

## Unity 5

The unity 5 is the current synthesis engine. It uses Unity 5, but instead of the built-in PhysX, it uses a Bullet Physics plugin for the physics engine. The team moved to this solution primarily because the PhysX libraries weren't capable of accurate real time physics simulations while Bullet Physics provides more accurate and efficient physics.
