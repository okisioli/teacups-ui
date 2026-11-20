using Avalonia;
using Avalonia.Controls;

namespace Okisioli.Teacups.UI.Controls.Avalonia;

/// <summary>
/// Provides configuration properties for the circular <see cref="ProgressBar"/> style.
/// </summary>
/// <remarks>
/// Include <c>avares://Okisioli.Teacups.UI.Controls.Avalonia/Themes/CircularProgressBar.axaml</c>
/// in the application styles and add the <c>circular</c> class to a <see cref="ProgressBar"/>.
/// </remarks>
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

    /// <summary>
    /// Defines whether the filled portion of a circular progress ring grows counter-clockwise.
    /// </summary>
    public static readonly AttachedProperty<bool> ReverseProperty =
        AvaloniaProperty.RegisterAttached<CircularProgress, ProgressBar, bool>(
            "Reverse",
            defaultValue: false);

    /// <summary>
    /// Sets whether the filled portion of a circular progress ring grows counter-clockwise.
    /// </summary>
    public static void SetReverse(ProgressBar element, bool value)
    {
        element.SetValue(ReverseProperty, value);
    }

    /// <summary>
    /// Gets whether the filled portion of a circular progress ring grows counter-clockwise.
    /// </summary>
    public static bool GetReverse(ProgressBar element)
    {
        return element.GetValue(ReverseProperty);
    }
}
