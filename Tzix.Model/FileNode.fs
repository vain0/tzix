namespace Tzix.Model

open System
open System.IO
open Dyxi.Util

module FileNode =
  let internal create (dict: Dict) (name: string) parentIdOpt =
    {
      Id              = dict.Counter ()
      ParentId        = parentIdOpt
      Name            = name
      Priority        = 0
    }

  let internal enumFromDirectory
      (dict: Dict)
      (excludes: FileSystemInfo -> bool)
      : option<Id> -> DirectoryInfo -> list<FileNode>
    =
    let rec walk acc parentId (dir: DirectoryInfo) =
      if excludes dir
      then acc
      else
        let node        = create dict dir.Name parentId
        let nodeId      = Some node.Id
        let subfiles    = dir.GetFiles() |> Array.filter (excludes >> not)
        let subdirs     = dir.GetDirectories()
        let acc         =
          (subfiles |> Array.map (fun file -> create dict file.Name nodeId) |> Array.toList)
          @ acc
        in 
          (node :: acc) |> fold' subdirs (fun dir acc -> walk acc nodeId dir)
    in
      walk []

  /// Enumerates all ancestor directories from the directory, excluding itself,
  /// and converts them into FileNode instances.
  /// Returns None if one of them is excluded.
  let internal enumParents excludes dict dir =
    let parents =
      dir |> DirectoryInfo.parents
    if parents |> List.exists excludes then
      None
    else
      let folder parent (dir: DirectoryInfo) =
        create dict dir.Name (parent |> Option.map (fun node -> node.Id))
        |> Some
      parents |> List.scan folder None |> List.choose id |> Some

  let ancestors dict node =
    let rec loop acc node =
      let acc' = node :: acc
      match node.ParentId with
      | Some parentId -> dict.FileNodes |> Map.find parentId |> loop acc'
      | None          -> acc'
    in loop [] node

  let fullPath dict node =
    let names =
      ancestors dict node |> List.map (fun node -> node.Name)
    in
      Path.Combine(names |> List.toArray)

  let shortName dict node =
    let names =
      ancestors dict node
      |> List.map (fun node -> node.Name)
      |> List.takeEnds ["..."] 2 2
    in
      Path.Combine(names |> List.toArray)
