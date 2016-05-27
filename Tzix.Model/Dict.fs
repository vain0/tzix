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
    }

  let findNode nodeId (dict: Dict) =
    dict.FileNodes |> Map.find nodeId

  let addNode node dict =
    let dict          = { dict with FileNodes = dict.FileNodes |> Map.add node.Id node }
    match node.ParentId with
    | Some parentId -> { dict with Subfiles = dict.Subfiles |> MultiMap.add parentId node.Id }
    | None          -> dict

  let addNodes nodes dict =
    dict |> fold' nodes addNode

  let importDirectory rule dir dict =
    let parents       = dir |> FileNode.enumParents dict
    if parents |> List.exists (fun dir -> rule |> ImportRule.excludes dir.Name) then
      dict
    else
      let excludes (file: FileSystemInfo) =
        rule |> ImportRule.excludes file.Name
      let parentId      = parents |> List.tryLast |> Option.map (fun node -> node.Id)
      let files         = dir |> FileNode.enumFromDirectory dict excludes parentId
      in
        dict |> addNodes (parents @ files)

  let incrementPriority node dict =
    let node          = { node with Priority = node.Priority + 1 }
    let dict          = { dict with FileNodes = dict.FileNodes |> Map.add node.Id node }
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

  let import (file: FileInfo) =
    let text        = File.ReadAllText(file.FullName)
    let rule        = ImportRule.parse file.Name text
    in
      empty |> fold' rule.Roots (importDirectory rule)

  let tryLoad (dictFile: FileInfo) (importRuleFile: FileInfo) =
    try
      File.ReadAllText(dictFile.FullName) |> ofJson |> pass
    with | e1 ->
      try
        import importRuleFile |> pass
      with | e2 ->
        Result.Bad [e1; e2]

  let findInfix word dict =
    dict.FileNodes |> Seq.choose (fun (KeyValue (_, node)) ->
      if node.Name |> Str.contains word
      then Some node
      else None
      )
