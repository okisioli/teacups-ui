# Dialog Service

> This is a local draft of a future GitHub Wiki page. When the Wiki is enabled, copy it to the separate Wiki repository as `Dialog-Service.md`; this file does not publish a Wiki page.

`Okisioli.Teacups.UI.Core` defines `IDialogService<TViewModel, TView>`, so application code can show a dialog without depending on a UI framework. Both implementations return `true` when accepted and `false` when cancelled.

## Complete a dialog from its view model

Use the factory overload to give the view model the dialog-scoped `IDialogCompletion`. Its commands can call `Confirm()` or `Cancel()`.

```csharp
using Okisioli.Teacups.UI.Core;

var accepted = await dialogService.ShowDialogAsync(
    completion => new EditCustomerViewModel(
        save: completion.Confirm,
        cancel: completion.Cancel));
```

The view model and view types are supplied by the service type, for example `IDialogService<EditCustomerViewModel, EditCustomerView>`.

## WPF

`Okisioli.Teacups.UI.Core.WPF.DialogService<TViewModel, TView>` hosts the `UserControl` view in a library-owned modal `Window`. Construct it with the application's UI `Dispatcher`:

```csharp
using System.Windows.Threading;
using Okisioli.Teacups.UI.Core.WPF;

var dialogService = new DialogService<EditCustomerViewModel, EditCustomerView>(
    Dispatcher.CurrentDispatcher);
```

Closing the window, including with Escape, cancels the dialog unless the view model has already completed it.

## Avalonia

`Okisioli.Teacups.UI.Core.Avalonia.DialogService<TViewModel, TView>` displays the `UserControl` as a full-screen `ContentPage` in a `NavigationPage` stack:

```csharp
using Avalonia.Controls;
using Okisioli.Teacups.UI.Core.Avalonia;

var dialogService = new DialogService<EditCustomerViewModel, EditCustomerView>(
    navigationPage,
    confirmOnBackNavigation: false);
```

The `confirmOnBackNavigation` constructor argument is mandatory: choose `true` when a mobile back navigation should confirm, or `false` when it should cancel. A confirmed dialog is popped from the navigation stack; back navigation completes the dialog using the configured choice.
