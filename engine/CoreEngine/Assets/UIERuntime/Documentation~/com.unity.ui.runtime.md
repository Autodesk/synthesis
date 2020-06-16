# UIElements Runtime Support


This package enables UIElements to be used at runtime. It provides a few Components that can be added to GameObjects in order to display a UIElements panel in the game view and to send in-game events to the panel.

Currently, UIElements panels can only be displayed as an overlay to the game view (equivalent to Unity GUI *Screen Space - Overlay* mode). 

## How to use

To display a UI built with UI elements, create an empty GameObject, click on `Add Component` and add a `Panel Renderer`  component, a  `Panel Scaler` component (automatically added when you add a `Panel Renderer`) and a `UIElements/Event System` component.

### Panel Renderer

The `Panel Renderer` is used to display a panel in the game view. Set its `Uxml` field to a UXML asset and add stylesheets by first setting the `Stylesheets.Size` field to the number of stylesheets you want to use and then adding a stylesheet asset to each slot.

### Panel Scaler

This components is used to define how the size of the UI changes with the size of the screen. It plays a similar role as the [`CanvasScaler`](https://docs.unity3d.com/Manual/script-CanvasScaler.html) in Unity UI.

### Event System

The `Event System` reads event and input and forwards the information to UIElements event dispatcher. There are three event sources availaible:

- IMGUI Events: when this source is selected, the event system will forward events received from the operating system, similar to what is done in the editor. If your UI needs to process keyboard input, such as a text field, you need this event source.

- Input Events: when this source is selected, we infer events from the legacy Input module state. Currently, only touch and mouse events are generated from this source. This source is required if you need to support multi-touch events.

- Navigation Events: this source requires `Input Events` to also be selected. When this source is selected, we will send navigation events (`NavigationMoveEvent`, `NavigationSubmitEvent` and `NavigationCancelEvent`) infered from the legacy Input module state.

The remaining fields serve the same purpose as their counterparts in the [`StandaloneInputModule`](https://docs.unity3d.com/ScriptReference/EventSystems.StandaloneInputModule.html): they are used to specify the names of the axis and buttons as well as the input response speed.
