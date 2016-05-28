namespace Tzix.ViewModel

open System
open System.Collections.ObjectModel
open System.Windows.Threading
open Tzix.Model
open Basis.Core
open Chessie.ErrorHandling
open Dyxi.Util.Wpf

type SearchControlViewModel(dict: Dict, dispatcher: Dispatcher) as this =
  inherit ViewModel.Base()

  let _foundListViewModel = FoundListViewModel()

  let mutable _dict = dict

  let mutable _searchText = ""

  let searchIncrementally () =
    async {
      dispatcher.Invoke(fun () ->
        _foundListViewModel.Items <- ObservableCollection()
        )
      let itemListList =
        if _searchText |> Str.isNullOrWhiteSpace
        then Seq.singleton Seq.empty
        else _dict |> Dict.findInfix _searchText
      for items in itemListList do
        let items =
          items
          |> Seq.map (FileNodeViewModel.ofFileNode _dict)
          |> Seq.toObservableCollection
        dispatcher.Invoke(fun () ->
          _foundListViewModel.Items |> ObservableCollection.addRange items
          )
    }
    |> Async.Start

  let _setSearchText v =
    _searchText <- v
    this.RaisePropertyChanged("SearchText")
    searchIncrementally ()

  let _commitCommand =
    Command.create (fun _ -> true) (fun _ ->
      _foundListViewModel.SelectFirstIfNoSelection()
      _foundListViewModel.TrySelectedItem() |> Option.iter (fun item ->
        let node      = _dict |> Dict.findNode item.FileNodeId
        match _dict |> Dict.tryExecute node with
        | Ok (dict, _) ->
            _dict <- dict
            _setSearchText ""
        | Bad es ->
            _foundListViewModel.Items.Remove(item) |> ignore
            _dict <- _dict |> Dict.removeNode node.Id
        ))
    |> fst

  member this.SearchText
    with get () = _searchText
    and  set v  = _setSearchText v

  member this.FoundList = _foundListViewModel

  member this.CommitCommand = _commitCommand

  member this.Dict = _dict
