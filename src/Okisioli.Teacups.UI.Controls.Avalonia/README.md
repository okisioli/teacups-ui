# Okisioli.Teacups.UI.Controls.Avalonia

Reusable Avalonia control styles.

## Drawer sheet

`DrawerSheet` replaces `BottomSheet` and overlays optional `SheetContent` at the edge selected by `Placement`.

```xml
<controls:DrawerSheet xmlns:controls="clr-namespace:Okisioli.Teacups.UI.Controls.Avalonia;assembly=Okisioli.Teacups.UI.Controls.Avalonia"
                      IsExpanded="{Binding IsSomethingOpen}">
  <controls:DrawerSheet.SheetContent>
    <SomeArbitraryControl />
  </controls:DrawerSheet.SheetContent>

  <MainPageContent />
</controls:DrawerSheet>
```

`Placement` uses the Avalonia `Dock` enum and defaults to `Bottom`. Top and bottom sheets stretch across the available width. Left and right sheets stretch across the available height. The sheet sizes to its content along the direction it slides.

## Circular progress bar

Import the style dictionary in your application resources:

```xml
<StyleInclude Source="avares://Okisioli.Teacups.UI.Controls.Avalonia/Themes/CircularProgressBar.axaml" />
```

Then apply the `circular` class to an ordinary `ProgressBar`. The ring uses `Foreground` for the filled part and `Background` for the empty part.

```xml
<ProgressBar xmlns:local="clr-namespace:Okisioli.Teacups.UI.Controls.Avalonia;assembly=Okisioli.Teacups.UI.Controls.Avalonia"
             Classes="circular"
             Width="160"
             Height="160"
             Minimum="0"
             Maximum="100"
             Value="64"
             Foreground="DodgerBlue"
             Background="#303030"
             local:CircularProgress.RingThickness="12"
             local:CircularProgress.Reverse="False" />
```

`RingThickness` is measured in device-independent pixels. The style fits the circle inside the shortest control dimension.
Set `CircularProgress.Reverse` to `True` to make the filled portion grow counter-clockwise.
