author: Synthesis Team
summary: Tutorial for navigating and using the configure assets panel.
id: ConfigureAssets
tags: Configuration, Assets, Options, Customization
categories: Configuration
environments: Synthesis
status: Draft
feedback link: https://github.com/Autodesk/synthesis/issues

# Configure Assets

## Abstract

The configure assets panels allows you to change the ways assemblies interact with each other and how they are simulated. In this panel, you can:
- add intakes and ejectors to robots
- configure scoring zones for fields
- adjust the speed of robots
- set the alliance / station of robots
- and more ...

You can open this panel from the MainHUD

<img src="img/configure/mainhud.png" alt="View of the 'Configure Assets' panel in the MainHUD" width="300">

## Robots

<img src="img/configure/robot-options.png" alt="Configuration options for Robots" width="300">

### Brain

The brain determines what controls the robot. There are currently two options, "Synthesis" and "WPILib".

### Move

Add a gizmo tool to your robot to orientate it.

### Intake

<img src="img/configure/intake.png" alt="Configuring Intake Panel" width="300">

Setup the intake zone for your robot.

### Ejector

<img src="img/configure/ejector.png" alt="Configuring Ejector Panel" width="300">

Setup the ejector position and direction for your robot.

### Configure Joints

Edit the joints on your robot and adjust the speed and force behind them. Debugging: if your robot moves very slowly, reconfigure your exporter settings or alternatively change the velocity of the wheel joints.

### Sequence Joints

Configure joints to work together. Helpful for multi-stage elevators.

### Controls (Synthesis Brain)

Change the controls of the input scheme currently in use, as well as switch which scheme is actively in use. Tutorials for configuring schemes are in the "Spawn Asset" tutorial.

### Simulation (WPILib Brain)

Modify the relation between your robot within Synthesis and your code.

## Fields

<img src="img/configure/field-options.png" alt="Configuration options for Field" width="300">

### Move

Add a gizmo tool to your field to orientate it.

### Scoring Zones

<img src="img/configure/scoring-zones.png" alt="Configure Scoring Zones Panel" width="300">

If a game piece enters a scoring zone, the team scores a point. Using the scoring zones panel, you can add, position, and delete these zones.

Enabling persistent points will require a game piece to stay in the zone for the point to count. If the game piece leaves, the points will be lost.

### Protected Zones

Configure the position of protected zones to issue penalties to a team if one of their robot enters the zone.

### Input

This works the same as the controls section for the robot. You can modify the controls for the schemes, as well as add and delete them.

## Video Walkthrough

Watch the video below to walk through configuring your assemblies.

[//]: # (TODO: Add Tutorial)
<video id=""></video>


## Need More Help?

If you need help with anything regarding Synthesis or it's related features please reach out through our
[discord sever](https://www.discord.gg/hHcF9AVgZA). It's the best way to get in contact with the community and our current developers.
