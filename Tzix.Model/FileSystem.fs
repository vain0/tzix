[<AutoOpen>]
module Tzix.Model.FileSystem

open System.IO

type IFile =
  abstract member Name: string
  abstract member Parent: option<IDirectory>
  abstract member Attributes: FileAttributes

and IDirectory =
  inherit IFile
  
  abstract member GetFiles: unit -> IFile []
  abstract member GetDirectories: unit -> IDirectory []

type MyFileInfo(_file: FileInfo) =
  new (path: string) =
    MyFileInfo(FileInfo(path))

  interface IFile with
    member this.Name = _file.Name

    member this.Parent =
      (MyDirectoryInfo(_file.Directory) :> IDirectory) |> Some

    member this.Attributes = _file.Attributes

and MyDirectoryInfo(_dir: DirectoryInfo) =
  new (path: string) =
    MyDirectoryInfo(DirectoryInfo(path))

  interface IDirectory with
    member this.Name = _dir.Name

    member this.Parent =
      _dir.Parent |> Option.ofObj
      |> Option.map (fun dir -> MyDirectoryInfo(dir) :> IDirectory)

    member this.Attributes = _dir.Attributes

    member this.GetFiles() =
      _dir.GetFiles()
      |> Array.map (fun file -> MyFileInfo(file) :> IFile)

    member this.GetDirectories() =
      _dir.GetDirectories()
      |> Array.map (fun dir -> MyDirectoryInfo(dir) :> IDirectory)

module File =
  let ancestors: IDirectory -> list<IDirectory> =
    let rec loop acc (dir: IDirectory) =
      match dir.Parent with
      | None -> acc
      | Some parent -> loop (parent :: acc) parent
    in loop []

module Directory =
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
