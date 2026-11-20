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
| `Okisioli.Teacups.UI.Behaviors.Avalonia` | Avalonia interaction behaviors. |

## Features

- Dialog and UI dispatcher services for WPF and Avalonia.
- [Bottom sheet and circular progress bar](src/Okisioli.Teacups.UI.Controls.Avalonia/README.md) for Avalonia.
- Long-press commands, list item double-click commands, text box submission, and tagging for WPF and Avalonia.

## Demos

- [WPF](src/Okisioli.Teacups.UI.Demo.WPF)
- [Avalonia](src/Okisioli.Teacups.UI.Demo.Avalonia)
