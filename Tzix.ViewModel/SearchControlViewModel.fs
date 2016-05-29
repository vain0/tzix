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
  let find word dict =
    match _searchSource with
    | SearchSource.All ->
        if word |> Str.isNullOrWhiteSpace
        then Seq.empty
        else
          dict |> Dict.findInfix word
          |> Seq.collect id
          |> Seq.map (FileNodeViewModel.ofFileNode dict)
    | SearchSource.Dir (_, items) ->
        items |> Seq.filter (fun item ->
          let node = dict |> Dict.findNode item.FileNodeId
          in node.Name |> Str.contains word
          )

  let searchAsync () =
    async {
      dispatcher.Invoke(fun () ->
        _foundListViewModel.Items <- Seq.empty
        )
      let items =
        find _searchText _dict
      dispatcher.Invoke(fun () ->
        _foundListViewModel.Items <- items
        )
    }

  let searchIncrementally prevSearchText =
    if _searchText |> Str.isNullOrEmpty then
      _searchSource <- SearchSource.All

    if (_searchText |> Str.startsWith prevSearchText)
      && (prevSearchText |> Str.isNullOrWhiteSpace |> not)
    then // If just appended some chars then reduce candidates.
      _foundListViewModel.Items <-
        _foundListViewModel.Items
        |> Seq.filter (fun item -> item.Name |> Str.contains _searchText)
    else
      searchAsync () |> Async.Start

  let _setSearchText v =
    let prevSearchText = _searchText
    _searchText <- v
    this.RaisePropertyChanged("SearchText")
    searchIncrementally prevSearchText

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
            _dict <- _dict |> Dict.removeNode node.Id
        ))
    |> fst

  let _selectDir nodeId =
    let (dict, nodeIds) = _dict |> Dict.selectDirectoryNode nodeId
    _dict <- dict
    let items =
      nodeIds |> Seq.map (fun nodeId ->
        dict |> Dict.findNode nodeId
        |> FileNodeViewModel.ofFileNode _dict
        )
    _setSearchText ""
    _searchSource <- SearchSource.Dir (nodeId, items)
    _foundListViewModel.Items <- items

  let _selectDirCommand =
    Command.create (fun _ -> true) (fun _ ->
      _foundListViewModel.SelectFirstIfNoSelection()
      _foundListViewModel.TrySelectedItem() |> Option.iter (fun item ->
        _selectDir item.FileNodeId
        ))
    |> fst

  let _selectParentDirCommand =
    Command.create (fun _ -> true) (fun _ ->
      option {
        _foundListViewModel.SelectFirstIfNoSelection()
        let! item       = _foundListViewModel.TrySelectedItem()
        let node        = _dict |> Dict.findNode item.FileNodeId
        let! parentId   = node.ParentId
        return _selectDir parentId
      } |> Option.getOr ())
    |> fst

  member this.SearchText
    with get () = _searchText
    and  set v  = _setSearchText v

  member this.FoundList = _foundListViewModel

  member this.CommitCommand = _commitCommand
  member this.SelectDirCommand = _selectDirCommand
  member this.SelectParentDirCommand = _selectParentDirCommand

  member this.Dict = _dict
