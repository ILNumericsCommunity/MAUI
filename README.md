ILNumerics.Community.MAUI
==========

[![Nuget](https://img.shields.io/nuget/v/ILNumerics.Community.Avalonia?style=flat-square&logo=nuget&color=blue)](https://www.nuget.org/packages/ILNumerics.Community.Avalonia)

Integration package for ILNumerics (http://ilnumerics.net/) scene graphs
and plot cubes with .NET Multi-platform App UI (https://dotnet.microsoft.com/apps/maui) framework. ILNumerics.Community.MAUI provides a ILNumerics *Panel* implementation for .NET MAUI and various helper/convenience functions.

All community-provided Panel implementations (for Avalonia, MAUI, etc.) share a common IPanel interface, so that extensions (such as the PlotEditor) can be applied to all of them. Currently, only a software-renderer is available for non-Windows platforms, which generally provides a quite smooth rendering experience.

## How to use

Add the .NET MAUI panel to your user interface (in XAML or code):

```csharp
<maui:Panel Background="White" x:Name="ilPanel" />
```

Assign a scene to the panel to render it:

```csharp
ilPanel.Scene = new Scene
{
    new PlotCube(twoDMode: false)
    {
        new Surface(tosingle(B), colormap: Colormaps.Jet) { new Colorbar() }
    }
};
ilPanel.Scene.Configure();
```

### License

ILNumerics.Community.MAUI is licensed under the terms of the MIT license (<http://opensource.org/licenses/MIT>, see LICENSE.txt).
