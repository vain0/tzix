namespace Tzix.Model

open System
open System.IO
open Dyxi.Util

module internal FileNode =
  let create (name: string) parentIdOpt =
    {
      Id              = createId ()
      ParentId        = parentIdOpt
      Name            = name
      Priority        = 0
    }

  let enumFromDirectory: option<Id> -> DirectoryInfo -> list<FileNode> =
    let rec walk acc parentId (dir: DirectoryInfo) =
      let node        = create dir.Name parentId
      let nodeId      = Some node.Id
      let subfiles    = dir.GetFiles("*.*")
      let subdirs     = dir.GetDirectories()
      let acc         =
        (subfiles |> Array.map (fun file -> create file.Name nodeId) |> Array.toList)
        @ acc
      in 
        acc |> fold' subdirs (fun dir acc -> walk acc nodeId dir)
    in
      walk []
