using System.Windows;

namespace Okisioli.Teacups.UI.Behaviors.WPF;

/// <summary>Submits on Enter or a committed delimiter character, which is not inserted.</summary>
public sealed class TaggingTextBoxBehavior : TextBoxSubmitBehavior
{
    public static readonly DependencyProperty DelimiterProperty =
        DependencyProperty.Register(nameof(Delimiter), typeof(char), typeof(TaggingTextBoxBehavior), new PropertyMetadata(','));

    public char Delimiter
    {
        get => (char)GetValue(DelimiterProperty);
        set => SetValue(DelimiterProperty, value);
    }

    protected override bool IsSubmitText(string? text) =>
        text is { Length: 1 } && text[0] == Delimiter;
}
