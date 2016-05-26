namespace Tzix.Model

open System
open System.IO
open Dyxi.Util

module FileNode =
  let internal create (name: string) parentIdOpt =
    {
      Id              = createId ()
      ParentId        = parentIdOpt
      Name            = name
      Priority        = 0
    }

  let internal enumFromDirectory: option<Id> -> DirectoryInfo -> list<FileNode> =
    let rec walk acc parentId (dir: DirectoryInfo) =
      let node        = create dir.Name parentId
      let nodeId      = Some node.Id
      let subfiles    = dir.GetFiles("*.*")
      let subdirs     = dir.GetDirectories()
      let acc         =
        (subfiles |> Array.map (fun file -> create file.Name nodeId) |> Array.toList)
        @ acc
      in 
        (node :: acc) |> fold' subdirs (fun dir acc -> walk acc nodeId dir)
    in
      walk []

  let internal enumParents dir =
    let folder parent (dir: DirectoryInfo) =
      create dir.Name (parent |> Option.map (fun node -> node.Id))
      |> Some
    in
      dir |> DirectoryInfo.parents |> List.scan folder None

  let fullPath dict node =
    let rec loop acc node =
      let acc' = node.Name :: acc
      match node.ParentId with
      | Some parentId -> dict.FileNodes |> Map.find parentId |> loop acc'
      | None          -> Path.Combine(acc' |> List.toArray)
    in loop [] node
