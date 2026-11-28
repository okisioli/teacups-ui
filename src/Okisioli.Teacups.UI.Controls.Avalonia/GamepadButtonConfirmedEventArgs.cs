namespace Okisioli.Teacups.UI.Controls.Avalonia;

/// <summary>The choice confirmed by a <see cref="GamepadButton"/>.</summary>
public sealed class GamepadButtonConfirmedEventArgs(GamepadButtonSelection selection) : EventArgs
{
    /// <summary>Gets the confirmed choice.</summary>
    public GamepadButtonSelection Selection { get; } = selection;
}
