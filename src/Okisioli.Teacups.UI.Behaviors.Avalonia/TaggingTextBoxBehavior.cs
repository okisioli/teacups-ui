using Avalonia;

namespace Okisioli.Teacups.UI.Behaviors.Avalonia;

/// <summary>Submits on Enter or a committed delimiter character, which is not inserted.</summary>
public sealed class TaggingTextBoxBehavior : TextBoxSubmitBehavior
{
    public static readonly StyledProperty<char> DelimiterProperty =
        AvaloniaProperty.Register<TaggingTextBoxBehavior, char>(nameof(Delimiter), ',');

    public char Delimiter
    {
        get => GetValue(DelimiterProperty);
        set => SetValue(DelimiterProperty, value);
    }

    protected override bool IsSubmitText(string? text) =>
        text is { Length: 1 } && text[0] == Delimiter;
}
