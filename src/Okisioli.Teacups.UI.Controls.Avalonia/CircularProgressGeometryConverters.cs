using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Okisioli.Teacups.UI.Controls.Avalonia;

/// <summary>
/// Produces the geometry for the circular progress track.
/// </summary>
public sealed class CircularProgressTrackGeometryConverter : IMultiValueConverter
{
    /// <inheritdoc />
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2
            || values[0] is not Rect bounds
            || bounds.Width <= 0d
            || bounds.Height <= 0d
            || !TryGetDouble(values[1], out var thickness))
        {
            return null;
        }

        return CircularProgressGeometry.CreateTrack(bounds, thickness);
    }

    private static bool TryGetDouble(object? value, out double result)
    {
        result = value is double doubleValue ? doubleValue : 0d;
        return value is double;
    }
}

/// <summary>
/// Produces the geometry for the filled portion of a circular progress ring.
/// </summary>
public sealed class CircularProgressValueGeometryConverter : IMultiValueConverter
{
    /// <inheritdoc />
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 5 || values[0] is not Rect bounds || bounds.Width <= 0d || bounds.Height <= 0d)
            return null;

        if (!TryGetDouble(values[1], out var value)
            || !TryGetDouble(values[2], out var minimum)
            || !TryGetDouble(values[3], out var maximum)
            || !TryGetDouble(values[4], out var thickness))
        {
            return null;
        }

        return CircularProgressGeometry.CreateValue(bounds, value, minimum, maximum, thickness);
    }

    private static bool TryGetDouble(object? value, out double result)
    {
        result = value is double doubleValue ? doubleValue : 0d;
        return value is double;
    }

    private static double ToThickness(object? value)
    {
        return value is double thickness ? thickness : 8d;
    }
}

internal static class CircularProgressGeometry
{
    public static Geometry? CreateTrack(Rect bounds, double thickness)
    {
        if (!TryGetRing(bounds, thickness, out var centerX, out var centerY, out var radius))
            return null;

        return Parse($"M {centerX} {centerY - radius} A {radius} {radius} 0 0 1 {centerX} {centerY + radius} A {radius} {radius} 0 0 1 {centerX} {centerY - radius}");
    }

    public static Geometry? CreateValue(Rect bounds, double value, double minimum, double maximum, double thickness)
    {
        if (!TryGetRing(bounds, thickness, out var centerX, out var centerY, out var radius) || maximum <= minimum)
            return null;

        var progress = Math.Clamp((value - minimum) / (maximum - minimum), 0d, 1d);
        if (progress <= 0d)
            return null;

        if (progress >= 1d)
            return CreateTrack(bounds, thickness);

        var angle = progress * 2d * Math.PI;
        var endX = centerX + (radius * Math.Sin(angle));
        var endY = centerY - (radius * Math.Cos(angle));
        var isLargeArc = progress > 0.5d ? 1 : 0;

        return Parse($"M {centerX} {centerY - radius} A {radius} {radius} 0 {isLargeArc} 1 {endX} {endY}");
    }

    private static bool TryGetRing(Rect bounds, double thickness, out double centerX, out double centerY, out double radius)
    {
        centerX = bounds.Width / 2d;
        centerY = bounds.Height / 2d;
        radius = (Math.Min(bounds.Width, bounds.Height) - thickness) / 2d;
        return double.IsFinite(radius) && radius > 0d;
    }

    private static Geometry Parse(FormattableString path)
    {
        return Geometry.Parse(FormattableString.Invariant(path));
    }
}
