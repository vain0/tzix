namespace Tzix.ViewModel

open System.Collections.ObjectModel
open Tzix.Model
open Dyxi.Util.Wpf

type FoundListViewModel() =
  inherit ViewModel.Base()

  let mutable _selectedIndex = -1

  let mutable _items =
    ObservableCollection<FileNode>()

  member this.Items
    with get () = _items
    and  set v  = _items <- v; this.RaisePropertyChanged("Items")

  member this.SelectedIndex
    with get () = _selectedIndex
    and  set v  =
      _selectedIndex <- v
      for name in ["SelectedIndex"; "SelectedItem"] do
        this.RaisePropertyChanged(name)

  member this.TrySelectedItem =
    _items |> Seq.tryItem _selectedIndex
