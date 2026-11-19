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


### Text box submission and tagging

Both behavior packages provide `TextBoxSubmitBehavior` and its subclass `TaggingTextBoxBehavior` for `TextBox` controls. Attach one through the framework's `Interaction.Behaviors` collection:

```xml
<TextBox>
    <i:Interaction.Behaviors>
        <behaviors:TextBoxSubmitBehavior Command="{Binding SubmitCommand}" />
    </i:Interaction.Behaviors>
</TextBox>
```

Use `xmlns:i="http://schemas.microsoft.com/xaml/behaviors"` for WPF or `xmlns:i="using:Avalonia.Xaml.Interactivity"` for Avalonia. Set `behaviors` to `clr-namespace:Okisioli.Teacups.UI.Behaviors.WPF;assembly=Okisioli.Teacups.UI.Behaviors.WPF` or `using:Okisioli.Teacups.UI.Behaviors.Avalonia`, respectively.

Unmodified Enter submits the current text, including an empty string. `Command` is optional and receives that text as its parameter. The `Submitted` event exposes the same snapshot through `TextSubmittedEventArgs.Text`. The command executes first, then the event fires, then the box clears. Set `ClearOnSubmit="False"` to retain the text. Clearing preserves the text binding. A command returning `false` from `CanExecute` prevents the command, event, and clearing; the confirmation key or delimiter is still consumed. Read-only and disabled boxes do not submit.

For tagging, replace the behavior with:

```xml
<behaviors:TaggingTextBoxBehavior Command="{Binding AddTagCommand}" Delimiter="," />
```

`Delimiter` is a single character, defaulting to comma. Use `Delimiter="&#32;"` for a space. It is matched against committed text input rather than a physical key, respecting keyboard layouts and IME character entry. A standalone delimiter submits the entire current text and is not inserted. Multi-character input and pasted text are not split into tags. No trimming or empty-tag filtering is performed; use `CanExecute` for validation.

Avalonia requests an IME return key of `Done` while attached, with explicit text-box `TextInputOptions.ReturnKeyType` settings taking precedence. It handles Enter and standalone newline text confirmation, and avoids submitting on Enter while the text presenter has active IME preedit text. Native keyboard support depends on the platform; mobile keyboard and IME flows should be checked on target devices. Detaching removes the handlers and the behavior's return-key setting.
