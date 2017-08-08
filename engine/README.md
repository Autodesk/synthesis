# Synthesis Simulator Engine

The simulator engine is what the exported fields and robots are loaded into. In real-time, it simulates a real world physics environment for robots to interact with fields or other robots.

## Unity 4

The unity 4 subcomponent was the original synthesis engine. This used Unity 4 along with its built-in PhysX physics libraries to provide a simulation environment. However, due to limitations with both Unity 4 and PhysX, we looked into other solutions. This version is no longer being developed.

## Unity 5

The unity 5 subcomponent is the current synthesis engine. It uses Unity 5, but instead of the built-in PhysX, it uses a Bullet Physics plugin for the physics engine. The team moved to this solution primarily because the PhysX libraries weren't capable of accurate real time physics simulations while Bullet Physics provides more accurate and efficient physics. Modify this project folder if you wish to make changes to the current synthesis engine.

## Engine_Research

This was another solution to the PhysX problem that the team looked into. It was using OpenTK for rendering and Bullet Physics for physics to provide a more versatile and modifiable simulation engine. However, this proved to be too time consuming and the Unity 5 + Bullet Physics solution seemed a better option. This is no longer being worked on.