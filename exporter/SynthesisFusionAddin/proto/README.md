# Synthesis Export Format

This is a export format to deal with more complex end use cases like AR experiences or Robotics experiences.

This is experimental and should not be shipped with the final build at any point. It's more for my TechX stuff and AR.

### Building

In order to build and distribute with this you need to do a few things.

1. Enable protobuf in ` config.ini ` and ` config-default.ini ` by changing the section to ` protobuf=yes `
2. Get the files needed by ` mira/protos ` which can be found at (github)[https://github.com/HiceS/mirabuf]
3. (for release) Package with ` python Package.py mira ` (adds the files to the final output directory for install)

### Usage

If enabled and correctly loaded you should see an additional option in the mode dropdown during export which will allow you to configure the export for Protobuf.