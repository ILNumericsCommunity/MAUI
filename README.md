# ILNumerics.Community.MAUI

[![Nuget](https://img.shields.io/nuget/v/ILNumerics.Community.MAUI?style=flat-square&logo=nuget&color=blue)](https://www.nuget.org/packages/ILNumerics.Community.MAUI)

Integration package for ILNumerics (http://ilnumerics.net/) scene graphs and plot cubes with .NET Multi-platform App UI (https://dotnet.microsoft.com/apps/maui) framework. `ILNumerics.Community.MAUI` provides an ILNumerics `Panel` implementation for .NET MAUI and a set of helper / convenience functions to make embedding ILNumerics scenes into MAUI apps straightforward.

This package makes it easy to host ILNumerics scene graphs and 2D/3D plot cubes inside MAUI applications. The panel acts as a bridge between MAUI's UI system and ILNumerics rendering, allowing you to build interactive visualizations that run on desktop and mobile platforms supported by .NET MAUI.

> Note: This package currently uses a software renderer on all platforms. It generally provides a smooth rendering experience for moderate data sizes, but performance may vary per platform and scene complexity.

## Compatibility

- .NET: targets `.NET 9`.
- ILNumerics: `ILNumerics 7.3+`.
- .NET MAUI: compatible with the .NET MAUI APIs in .NET 9.

> Note: Desktop platforms (Windows, Linux, macOS) are working well. There are currently some outstanding issues on mobile platforms; please refer to the issue tracker for details and status updates.

## Usage

Add the ILNumerics panel to your user interface (in XAML or in code). The example below shows a simple XAML usage; adjust XML namespaces as appropriate for your project:

```xml
<maui:Panel Background="White" x:Name="ilPanel" />
```

Assign a scene to the panel to render it. A minimal example in C#:

```csharp
// Create a Scene containing a PlotCube and a Surface. Replace 'B' with your data array.
ilPanel.Scene = new Scene
{
    new PlotCube(twoDMode: false)
    {
        new Surface(tosingle(B), colormap: Colormaps.Jet) { new Colorbar() }
    }
};

// Call Configure so ILNumerics computes bounds and internal state required for rendering.
ilPanel.Scene.Configure();
```

Notes:
- Call `Configure()` on the scene after setup so ILNumerics computes bounds and other internal state required for rendering.
- Assign a new `Scene` or modify an existing one to update what is rendered.
- Use the provided helper functions and the `Panel` events to integrate with your application input and lifecycle.

## Keyboard modifier keys (`IModifierKeyService`)

For interactive scene manipulation (for example, orbit/pan/zoom with modifier keys), the project provides an `IModifierKeyService` abstraction and platform-specific implementations under `MAUI/Platforms/*`.

Short integration steps:
- Register or resolve `IModifierKeyService` from your MAUI app's DI container (or construct the platform implementation directly).
- Use the service from input handlers to query current modifier state when processing touch/keyboard events.

Example (in `MauiProgram.cs`):

```csharp
// Register the shared service; platform implementations are provided in the project
builder.Services.AddSingleton<IModifierKeyService, ModifierKeyService>();
```

See `Demo/MAUIDemo` for a concrete example of wiring up `IModifierKeyService` and handling modifier-driven interactions in the demo's input handlers.

## Examples and demos

This repository includes demo projects under the `Demo/` folder showcasing usage across supported MAUI targets. The `MAUIDemo` project demonstrates basic integration and typical usage patterns—run it to see concrete examples and to experiment with different scenes and rendering configurations.

### License

ILNumerics.Community.MAUI is licensed under the terms of the MIT license (<http://opensource.org/licenses/MIT>, see LICENSE.txt).