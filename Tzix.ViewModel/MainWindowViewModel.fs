namespace Tzix.ViewModel

open Tzix.Model
open Basis.Core
open Dyxi.Util.Wpf

type MainWindowViewModel() =
  inherit ViewModel.Base()

  let _dict =
    Dict.createForDebug ()

  let _foundListViewModel = FoundListViewModel()

  let mutable _searchText = ""

  let searchIncrementally () =
    let items =
      if _searchText |> Str.isNullOrWhiteSpace
      then Seq.empty
      else
        _dict
        |> Dict.findInfix _searchText
        |> Seq.map (FileNodeViewModel.ofFileNode _dict)
    do _foundListViewModel.Items <- items |> Seq.toObservableCollection

  member this.SearchText
    with get () = _searchText
    and  set v  =
      _searchText <- v
      this.RaisePropertyChanged("SearchText")
      searchIncrementally ()

  member this.FoundList = _foundListViewModel
