# Teacups UI

Teacups UI is a collection of small, reusable UI building blocks for Teacups applications. Shared contracts live in a framework-neutral package; WPF and Avalonia packages provide the platform-specific implementations, controls, and interaction behaviors.

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
- [Bottom sheet and circular progress bar](src/Okisioli.Teacups.UI.Controls.Avalonia/README.md) for Avalonia.
- Long-press commands, list item double-click commands, text box submission, and tagging for WPF and Avalonia.

## Demos

- [WPF](demo/Okisioli.Teacups.UI.Demo.WPF)
- [Fastr WPF](demo/Okisioli.Teacups.UI.Fastr.Demo.WPF) · Run the project on Windows with the .NET 10 SDK.
- [Avalonia](demo/Okisioli.Teacups.UI.Demo.Avalonia) · [View demo](https://teacups-libs.github.io/teacups-ui/)

To run the Avalonia browser demo locally, install the .NET 10 SDK and run
`./start.ps1` in PowerShell. The script restores the required WebAssembly workload,
builds the demo, and prints the local URL. The [desktop host](demo/Okisioli.Teacups.UI.Demo.Avalonia.Desktop)
can be run from the solution.

The [Avalonia demo workflow](.github/workflows/publish-demo.yml) publishes from
`main` when the repository's **Settings → Pages → Source** is set to **GitHub Actions**.
