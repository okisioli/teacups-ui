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

/// <summary>Submits a text box on Enter or platform confirmation.</summary>
public class TextBoxSubmitBehavior : Behavior<TextBox>
{
    private IDisposable? returnKeySetting;

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<TextBoxSubmitBehavior, ICommand?>(nameof(Command));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly StyledProperty<bool> ClearOnSubmitProperty =
        AvaloniaProperty.Register<TextBoxSubmitBehavior, bool>(nameof(ClearOnSubmit), false);

    public bool ClearOnSubmit
    {
        get => GetValue(ClearOnSubmitProperty);
        set => SetValue(ClearOnSubmitProperty, value);
    }

    /// <summary>Raised with the text captured before clearing the text box.</summary>
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

    /// <summary>Submits the current text unless the box is read-only or the command rejects it.</summary>
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
        {
            textBox.SetCurrentValue(TextBox.TextProperty, string.Empty);
            textBox.Focus();
        }
        return true;
    }
}
