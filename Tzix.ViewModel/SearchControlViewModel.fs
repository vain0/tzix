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

  let mutable _dict = dict

  let mutable _searchSource = SearchSource.All

  let mutable _searchText = ""

  let mutable _selectedIndex = -1

  let mutable _foundNodes =
    (Seq.empty: seq<FileNode>)

  let _setNodes nodes =
    _foundNodes <- nodes
    this.RaisePropertyChanged("ItemChunks")

  let trySelectedNode () =
    _foundNodes |> Seq.tryItem _selectedIndex

  /// 検索結果を列挙する。
  let findNodes word =
    match _searchSource with
    | SearchSource.All ->
        if word |> Str.isNullOrWhiteSpace
        then Seq.empty
        else
          dict |> Dict.findInfix word
          |> Seq.collect id
    | SearchSource.Dir (_, nodes) ->
        nodes |> Seq.filter (fun node ->
          node.Name |> Str.contains word
          )

  let searchAsync prevSearchText =
    async {
      if _searchText |> Str.isNullOrEmpty then
        _searchSource <- SearchSource.All

      let nodes =
        if (_searchText |> Str.startsWith prevSearchText)
          && (prevSearchText |> Str.isNullOrWhiteSpace |> not)
        then // If just appended some chars then reduce candidates.
          _foundNodes
          |> Seq.filter (fun item -> item.Name |> Str.contains _searchText)
        else
          findNodes _searchText
      _setNodes nodes
    }

  let _setSearchText v =
    let prevSearchText = _searchText
    _searchText <- v
    this.RaisePropertyChanged("SearchText")
    searchAsync prevSearchText |> Async.Start

  let _commitCommand =
    Command.create (fun _ -> true) (fun _ ->
      this.SelectFirstIfNoSelection()
      trySelectedNode () |> Option.iter (fun node ->
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
    let nodes =
      nodeIds |> Seq.map (fun nodeId -> dict |> Dict.findNode nodeId)
    _setSearchText ""
    _searchSource <- SearchSource.Dir (nodeId, nodes)
    _setNodes nodes

  let _selectDirCommand =
    Command.create (fun _ -> true) (fun _ ->
      this.SelectFirstIfNoSelection()
      trySelectedNode () |> Option.iter (fun node ->
        _selectDir node.Id
        ))
    |> fst

  let _selectParentDirCommand =
    Command.create (fun _ -> true) (fun _ ->
      option {
        this.SelectFirstIfNoSelection()
        let! node       = trySelectedNode ()
        let! parentId   = node.ParentId
        return _selectDir parentId
      } |> Option.getOr ())
    |> fst

  member this.SearchText
    with get () = _searchText
    and  set v  = _setSearchText v

  member this.SelectedIndex
    with get () = _selectedIndex
    and  set v  =
      _selectedIndex <- v
      this.RaisePropertyChanged("SelectedIndex")

  member this.SelectFirstIfNoSelection() =
    if this.SelectedIndex < 0 && (_foundNodes |> Seq.isEmpty |> not) then
      this.SelectedIndex <- 0

  member this.ItemChunks
    with get () =
      _foundNodes
      |> Seq.map (FileNodeViewModel.ofFileNode _dict)
      |> Seq.chunkBySize 100
    and  set (itemChunks: seq<array<FileNodeViewModel>>) =
      itemChunks |> Seq.collect id
      |> Seq.map (fun item -> _dict |> Dict.findNode item.FileNodeId)
      |> _setNodes
      this.RaisePropertyChanged("ItemChunks")

  member this.CommitCommand = _commitCommand
  member this.SelectDirCommand = _selectDirCommand
  member this.SelectParentDirCommand = _selectParentDirCommand

  member this.Dict = _dict
