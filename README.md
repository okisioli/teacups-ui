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

The `Demo.Avalonia` and `Controls.Avalonia.Demo` projects demonstrate selected Avalonia components and are not library packages.

## Core services

`IDialogService<TViewModel, TView>` shows a view for a dialog view model and returns `true` for acceptance or `false` for cancellation. Use its factory overload when the view model should receive the dialog-scoped `IDialogCompletion`; call `Confirm()` or `Cancel()` from the view model's commands.

`IDispatcherService` queues an `Action` on the UI dispatcher, even when called from the UI thread. Its priorities, from lowest to highest, are `ApplicationIdle`, `Background`, `Input`, and `Normal`, mapped to the matching native WPF or Avalonia priority. The default is `Normal`; use `Background` for incremental collection updates that should yield to input and rendering. Replace uses of the former `High` priority with `Input`.

## Framework implementations

Both Core.WPF and Core.Avalonia implement `IDispatcherService`, accepting their framework's dispatcher in the constructor.

Their `DialogService<TViewModel, TView>` implementations require a `UserControl` view with a parameterless constructor. WPF hosts that view in a library-owned modal window; closing the window cancels the dialog. Avalonia pushes the view as a full-screen page on a `NavigationPage`. Its `confirmOnBackNavigation` constructor argument explicitly determines whether back navigation confirms or cancels the dialog.

## Controls

Controls.Avalonia supplies the circular `ProgressBar` style and the `CircularProgress` attached properties: `RingThickness` configures the ring width and `Reverse` changes the fill direction.

## Behaviors

Both behavior packages provide `LongPressBehavior` and `ListBoxDoubleClickBehavior`. `LongPressBehavior` invokes an `ICommand` after a configurable hold (500 ms by default); `ListBoxDoubleClickBehavior` invokes an `ICommand` with the double-clicked list item's data context. The WPF package uses `Microsoft.Xaml.Behaviors`; the Avalonia package uses `Avalonia.Xaml.Interactivity`.
