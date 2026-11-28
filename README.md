# Teacups UI

A (hopefully) growing collection of small, narrow-focused, **reusable UI building blocks and helpers for .NET apps**, initially focusing on Avalonia and WPF.

Stuff that I (and many of us) wrote many times over and over again. And even if today it's matter of seconds for an agent to write things like this, I don't want to repeat my prompts every time - I'd rather build my own foundation for every future project, and let it evolve over time.

A sibling repository, focused on various non-UI functionality and helpers, can be found at [Teacups](https://github.com/okisioli/teacups).

## Packages

| Package | Purpose |
| --- | --- |
| [Okisioli.Teacups.UI.Core](src/Okisioli.Teacups.UI.Core) | Framework-neutral UI service contracts. |
| [Okisioli.Teacups.UI.Core.WPF](src/Okisioli.Teacups.UI.Core.WPF) | WPF implementations of the Core dialog and UI dispatcher services. |
| [Okisioli.Teacups.UI.Core.Avalonia](src/Okisioli.Teacups.UI.Core.Avalonia) | Avalonia implementations of the Core dialog and UI dispatcher services. |
| [Okisioli.Teacups.UI.Controls.Avalonia](src/Okisioli.Teacups.UI.Controls.Avalonia) | Avalonia controls and styles.<ul><li>Expandable drawer sheet for dialogs and bottom sheets</li><li>Three-state floating action button</li><li>Circular progress bar</li></ul> |
| [Okisioli.Teacups.UI.Behaviors.WPF](src/Okisioli.Teacups.UI.Behaviors.WPF) | WPF XAML interaction behaviors.<ul><li>Long press on any element</li><li>List item double-click commands</li><li>Text box submission and tag entry</li></ul> |
| [Okisioli.Teacups.UI.Fastr.WPF](src/Okisioli.Teacups.UI.Fastr.WPF) | Buffered item sources, incremental collection updates, and optional scroll resets for WPF item controls. |
| [Okisioli.Teacups.UI.Behaviors.Avalonia](src/Okisioli.Teacups.UI.Behaviors.Avalonia) | Avalonia XAML interaction behaviors.<ul><li>Long press on any element</li><li>List item double-tap commands</li><li>Text box submission and tag entry</li></ul> |

## Demos

- [WPF Controls](demo/Okisioli.Teacups.UI.Demo.WPF)
- [WPF Fastr](demo/Okisioli.Teacups.UI.Fastr.Demo.WPF) · Run the project on Windows with the .NET 10 SDK.
- [Avalonia](demo/Okisioli.Teacups.UI.Demo.Avalonia) · [View in browser](https://okisioli.github.io/teacups-ui/)

To run the Avalonia browser demo locally, install the .NET 10 SDK and run
`./start.ps1` in PowerShell. The script restores the required WebAssembly workload,
builds the demo, and prints the local URL. The [desktop host](demo/Okisioli.Teacups.UI.Demo.Avalonia.Desktop)
can be run from the solution.
