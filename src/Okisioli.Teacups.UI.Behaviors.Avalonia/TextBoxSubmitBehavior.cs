using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace Okisioli.Teacups.UI.Behaviors.Avalonia;

/// <summary>Submits the current text in a <see cref="TextBox"/>.</summary>
/// <remarks>
/// <para>
/// Press Enter without modifiers or use a platform confirmation that commits a standalone newline.
/// Enter used to accept an active IME composition does not submit the text box.
/// </para>
/// <para>
/// While attached, the behavior requests the <see cref="TextInputReturnKeyType.Done"/> keyboard
/// return key. An explicit <see cref="TextInputOptions.ReturnKeyTypeProperty"/> setting takes
/// precedence. Detaching removes the input handlers and the behavior's return-key setting.
/// Keyboard and IME support vary by platform. Verify input on target devices.
/// </para>
/// <para>
/// Confirmation input is consumed even when <see cref="ICommand.CanExecute"/> rejects submission.
/// </para>
/// </remarks>
public class TextBoxSubmitBehavior : Behavior<TextBox>
{
    private IDisposable? returnKeySetting;

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<TextBoxSubmitBehavior, ICommand?>(nameof(Command));

    /// <summary>Gets or sets the optional command, which receives the submitted text as a string.</summary>
    /// <remarks>
    /// If <see cref="ICommand.CanExecute"/> returns <see langword="false"/>, submission stops
    /// before executing the command, raising <see cref="Submitted"/>, or clearing the text.
    /// </remarks>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly StyledProperty<bool> ClearOnSubmitProperty =
        AvaloniaProperty.Register<TextBoxSubmitBehavior, bool>(nameof(ClearOnSubmit), false);

    /// <summary>Gets or sets whether to clear the text after submission while preserving its binding.</summary>
    /// <remarks>
    /// Defaults to <see langword="false"/> here and <see langword="true"/> on
    /// <see cref="TaggingTextBoxBehavior"/>. Set this property explicitly to override either default.
    /// </remarks>
    public bool ClearOnSubmit
    {
        get => GetValue(ClearOnSubmitProperty);
        set => SetValue(ClearOnSubmitProperty, value);
    }

    /// <summary>Occurs after the command runs, if set, and before optional clearing.</summary>
    /// <remarks><see cref="TextSubmittedEventArgs.Text"/> contains the same text passed to the command.</remarks>
    public event EventHandler<TextSubmittedEventArgs>? Submitted;

    protected override void OnAttached()
    {
        base.OnAttached();
        returnKeySetting = AssociatedObject!.SetValue(TextInputOptions.ReturnKeyTypeProperty,
            TextInputReturnKeyType.Done, BindingPriority.Style);
        AssociatedObject.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        AssociatedObject.AddHandler(InputElement.TextInputEvent, OnTextInput, RoutingStrategies.Tunnel);
    }

    protected override void OnDetaching()
    {
        AssociatedObject!.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        AssociatedObject.RemoveHandler(InputElement.TextInputEvent, OnTextInput);
        returnKeySetting?.Dispose();
        returnKeySetting = null;
        base.OnDetaching();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        // Enter used to accept an IME candidate must not submit the field.
        if (AssociatedObject!.GetVisualDescendants().OfType<TextPresenter>()
            .Any(presenter => !string.IsNullOrEmpty(presenter.PreeditText)))
            return;

        if (!e.Handled && e.Key == Key.Enter && e.KeyModifiers == KeyModifiers.None)
        {
            // Consume confirmation even when a command currently rejects submission.
            TrySubmit();
            e.Handled = true;
        }
    }

    private void OnTextInput(object? sender, TextInputEventArgs e)
    {
        if (!e.Handled && (e.Text is "\r" or "\n" or "\r\n" || IsSubmitText(e.Text)))
        {
            TrySubmit();
            e.Handled = true;
        }
    }

    /// <summary>Determines whether committed text acts as a submission delimiter.</summary>
    protected virtual bool IsSubmitText(string? text) => false;

    /// <summary>Submits a snapshot of the current text, including an empty string.</summary>
    /// <remarks>
    /// Executes <see cref="Command"/> if set, raises <see cref="Submitted"/>, then clears the text
    /// if <see cref="ClearOnSubmit"/> is enabled.
    /// </remarks>
    /// <returns>
    /// <see langword="true"/> if submitted. Returns <see langword="false"/> when detached,
    /// when the text box is disabled or read-only, or when the command rejects the text.
    /// </returns>
    protected bool TrySubmit()
    {
        var textBox = AssociatedObject;
        if (textBox is null || textBox.IsReadOnly || !textBox.IsEnabled)
            return false;

        var text = textBox.Text ?? string.Empty;
        var command = Command;
        if (command is not null && !command.CanExecute(text))
            return false;

        command?.Execute(text);
        Submitted?.Invoke(this, new TextSubmittedEventArgs(text));
        if (ClearOnSubmit)
            textBox.SetCurrentValue(TextBox.TextProperty, string.Empty);
        return true;
    }
}
