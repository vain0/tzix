namespace Tzix.Model

open System
open System.IO
open Dyxi.Util

module FileNode =
  let create (dict: Dict) (name: string) parentIdOpt =
    {
      Id              = dict.Counter ()
      ParentId        = parentIdOpt
      Name            = name
      Priority        = 0
    }

  let excludes rule (file: IFile) =
    [
      rule |> ImportRule.excludes file.Name
      file.Attributes.HasFlag(FileAttributes.Temporary)
    ] |> List.exists id

  let enumSubfiles rule (dir: IDirectory) =
    let subfiles    =
      dir |> Directory.getAllFilesIfAble
      |> Array.filter (excludes rule >> not)
    let subdirs     =
      dir |> Directory.getAllDirectoriesIfAble
      |> Array.filter (excludes rule >> not)
    in (subfiles, subdirs)

  /// Enumerates files in a directory recursively and converts them into FileNodes.
  /// Creates an array (instead of a list) for performance.
  let enumFromDirectory
      (dict: Dict)
      : option<Id> -> IDirectory -> array<FileNode>
    =
    let rec walk parentId (dir: IDirectory) =
      [|
        let node        = create dict dir.Name parentId
        let nodeId      = Some node.Id
        let (subfiles, subdirs) =
          enumSubfiles dict.ImportRule dir
        yield node
        yield! subfiles |> Array.map (fun file -> create dict file.Name nodeId)
        yield! subdirs |> Array.collect (walk nodeId)
      |]
    in walk

  /// Enumerates all ancestor directories from the directory, excluding itself,
  /// and converts them into FileNode instances.
  /// Returns None if one of them is excluded.
  let internal enumParents dict dir =
    let parents =
      dir |> File.ancestors
    if parents |> List.exists (excludes dict.ImportRule) then
      None
    else
      let folder parent (dir: IDirectory) =
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
