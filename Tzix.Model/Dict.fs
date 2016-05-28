namespace Tzix.Model

open System.Diagnostics
open System.IO
open System.Runtime.Serialization.Json
open Basis.Core
open Chessie.ErrorHandling
open Dyxi.Util

module Dict =
  let empty =
    {
      Counter             = createCounter 0L
      FileNodes           = Map.empty
      Subfiles            = MultiMap.empty
      PriorityIndex       = MultiMap.empty
    }

  let findNode nodeId (dict: Dict) =
    dict.FileNodes |> Map.find nodeId

  let addNode node dict =
    let dict          = { dict with FileNodes = dict.FileNodes |> Map.add node.Id node }
    let dict =
      match node.ParentId with
      | Some parentId -> { dict with Subfiles = dict.Subfiles |> MultiMap.add parentId node.Id }
      | None          -> dict
    let dict =
      { dict with PriorityIndex = dict.PriorityIndex |> MultiMap.add node.Priority node.Id }
    in dict

  let addNodes nodes dict =
    dict |> fold' nodes addNode

  let importDirectory rule dir dict =
    match dir |> FileNode.enumParents rule dict with
    | None -> dict
    | Some parents ->
      let parentId      = parents |> List.tryLast |> Option.map (fun node -> node.Id)
      let files         = dir |> FileNode.enumFromDirectory dict rule parentId
      in
        dict |> addNodes (parents @ files)

  let incrementPriority node dict =
    let node'         = { node with Priority = node.Priority + 1 }
    let dict          = { dict with FileNodes = dict.FileNodes |> Map.add node.Id node' }
    let priorityIndex =
      dict.PriorityIndex
      |> MultiMap.removeOne node.Priority node.Id
      |> MultiMap.add node'.Priority node.Id
    let dict          = { dict with PriorityIndex = priorityIndex }
    in dict

  let execute node dict =
    let path          = node |> FileNode.fullPath dict
    Process.Start(path) |> ignore
    dict |> incrementPriority node

  let toSpec (dict: Dict) =
    {
      NextId          = dict.Counter ()
      Nodes           = dict.FileNodes |> Map.values |> Seq.toArray
    }

  let ofSpec (spec: DictSpec) =
    let dict = empty |> addNodes spec.Nodes
    in { dict with Counter = createCounter spec.NextId }

  let toJson dict =
    dict |> toSpec |> Serialize.Json.serialize<DictSpec>

  let ofJson (json: string) =
    json |> Serialize.Json.deserialize<DictSpec> |> ofSpec

  let importAsync (file: FileInfo) =
    async {
      let! text       = file |> FileInfo.readTextAsync
      let rule        = ImportRule.parse file.Name text
      return
        empty |> fold' rule.Roots (importDirectory rule)
    }

  let tryLoadAsync (dictFile: FileInfo) (importRuleFile: FileInfo) =
    async {
      try
        let! jsonText = dictFile |> FileInfo.readTextAsync
        return jsonText |> ofJson |> pass
      with | e1 ->
        try
          let! dict = importAsync importRuleFile
          return dict |> pass
        with | e2 ->
          return Result.Bad [e1; e2]
    }

  let findInfix word dict =
    let find nodeIds =
      nodeIds |> Seq.choose (fun nodeId ->
        let node = dict |> findNode nodeId
        if node.Name |> Str.contains word
        then Some node
        else None
        )
    in
      dict.PriorityIndex
      |> MultiMap.toMap
      |> Seq.rev
      |> Seq.map (fun (KeyValue (_, nodeIds)) ->
          find nodeIds
          )
