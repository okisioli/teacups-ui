# Teacups UI Fastr for WPF

Fastr provides a buffered item source, incremental collection updates, and an optional scroll reset for WPF item controls. It works with standard controls such as `ListBox` and with custom controls derived from `ItemsControl`.

Add the `Okisioli.Teacups.UI.Fastr.WPF` package to a WPF project targeting `net10.0-windows`. The C# example also uses `DispatcherService` from `Okisioli.Teacups.UI.Core.WPF`.

## Bind a buffered item source

Move the existing `ItemsSource` binding to `BufferedItems.Source`. The behavior manages `ItemsSource` and switches to a fresh visible collection when the source signals that replacement is starting. The detached collection is then cleared in batches. Add `ItemsControlScroll.ResetOnItemsReset` to return to the top when the item source changes or reports a collection reset.

```xml
<ListBox
    xmlns:fastr="clr-namespace:Okisioli.Teacups.UI.Fastr.WPF.Behaviors;assembly=Fastr.WPF"
    fastr:BufferedItems.Source="{Binding CurrentFiles}"
    fastr:ItemsControlScroll.ResetOnItemsReset="True" />
```

You can also use the scroll behavior on its own with a regular `ItemsSource` binding.

For an existing custom `ItemsControl` such as `FileList`, use the same attached properties. Scroll reset uses the first `ScrollViewer` in the control's visual tree.

```xml
<local:FileList
    xmlns:local="clr-namespace:MyApp.Controls"
    xmlns:fastr="clr-namespace:Okisioli.Teacups.UI.Fastr.WPF.Behaviors;assembly=Fastr.WPF"
    fastr:BufferedItems.Source="{Binding CurrentFiles}"
    fastr:ItemsControlScroll.ResetOnItemsReset="True" />
```

## Replace a large item collection

Create the view model on the UI thread and bind to its `DispatcherCollection<T>`. If replacement starts with an asynchronous load, call `BeginUpdate()` before the load and pass that version to `ReplaceRangeAsync` so a newer request can supersede it.

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Threading;
using Okisioli.Teacups.UI.Core.WPF;
using Okisioli.Teacups.UI.Fastr.WPF.Collections;

public sealed class FilesViewModel
{
    public DispatcherCollection<string> CurrentFiles { get; } =
        new(new DispatcherService(Dispatcher.CurrentDispatcher));

    public Task ReplaceFilesAsync(IEnumerable<string> files)
    {
        var version = CurrentFiles.BeginUpdate();
        return CurrentFiles.ReplaceRangeAsync(files, version);
    }
}
```

See the [WPF demo](../../demo/Okisioli.Teacups.UI.Fastr.Demo.WPF) for standard, virtualized, and buffered `ListBox` examples. The virtualized example uses the third-party `VirtualizingWrapPanel`.
