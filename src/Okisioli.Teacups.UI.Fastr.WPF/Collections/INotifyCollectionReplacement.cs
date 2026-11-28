namespace Okisioli.Teacups.UI.Fastr.WPF.Collections;

/// <summary>Provides advance notice that a collection's items are being replaced.</summary>
public interface INotifyCollectionReplacement
{
    /// <summary>Occurs before replacement changes are applied to the collection.</summary>
    event EventHandler? ReplacementStarting;
}
