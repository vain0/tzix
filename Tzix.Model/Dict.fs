namespace Tzix.Model

open System.Diagnostics
open System.IO
open Basis.Core
open Dyxi.Util

module Dict =
  let empty =
    {
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

  let importDirectory dir dict =
    let parents       = FileNode.enumParents dir |> List.choose id
    let parentId      = parents |> List.tryLast |> Option.map (fun node -> node.Id)
    let files         = FileNode.enumFromDirectory parentId dir
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

  let createForDebug () =
    let roots =
      [
        @"D:/repo/tzix"
      ]
    in
      empty |> fold' roots (fun path dict -> dict |> importDirectory (DirectoryInfo(path)))

  let findInfix word dict =
    dict.FileNodes |> Seq.choose (fun (KeyValue (_, node)) ->
      if node.Name |> Str.contains word
      then Some node
      else None
      )
