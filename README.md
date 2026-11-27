# Teacups UI

A (hopefully) growing collection of small, narrow-focused, **reusable UI building blocks and helpers for .NET apps**, initially focusing on Avalonia and WPF.

Stuff that I (and many of us) wrote many times over and over again. And even if today it's matter of seconds for an agent to write things like this, I don't want to repeat my prompts every time - I'd rather build my own foundation for every future project, and let it evolve over time.

A sibling repository, focused on various non-UI functionality and helpers, can be found at [Teacups](https://github.com/okisioli/teacups).

## Packages

| Package | Purpose |
| --- | --- |
| `Okisioli.Teacups.UI.Core` | Framework-neutral UI service contracts. |
| `Okisioli.Teacups.UI.Core.WPF` | WPF implementations of Core services. |
| `Okisioli.Teacups.UI.Core.Avalonia` | Avalonia implementations of Core services. |
| `Okisioli.Teacups.UI.Controls.Avalonia` | Avalonia controls and styles. |
| `Okisioli.Teacups.UI.Behaviors.WPF` | WPF interaction behaviors. |
| `Okisioli.Teacups.UI.Fastr.WPF` | Buffered item sources, incremental collection updates, and optional scroll resets for WPF item controls. |
| `Okisioli.Teacups.UI.Behaviors.Avalonia` | Avalonia interaction behaviors. |

## Features

- Dialog and UI dispatcher services for WPF and Avalonia.
- [Controls for Avalonia](src/Okisioli.Teacups.UI.Controls.Avalonia/README.md), including:
  - An expandable overlay to serve as a dialog or a "bottom sheet".
  - A an unique three-state floating action button.
  - A circular progress bar.
- [Fastr](src/Okisioli.Teacups.UI.Fastr.WPF/README.md) aims to speed up UI frameworks with tricks, workarounds, and hacks. It currently supports WPF.
- XAML interaction behaviors for both WPF and Avalonia, such as:
  - Long press on any element.
  - List item double-taplist item double-click commands.
  - Convenient text-box submission and tag entry.

## Demos

- [WPF Controls](demo/Okisioli.Teacups.UI.Demo.WPF)
- [WPF Fastr](demo/Okisioli.Teacups.UI.Fastr.Demo.WPF) · Run the project on Windows with the .NET 10 SDK.
- [Avalonia](demo/Okisioli.Teacups.UI.Demo.Avalonia) · [View in browser](https://okisioli.github.io/teacups-ui/)

To run the Avalonia browser demo locally, install the .NET 10 SDK and run
`./start.ps1` in PowerShell. The script restores the required WebAssembly workload,
builds the demo, and prints the local URL. The [desktop host](demo/Okisioli.Teacups.UI.Demo.Avalonia.Desktop)
can be run from the solution.
