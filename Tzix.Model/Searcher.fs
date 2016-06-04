namespace Tzix.Model

open System
open Basis.Core

type Searcher(_dict: Dict) as this =
  let mutable _dict = _dict

  let mutable _searchSource = SearchSource.All

  let _foundNodesChanged = Event<EventHandler, EventArgs>()

  let mutable _foundNodes =
    (Seq.empty: seq<FileNode>)

  let _setFoundNodes v =
    _foundNodes <- v
    _foundNodesChanged.Trigger(this, null)

  let _findNodes word =
    match _searchSource with
    | SearchSource.All ->
        if word |> Str.isNullOrWhiteSpace
        then Seq.empty
        else _dict |> Dict.findInfix word
    | SearchSource.Dir (_, nodes) ->
        nodes |> Seq.filter (fun node ->
          node.Name |> Str.contains word
          )

  let _searchAsync prevSearchText newSearchText =
    async {
      if newSearchText |> Str.isNullOrEmpty then
        _searchSource <- SearchSource.All
      let nodes =
        if (newSearchText |> Str.startsWith prevSearchText)
          && (prevSearchText |> Str.isNullOrWhiteSpace |> not)
        then // If just appended some chars then reduce candidates.
          _foundNodes
          |> Seq.filter (fun item -> item.Name |> Str.contains newSearchText)
        else
          _findNodes newSearchText
      in
        _setFoundNodes nodes
    }

  let _browseDir nodeId =
    let (dict, nodeIds) = _dict |> Dict.browseNode nodeId
    _dict <- dict
    let nodes =
      nodeIds
      |> List.map (fun nodeId -> dict |> Dict.findNode nodeId)
    _searchSource <- SearchSource.Dir (nodeId, nodes)
    _setFoundNodes nodes

  member this.FoundNodes
    with get () = _foundNodes
    and  set v  = _setFoundNodes v

  [<CLIEvent>]
  member this.FoundNodesChanged = _foundNodesChanged.Publish

  member this.SearchAsync(prevText, newText) = _searchAsync prevText newText

  member this.BrowseDir(nodeId) = _browseDir nodeId

  member this.Dict
    with get () = _dict
    and  set v  = _dict <- v
