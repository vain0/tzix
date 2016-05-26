namespace Tzix.Model

open System.IO
open Basis.Core
open Dyxi.Util

module Dict =
  let empty =
    {
      FileNodes           = Map.empty
      Subfiles            = MultiMap.empty
      Roots               = Set.empty
    }

  let addNode node dict =
    let dict          = { dict with FileNodes = dict.FileNodes |> Map.add node.Id node }
    match node.ParentId with
    | Some parentId -> { dict with Subfiles = dict.Subfiles |> MultiMap.add parentId node.Id }
    | None          -> dict

  let addNodes nodes dict =
    dict |> fold' nodes addNode

  let importDirectory dir dict =
    dict |> addNodes (FileNode.enumFromDirectory None dir)

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
