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

  let mutable _searchSource = SearchSource.All

  let mutable _searchText = ""

  /// 検索結果を列挙する。
  /// 非同期的に結果を伸ばしていくために列の列を返す。
  let find word dict =
    match _searchSource with
    | SearchSource.All ->
        if word |> Str.isNullOrWhiteSpace
        then Seq.empty
        else
          dict |> Dict.findInfix word
          |> Seq.map (Seq.map (FileNodeViewModel.ofFileNode dict))
    | SearchSource.Dir (_, items) ->
        items |> Seq.filter (fun item ->
          let node = dict |> Dict.findNode item.FileNodeId
          in node.Name |> Str.contains word
          )
        |> Seq.singleton

  let searchIncrementally () =
    async {
      dispatcher.Invoke(fun () ->
        _foundListViewModel.Items <- ObservableCollection()
        )
      let itemListList =
        find _searchText _dict
      for items in itemListList do
        dispatcher.Invoke(fun () ->
          _foundListViewModel.Items
          |> ObservableCollection.addRange (items |> Seq.toObservableCollection)
          )
    }
    |> Async.Start

  let _setSearchText v =
    _searchText <- v
    this.RaisePropertyChanged("SearchText")
    if _searchText |> Str.isNullOrEmpty then
      _searchSource <- SearchSource.All
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

  let _selectDirCommand =
    Command.create (fun _ -> true) (fun _ ->
      _foundListViewModel.SelectFirstIfNoSelection()
      _foundListViewModel.TrySelectedItem() |> Option.iter (fun item ->
        let (dict, nodeIds) = _dict |> Dict.selectDirectoryNode item.FileNodeId
        _dict <- dict
        let items =
          nodeIds |> Seq.map (fun nodeId ->
            dict |> Dict.findNode nodeId
            |> FileNodeViewModel.ofFileNode _dict
            )
        _setSearchText ""
        _searchSource <- SearchSource.Dir (item.FileNodeId, items)
        _foundListViewModel.Items <- items |> Seq.toObservableCollection
        ))
    |> fst

  member this.SearchText
    with get () = _searchText
    and  set v  = _setSearchText v

  member this.FoundList = _foundListViewModel

  member this.CommitCommand = _commitCommand
  member this.SelectDirCommand = _selectDirCommand

  member this.Dict = _dict
