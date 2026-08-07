# Okisioli.Teacups.UI.Controls.Avalonia

Reusable Avalonia control styles.

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
