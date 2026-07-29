![Synthesis: An Autodesk Technology](/fission/res/branding/Synthesis-An-Autodesk-Technology-2023-lockup-Blk-OL-No-Year-stacked.svg#gh-light-mode-only)
![Synthesis: An Autodesk Technology](/fission/res/branding/Synthesis-An-Autodesk-Technology-2023-lockup-Wht-OL-No-Year-stacked.svg#gh-dark-mode-only)

<br/>

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Autodesk/synthesis/FissionUnitTest.yml?branch=prod&style=for-the-badge&logoSize=auto&label=Fission%20Unit%20Tests&link=https%3A%2F%2Fgithub.com%2FAutodesk%2Fsynthesis%2Factions%2Fworkflows%2FFissionUnitTest.yml)
![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Autodesk/synthesis/FissionPackage.yml?branch=prod&style=for-the-badge&logoSize=auto&label=Fission%20-%20Packaging&link=https%3A%2F%2Fgithub.com%2FAutodesk%2Fsynthesis%2Factions%2Fworkflows%2FFissionPackage.yml)
![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Autodesk/synthesis/FissionBiome.yml?branch=prod&style=for-the-badge&logoSize=auto&label=Fission%20Lint%2FFormat&link=https%3A%2F%2Fgithub.com%2FAutodesk%2Fsynthesis%2Factions%2Fworkflows%2FFissionBiome.yml)
![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Autodesk/synthesis/BlackFormat.yml?branch=prod&style=for-the-badge&logoSize=auto&label=Fusion%20Exporter%20Format&link=https%3A%2F%2Fgithub.com%2FAutodesk%2Fsynthesis%2Factions%2Fworkflows%2FBlackFormat.yml)
![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Autodesk/synthesis/FusionWebUI.yml?branch=prod&style=for-the-badge&logoSize=auto&label=Fusion%20Exporter%20WebUI%20Build&link=https%3A%2F%2Fgithub.com%2FAutodesk%2Fsynthesis%2Factions%2Fworkflows%2FFusionWebUI.yml)

Synthesis is a robotics simulator designed by and for [FIRST®](https://www.firstinspires.org/) robotics students to help teams design, strategize, test and practice. Teams have the ability to import their own robots and fields using our [Fusion Exporter](/exporter/) or use the pre-made ones available within Synthesis.

For more information on the product itself or the team, visit [http://synthesis.autodesk.com](http://synthesis.autodesk.com/).

## Goals

Synthesis is built with a direct focus on the FIRST® community. Every single one of our developers is or was a FIRST® student. We've also made the project completely open source in order to better involve the community. This way contributors can help improve Synthesis broadly or adapt it to their team’s needs.

Here are some of our primary goals for Synthesis:

- **Ease of Use**: It's important for us that Synthesis is out of the box ready for teams to use. We want to make sure that teams can get up and running with Synthesis as quickly as possible. To that end, Synthesis comes ready with a variety of robots and fields in addition to the ability to export and import your own.
- **Testing Robot Designs**: Synthesis is designed to be a tool for teams to quickly test their robot designs in a semi-realistic environment. Are you a builder who wants to use some crazy virtual four-bar linkage and your team says it's a waste of time? Well now you can prove them wrong by testing it in Synthesis!
- **Exploring the Field Environment**: Every year on kickoff, for both FTC and FRC FIRST® competitions, Synthesis has the newest field available immediately. This allows teams to explore the field through a 3D model, drive a robot around, and begin to strategize for the upcoming season's game.
- **Driver Practice & Strategy**: Not getting enough driver practice or don't have a full field available to you? Synthesis has you covered with the ability to play full simulated matches, controlling your robot with a gamepad from a first-person view at the driver station. This allows you to get a feel for potential control scheme layouts and any line-of-sight challenges that may arise. This also allows the drive team and the programmers to communicate about what control layouts work best for each driver.

## Getting Started

If you are a FIRST robotics student who just wants to use Synthesis, you _don't_ need this repo. Simply go follow [this link](https://synthesis.autodesk.com/fission) to the simulator and start spawning in robots!

If you're a developer who wants to contribute to Synthesis, you're in the right place. Synthesis is comprised of 2 main components that can be developed separately:

- [Fission (Core Web App)](/fission/README.md)
- [Fusion Exporter (Fusion exporter to Mirabuf file format)](/exporter/SynthesisFusionAddin/README.md)
<!-- - [Fusion Exporter Installer](/installer/) -->

Follow the above links to the respective READMEs on how to build, run, and test each component.

> [!NOTE]
> As Fusion is not officially supported on Linux, we do not provide an installer for the Fusion Exporter on Linux.

## Contributing

See [CONTRIBUTING.md](/CONTRIBUTING.md) for information on how you can help build synthesis

## Other Components

### [Mirabuf](https://github.com/HiceS/mirabuf/)

Mirabuf is a file format we use to store physical data from Fusion to load into the Synthesis simulator (Fission). This is a separate project that is a submodule of Synthesis.

### [Jolt Physics](https://github.com/HunterBarclay/JoltPhysics.js)

Jolt is the core physics engine for our web-based simulator.

### Protocols

Additional protobuf files that we use in addition to Mirabuf. [See Protocols](/protocols/README.md)

## [Tutorials](https://synthesis.autodesk.com/tutorials.html)

We have a variety of tutorials available to help you get started with Synthesis. Additionally, you can view these same tutorials as Markdown files in the [tutorials](/tutorials/) directory of this repository.

Updating our tutorials is an ongoing process. If you are at all interested in helping, check out the [Synthesis Contribution Guide](/CONTRIBUTING.md) for more information on how to get started.

## [Immersion Program](https://synthesis.autodesk.com/internship.html)

Annually, since 2014, Autodesk has sponsored the Synthesis Immersion Program for FIRST robotics students to develop Synthesis. The immersion program is a 10 week paid work experience at the Portland, Oregon Autodesk office from June 16th to August 22nd. The immersion program focuses on not only developing Synthesis, but also allowing for opportunities to meet and collaborate with other Autodesk employees.

### Want To Be A Part Of The Team?

If you're a FIRST robotics student who wants to be a part of the Synthesis development team here is some basic information about applying.

Applicants must:

- Be at least 16 years of age (at the start of the internship)
- Have been a member of a FIRST Robotics team for at least one full season

Applications open each year during the spring. For more information about applying, exceptions to these requirements or for more info about specific positions offered, please visit [Synthesis Immersion Program](https://synthesis.autodesk.com/internship.html).

## Contact

If you have any questions about Synthesis or the Immersion Program, you can contact us through email ([frc@autodesk.com](mailto:frc@autodesk.com)). Additionally, please reach out through our [community Discord](https://www.discord.gg/hHcF9AVgZA). It's the best way to get in touch with not only the community, but Synthesis' current development team.

## [License](/LICENSE.txt)

Copyright (c) Autodesk

Except for the assets and files identified in [MARKS-LICENSE.md](/MARKS-LICENSE.md), this repository is licensed under the Apache License, Version 2.0.

SPDX-License-Identifier: Apache-2.0
