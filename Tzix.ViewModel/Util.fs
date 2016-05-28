namespace Tzix.ViewModel

module ObservableCollection =
  open System.Collections.ObjectModel

  let addRange xs (this: ObservableCollection<_>) =
    for x in xs do
      this.Add(x)
