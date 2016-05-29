namespace Tzix.ViewModel

open System
open System.Collections.ObjectModel
open System.Windows.Threading
open Tzix.Model
open Basis.Core
open Chessie.ErrorHandling
open Dyxi.Util.Wpf

type SearchControlViewModel(_dict: Dict, dispatcher: Dispatcher) as this =
  inherit ViewModel.Base()

  let _searcher = Searcher(_dict)

  let mutable _searchText = ""

  let mutable _selectedIndex = -1

  let trySelectedNode () =
    _searcher.FoundNodes |> Seq.tryItem _selectedIndex

  let _setSearchText v =
    let prevSearchText = _searchText
    _searchText <- v
    this.RaisePropertyChanged("SearchText")
    _searcher.SearchAsync(prevSearchText, _searchText) |> Async.Start

  let _commitCommand =
    Command.create (fun _ -> true) (fun _ ->
      this.SelectFirstIfNoSelection()
      trySelectedNode () |> Option.iter (fun node ->
        match _dict |> Dict.tryExecute node with
        | Ok (dict, _) ->
            _searcher.Dict <- dict
            _setSearchText ""
        | Bad es ->
            _searcher.Dict <- _dict |> Dict.removeNode node.Id
        ))
    |> fst

  let _selectDir nodeId =
    _setSearchText ""
    _searcher.BrowseDir(nodeId)

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

  do
    _searcher.FoundNodesChanged.Add(fun _ ->
      this.RaisePropertyChanged("ItemChunks")
      )

  member this.SearchText
    with get () = _searchText
    and  set v  = _setSearchText v

  member this.SelectedIndex
    with get () = _selectedIndex
    and  set v  =
      _selectedIndex <- v
      this.RaisePropertyChanged("SelectedIndex")

  member this.SelectFirstIfNoSelection() =
    if this.SelectedIndex < 0 && (_searcher.FoundNodes |> Seq.isEmpty |> not) then
      this.SelectedIndex <- 0

  member this.ItemChunks
    with get () =
      _searcher.FoundNodes
      |> Seq.map (FileNodeViewModel.ofFileNode _dict)
      |> Seq.chunkBySize 100
    and  set (itemChunks: seq<array<FileNodeViewModel>>) =
      itemChunks |> Seq.collect id
      |> Seq.map (fun item -> _dict |> Dict.findNode item.FileNodeId)
      |> (fun nodes -> _searcher.FoundNodes <- nodes)  // raises PropertyChanged("ItemChunks")

  member this.CommitCommand = _commitCommand
  member this.SelectDirCommand = _selectDirCommand
  member this.SelectParentDirCommand = _selectParentDirCommand

  member this.Dict = _dict
