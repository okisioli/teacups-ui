using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Okisioli.Teacups.UI.Behaviors.WPF;

/// <summary>Submits a text box on Enter or platform confirmation.</summary>
public class TextBoxSubmitBehavior : Behavior<TextBox>
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(TextBoxSubmitBehavior));

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty ClearOnSubmitProperty =
        DependencyProperty.Register(nameof(ClearOnSubmit), typeof(bool), typeof(TextBoxSubmitBehavior), new PropertyMetadata(false));

    public bool ClearOnSubmit
    {
        get => (bool)GetValue(ClearOnSubmitProperty);
        set => SetValue(ClearOnSubmitProperty, value);
    }

    /// <summary>Raised with the text captured before clearing the text box.</summary>
    public event EventHandler<TextSubmittedEventArgs>? Submitted;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewKeyDown += OnKeyDown;
        AssociatedObject.PreviewTextInput += OnTextInput;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PreviewKeyDown -= OnKeyDown;
        AssociatedObject.PreviewTextInput -= OnTextInput;
        base.OnDetaching();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (!e.Handled && e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
        {
            // Consume confirmation even when a command currently rejects submission.
            TrySubmit();
            e.Handled = true;
        }
    }

    private void OnTextInput(object? sender, TextCompositionEventArgs e)
    {
        if (!e.Handled && IsSubmitText(e.Text))
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
            textBox.SetCurrentValue(TextBox.TextProperty, string.Empty);
        return true;
    }
}
