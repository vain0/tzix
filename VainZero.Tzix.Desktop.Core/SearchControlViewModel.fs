namespace VainZero.Tzix.Desktop.Core

open System
open System.Collections.ObjectModel
open System.Windows.Threading
open VainZero.Tzix.Core
open Basis.Core
open Chessie.ErrorHandling
open Dyxi.Util.Wpf

type SearchControlViewModel(_dict: Dict) as this =
  inherit ViewModel.Base()

  let _searcher = Searcher(_dict)

  let _dict () = _searcher.Dict

  let mutable _searchText = ""

  let mutable _selectedIndex = -1

  let _trySelectedNode () =
    _searcher.FoundNodes |> Seq.tryItem _selectedIndex

  let _setSearchText v =
    let prevSearchText = _searchText
    _searchText <- v
    this.RaisePropertyChanged("SearchText")
    _searcher.SearchAsync(prevSearchText, _searchText) |> Async.Start

  let _commitCommand =
    Command.create (fun _ -> true) (fun _ ->
      this.SelectFirstIfNoSelection()
      _trySelectedNode () |> Option.iter (fun node ->
        match _dict () |> Dict.tryExecute node with
        | Ok (dict, _) ->
            _searcher.Dict <- dict
            _setSearchText ""
        | Bad es ->
            _searcher.Dict <- _dict () |> Dict.removeNode node.Id
        ))
    |> fst

  let _browseDir nodeId =
    _setSearchText ""
    _searcher.BrowseDir(nodeId)

  let _browseDirCommand =
    Command.create (fun _ -> true) (fun _ ->
      this.SelectFirstIfNoSelection()
      _trySelectedNode () |> Option.iter (fun node ->
        _browseDir node.Id
        ))
    |> fst

  let _browseGrandparentDirCommand =
    Command.create (fun _ -> true) (fun _ ->
      option {
        this.SelectFirstIfNoSelection()
        let! node       = _trySelectedNode ()
        let! parentId   = node.ParentId
        let parentNode  = _dict () |> Dict.findNode parentId
        let! gpId       = parentNode.ParentId
        return _browseDir gpId
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
      |> Seq.map (_dict () |> FileNodeViewModel.ofFileNode)
      |> Seq.chunkBySize 100
    and  set (itemChunks: seq<array<FileNodeViewModel>>) =
      itemChunks |> Seq.collect id
      |> Seq.map (fun item -> _dict () |> Dict.findNode item.FileNodeId)
      |> (fun nodes -> _searcher.FoundNodes <- nodes)  // raises PropertyChanged("ItemChunks")

  member this.CommitCommand = _commitCommand
  member this.BrowseDirCommand = _browseDirCommand
  member this.BrowseGrandparentDirCommand = _browseGrandparentDirCommand

  member this.Dict = _dict ()

  member this.Searcher = _searcher
