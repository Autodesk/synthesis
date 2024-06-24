![Synthesis: An Autodesk Technology](/fission/res/branding/Synthesis-An-Autodesk-Technology-2023-lockup-Blk-OL-No-Year-stacked.svg#gh-light-mode-only)
![Synthesis: An Autodesk Technology](/fission/res/branding/Synthesis-An-Autodesk-Technology-2023-lockup-Wht-OL-No-Year-stacked.svg#gh-dark-mode-only)

<br/>

[![Fission - Unit Test](https://github.com/Autodesk/synthesis/actions/workflows/FissionUnitTest.yml/badge.svg?branch=master)](https://github.com/Autodesk/synthesis/actions/workflows/FissionUnitTest.yml)
[![Fission - Packaging](https://github.com/Autodesk/synthesis/actions/workflows/FissionPackage.yml/badge.svg?branch=master)](https://github.com/Autodesk/synthesis/actions/workflows/FissionPackage.yml)
[![Fission - Lint/Format](https://github.com/Autodesk/synthesis/actions/workflows/FissionESLintFormat.yml/badge.svg?branch=master)](https://github.com/Autodesk/synthesis/actions/workflows/FissionESLintFormat.yml)
[![Fusion - Format](https://github.com/Autodesk/synthesis/actions/workflows/BlackFormat.yml/badge.svg?branch=master)](https://github.com/Autodesk/synthesis/actions/workflows/BlackFormat.yml)

Synthesis is a robotics simulator designed by and for [FIRST®](https://www.firstinspires.org/) robotics students to help teams design, strategize, test and practice. Teams have the ability to import their own robots and fields using our [Fusion Exporter](/exporter/) or use the pre-made ones available within Synthesis.

For more information on the product itself or the team, visit [http://synthesis.autodesk.com](http://synthesis.autodesk.com/).

## Goals

Synthesis is built with a direct focus on the FIRST® community. Every single one of our developers is a FIRST® student. We've also made the project completely open source in order to better involve the community. This way contributors can help make Synthesis better or modify Synthesis to better suit their team’s needs.

Here are some of our primary goals for Synthesis:

- **Ease of Use**: It's important for us that Synthesis is out of the box ready for teams to use. We want to make sure that teams can get up and running with Synthesis as quickly as possible. To that end, Synthesis comes ready with a variety of robots and fields; in addition to the ability to export and import your own.
- **Testing Robot Designs**: Synthesis is designed to be a tool for teams to quickly test their robot designs in a semi-realistic environment. Are you a builder who wants to use some crazy virtual four-bar linkage and your team says it's a waste of time? Well now you can prove them wrong by testing it in Synthesis!
- **Exploring the Field Environment**: Every year on kickoff, for both FTC and FRC FIRST® competitions, Synthesis has the newest field available immediately. This allows teams to explore the field through a 3D model, drive a robot around, and begin to strategize for the upcoming season's game.
- **Driver Practice & Strategy**: Not getting enough driver practice or don't have a full field available to you? Synthesis has you covered with the ability to drive your robot around with a gamepad from a first-person view at the driver station; allowing you to get a feel for potential control scheme layouts and any line-of-sight challenges that may arise. This also allows the drive team and the programmers to communicate about what control layouts work best for each driver.

## Getting Started

If you are a FIRST robotics student who just wants to use Synthesis, you *don't* need this repo. Simply **install the latest release of Synthesis from [synthesis.autodesk.com/download](https://synthesis.autodesk.com/download.html)**.

[!IMPORTANT]
Moving to [synthesis.autodesk.com]

If you're a developer who wants to contribute to Synthesis, you're in the right place. Synthesis is comprised of 3 main components that can be developed separately. These components include:

- [Fission (Core Web App)](/fission/README.md)
- [Fusion Exporter (Fusion exporter to Mirabuf file format)](/exporter/SynthesisFusionAddin/README.md)
- [Installers](/installer/)

Follow the above links to the respective READMEs on how to build and run each component.

### Compatibility Notes

As Fusion is not supported on linux, the linux installer does not come with the Fusion Addin for exporting robots and fields.

## Contributing

This project welcomes community suggestions and contributions. Synthesis is nearly 100% open source and relies on the FIRST® community to help make it better. The [Synthesis Contribution Guide](/CONTRIBUTING.md) suggests ways in which you can get involved through development and non-development avenues.

Before you contribute to this repository, please first discuss the change you wish to make via a GitHub issue, email us ([frc@autodesk.com](mailto:frc@autodesk.com)), or reach out through our [community discord](https://www.discord.gg/hHcF9AVgZA). This way we can ensure that there is no overlap between outside contributors and internal development work.

When ready to contribute, fork the synthesis repository, make your changes, and submit a pull request. Be sure to fill out the template accordingly to make reviewing your work as smooth as possible. Feel free to check out our [contributing guidelines](/CONTRIBUTING.md) to learn more.

## Code Formatting And Style

All code is under a configured formatting utility. See each component for more details.

## Other Components

### Mirabuf

Mirabuf is a file format we use to store physical data from Fusion to load into the Synthesis simulator (Fission). This is a separate project that is a submodule of Synthesis. [See Mirabuf](https://github.com/HiceS/mirabuf/)

### Tutorials

Our source code for the tutorials featured on our [Tutorials Page](https://synthesis.autodesk.com/tutorials.html).

### Protocols

Additional protobuf files that we use in addition to Mirabuf. [See Protocols](/protocols/README.md)

## Tutorials

We have a variety of tutorials available to help you get started with Synthesis. These tutorials can be found on our [Tutorials Page](https://synthesis.autodesk.com/tutorials.html) on our website. Additionally, you can view these same tutorials as Markdown files in the [tutorials](/tutorials/) directory of this repository.

Updating our tutorials is a ongoing process. If you are at all interested in helping, checkout the [Synthesis Contribution Guide](/CONTRIBUTING.md) for more information on how to get started.

## Immersion Program

Annually, since 2014, Autodesk has sponsored the Synthesis Immersion Program for FIRST robotics students to develop Synthesis. The immersion program is a 10 week paid work experience at the Portland, Oregon Autodesk office from June 20th to August 25th. The immersion program focuses on not only developing Synthesis, but also allowing for opportunities to meet and collaborate with other Autodesk employees. For more information about the immersion program, visit our website at [synthesis.autodesk.com/internship](https://synthesis.autodesk.com/internship.html).

### Want To Be A Part Of The Team?

If you're a FIRST robotics student who wants to be a part of the Synthesis development team here is some basic information about applying.

Applicants must be:

- At least 16 years of age
- Been a member of a FIRST Robotics team for at least one full season

Applications open each year during the spring. For more information about applying, exceptions to these requirements or for more info about specific positions offered, please visit the [Synthesis Immersion Program](https://synthesis.autodesk.com/internship.html) website.

## Contact

If you have any questions about Synthesis or the Immersion Program, you can contact us through email ([frc@autodesk.com](mailto:frc@autodesk.com)). Additionally please reach out through our [community discord](https://www.discord.gg/hHcF9AVgZA). It's the best way to get in touch with not only the community, but Synthesis' current development team.

## License

Copyright (c) Autodesk

SPDX-License-Identifier: Apache-2.0
