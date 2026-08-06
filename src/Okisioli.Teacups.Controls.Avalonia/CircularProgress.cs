using Avalonia;
using Avalonia.Controls;

namespace Okisioli.Teacups.Controls.Avalonia;

/// <summary>
/// Provides configuration properties for the circular <see cref="ProgressBar"/> style.
/// </summary>
public sealed class CircularProgress
{
    private CircularProgress()
    {
    }

    /// <summary>
    /// Defines the thickness, in device-independent pixels, of a circular progress ring.
    /// </summary>
    public static readonly AttachedProperty<double> RingThicknessProperty =
        AvaloniaProperty.RegisterAttached<CircularProgress, ProgressBar, double>(
            "RingThickness",
            defaultValue: 8d,
            validate: static value => double.IsFinite(value) && value >= 0d);

    /// <summary>
    /// Sets the thickness, in device-independent pixels, of a circular progress ring.
    /// </summary>
    public static void SetRingThickness(ProgressBar element, double value)
    {
        element.SetValue(RingThicknessProperty, value);
    }

    /// <summary>
    /// Gets the thickness, in device-independent pixels, of a circular progress ring.
    /// </summary>
    public static double GetRingThickness(ProgressBar element)
    {
        return element.GetValue(RingThicknessProperty);
    }
}
