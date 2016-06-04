[<AutoOpen>]
module Tzix.Model.FileSystem

open System.IO
open System.Text
open System.Threading.Tasks
open Basis.Core

type IFileBase =
  abstract member Name: string
  abstract member Parent: option<IDirectory>
  abstract member Attributes: FileAttributes
  abstract member Exists: bool
  abstract member Create: unit -> unit
  abstract member Delete: unit -> unit

and IFile =
  inherit IFileBase

  abstract member ReadTextAsync: unit -> Async<string>
  abstract member WriteTextAsync: string -> Async<unit>

and IDirectory =
  inherit IFileBase
  
  abstract member GetFiles: unit -> IFile []
  abstract member GetDirectories: unit -> IDirectory []

  abstract member AddFiles: array<IFile> -> unit
  abstract member AddDirectories: array<IDirectory> -> unit

type IFileSystem =
  abstract member FileInfo: string -> IFile
  abstract member DirectoryInfo: string -> IDirectory

type DotNetFileInfo(_file: FileInfo) =
  new (path: string) =
    DotNetFileInfo(FileInfo(path))

  interface IFile with
    member this.Name = _file.Name

    member this.Parent =
      (DotNetDirectoryInfo(_file.Directory) :> IDirectory) |> Some

    member this.Attributes = _file.Attributes

    member this.Exists = _file.Exists

    member this.Create() =
      use stream = _file.Create() in ()

    member this.Delete() =
      _file.Delete()

    member this.ReadTextAsync() =
      async {
        let stream = _file.OpenText()
        let! text = stream.ReadToEndAsync() |> Async.AwaitTask
        do stream.Dispose()
        return text
      }

    member this.WriteTextAsync(text) =
      async {
        let stream = _file.OpenWrite()
        do! stream.AsyncWrite(UTF8Encoding.UTF8.GetBytes(text))
        do stream.Dispose()
      }

and DotNetDirectoryInfo(_dir: DirectoryInfo) =
  new (path: string) =
    DotNetDirectoryInfo(DirectoryInfo(path))

  interface IDirectory with
    member this.Name = _dir.Name

    member this.Parent =
      _dir.Parent |> Option.ofObj
      |> Option.map (fun dir -> DotNetDirectoryInfo(dir) :> IDirectory)

    member this.Attributes = _dir.Attributes

    member this.Exists = _dir.Exists

    member this.Delete() =
      _dir.Delete()

    member this.Create() =
      _dir.Create()

    member this.GetFiles() =
      _dir.GetFiles()
      |> Array.map (fun file -> DotNetFileInfo(file) :> IFile)

    member this.GetDirectories() =
      _dir.GetDirectories()
      |> Array.map (fun dir -> DotNetDirectoryInfo(dir) :> IDirectory)

    member this.AddFiles(files) = ()

    member this.AddDirectories(dirs) = ()

type DotNetFileSystem private() =
  static member val private LazyInstance = lazy DotNetFileSystem()
  static member Instance = DotNetFileSystem.LazyInstance.Value

  interface IFileSystem with
    member this.FileInfo(path) =
      DotNetFileInfo(path) :> IFile

    member this.DirectoryInfo(path) =
      DotNetDirectoryInfo(path) :> IDirectory

module Directory =
  /// Enumerates ancestors of the directory, excluding itself.
  let ancestors: IDirectory -> list<IDirectory> =
    let rec loop acc (dir: IDirectory) =
      match dir.Parent with
      | None -> acc
      | Some parent -> loop (parent :: acc) parent
    in loop []

  let getAllFilesIfAble (dir: IDirectory) =
    try
      dir.GetFiles()
    with
    | _ -> [||]

  let getAllDirectoriesIfAble (dir: IDirectory) =
    try
      dir.GetDirectories()
    with
    | _ -> [||]

  let tryFindFile name dir =
    dir |> getAllFilesIfAble |> Array.tryFind (fun file -> file.Name = name)

  let tryFindDirectory name dir =
    dir |> getAllDirectoriesIfAble |> Array.tryFind (fun dir -> dir.Name = name)

module File =
  let fullName (file: IFileBase) =
    let ancestors =
      file.Parent
      |> Option.map (fun dir ->
          (dir |> Directory.ancestors) @ [dir]
          |> List.map (fun dir -> dir :> IFileBase)
          )
      |> Option.getOr []
    let names =
      (ancestors |> List.map (fun file -> file.Name)) @ [file.Name]
      |> List.toArray
    in Path.Combine(names)
