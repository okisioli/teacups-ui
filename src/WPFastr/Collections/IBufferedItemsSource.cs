using System.Collections;
using System.ComponentModel;

namespace Olicode.WPFastr.Collections;

public interface IBufferedItemsSource : INotifyPropertyChanged
{
    IEnumerable VisibleItems { get; }
}
