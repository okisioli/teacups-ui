# WPFastr

WPFastr provides cross-platform atomic and incremental item-source updates through a consumer-supplied UI dispatcher. Framework-specific dispatcher integrations live in separate packages.

Both packages retain the .NET 6 compatibility floor:

- `Olivercode.WPFastr` targets `net6.0` and contains no WPF dependency.
- `Olivercode.WPFastr.Windows` targets `net6.0-windows`, references the core package, and provides WPF integration.

## Install

The packages are hosted on GitHub Packages. Add the package source using a GitHub token with `read:packages` permission:

```powershell
dotnet nuget add source "https://nuget.pkg.github.com/oli-g-sk/index.json" `
  --name github `
  --username YOUR_GITHUB_USERNAME `
  --password YOUR_GITHUB_TOKEN `
  --store-password-in-clear-text
```

Install the core package in platform-neutral projects:

```powershell
dotnet add package Olivercode.WPFastr --version VERSION --source github
```

WPF applications can install the Windows package, which brings in the matching core dependency:

```powershell
dotnet add package Olivercode.WPFastr.Windows --version VERSION --source github
```

## Atomic visible-items swaps

`BufferedItemsSource<T>` materializes the next snapshot, then replaces its `VisibleItems` property in one dispatched assignment. Bind through that property:

```xml
<ItemsControl ItemsSource="{Binding MyItems.VisibleItems}" />
```

```csharp
using Olicode.WPFastr.Collections;

public BufferedItemsSource<FileItem> MyItems { get; }

MyItems = new BufferedItemsSource<FileItem>(dispatcherService);

var version = MyItems.BeginUpdate();
await MyItems.ReplaceRangeAsync(newItems, version);
```

`BufferedItemsSource<T>` implements `IBufferedItemsSource` and `INotifyPropertyChanged`. It contains no control or framework dependency: WPF, Avalonia, and MAUI bindings all observe the same `VisibleItems` property change.

## Dispatcher observable collections

`DispatcherObservableCollection<T>` is a normal `ObservableCollection<T>`, so WPF, Avalonia, MAUI, and other frameworks can bind to it directly:

```xml
<ItemsControl ItemsSource="{Binding MyItems}" />
```

The view model supplies the appropriate framework dispatcher:

```csharp
using Olicode.WPFastr.Collections;

public DispatcherObservableCollection<FileItem> MyItems { get; }

MyItems = new DispatcherObservableCollection<FileItem>(
    dispatcherService,
    batchSize: 16);

var version = MyItems.BeginUpdate();
await MyItems.ReplaceRangeAsync(newItems, version);
```

Each dispatcher callback processes at most `BatchSize` changes. Control then returns to the framework dispatcher before the next batch, keeping the UI responsive. The default batch size is `1`, which preserves maximum yielding; larger values trade some responsiveness for throughput.

Calling `BeginUpdate()` invalidates older update versions. In-flight operations check their version between and within dispatched batches, allowing stale replacements to stop without applying the remaining items.

## Dispatcher services

Core only defines `IDispatcherService` and the framework-neutral `DispatchPriority` enum. Applications or framework packages supply the implementation.

The Windows package includes `WpfDispatcherService`:

```csharp
using Olicode.WPFastr.Windows.Threading;

var dispatcherService = new WpfDispatcherService(Application.Current.Dispatcher);
```

A future Avalonia package only needs to provide an `AvaloniaDispatcherService` that maps `DispatchPriority` to Avalonia's dispatcher.

## WPF scroll reset

`ItemsControlScroll` remains a WPF-only behavior. Scroll position is preserved by default; opt in to returning to the top when the source is reset:

```xml
<Window xmlns:behaviors="clr-namespace:Olicode.WPFastr.Windows.Behaviors;assembly=Olicode.WPFastr.Windows">
    <ListBox ItemsSource="{Binding MyItems}"
             behaviors:ItemsControlScroll.ResetOnItemsReset="True" />
</Window>
```
