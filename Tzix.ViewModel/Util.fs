namespace Tzix.ViewModel

module ObservableCollection =
  open System.Collections.ObjectModel

  let addRange xs (this: ObservableCollection<_>) =
    for x in xs do
      this.Add(x)

  let removeIf pred (this: ObservableCollection<_>) =
    let rec loop i =
      if i < this.Count then
        if pred (this.[i]) then
          this.RemoveAt(i)
          loop i
        else
          loop (i + 1)
    in loop 0
