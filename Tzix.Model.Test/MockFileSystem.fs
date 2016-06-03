namespace Tzix.Model.Test.MockFileSystem

open System
open System.IO
open Basis.Core
open Persimmon
open Persimmon.Syntax.UseTestNameByReflection
open Tzix.Model
open Tzix.Model.Test

type MockFile(_name: string, _parent: option<IDirectory>, _attributes: FileAttributes) =
  let mutable _exists = true

  new (name, parent) =
    MockFile(name, parent, FileAttributes.Normal)

  member internal this.GetAttributes = _attributes

  member this.Delete() =
    _exists <- false

  interface IFile with
    member this.Name = _name
    
    member this.Parent = _parent

    member this.Attributes = _attributes

    member this.Exists = _exists

and MockDirectory
  ( _name: string
  , _parent: option<IDirectory>
  , _subfiles: array<IFile>
  , _subdirs: array<IDirectory>
  , _attributes: FileAttributes
  ) =
  inherit MockFile(_name, _parent, _attributes)

  let mutable _subfiles = _subfiles
  let mutable _subdirs = _subdirs

  new (name, parent) =
    MockDirectory(name, parent, [||], [||])

  new (name, parent, subfiles, subdirs) =
    MockDirectory(name, parent, subfiles, subdirs, FileAttributes.Normal)

  interface IDirectory with
    member this.GetFiles() = _subfiles
    member this.GetDirectories() = _subdirs

    override this.Attributes =
      this.GetAttributes ||| FileAttributes.Directory

  member this.AddFiles(files) =
    _subfiles <- Array.append _subfiles files

  member this.AddDirectories(dirs) =
    _subdirs <- Array.append _subdirs dirs

type MockFileSystem(_roots: array<IDirectory>) =
  interface IFileSystem with
    /// Example: @"R\path\to\dir"
    member this.DirectoryInfo(path) =
      let rec loop dir =
        function
        | [] | [""] ->
            dir
        | name :: path ->
            let dir =
              dir |> Directory.tryFindDirectory name
              |> Option.getOrElse (fun () ->
                  let dir = MockDirectory(name, Some dir)
                  dir.Delete()
                  dir :> IDirectory
                  )
            in loop dir path
      match path.Split(Path.DirectorySeparatorChar) |> Array.toList with
      | [] -> raise (ArgumentException())
      | rootName :: path ->
          let root = _roots |> Array.find (fun dir -> dir.Name = rootName)
          in loop root path

    member this.FileInfo(path) =
      let dir = (this :> IFileSystem).DirectoryInfo(Path.GetDirectoryName(path))
      let name = Path.GetFileName(path)
      match dir |> Directory.tryFindFile name with
      | Some file -> file
      | None ->
          let file = MockFile(name, Some dir)
          file.Delete()
          file :> IFile

  member this.Roots = _roots

module Parser =
  open FParsec
  open FParsec.CharParsers
  open FParsec.Primitives

  type FileOrDirectory =
    | File          of IFile
    | Dir           of IDirectory

  let (fileList: Parser<array<IFile> * array<IDirectory>, _>, fileListRef) =
    createParserForwardedToRef ()

  let fileName =
    many1Chars (noneOf " \t\n/\\{}")

  let nondirectoryFile =
    parse {
      let! name = fileName
      let! parentOpt = getUserState
      return (MockFile(name, parentOpt) :> IFile) |> File
    }

  let directory =
    parse {
      let! name = fileName
      let! parentOpt = getUserState
      do! spaces >>. skipChar '{' .>> spaces
      let dir = MockDirectory(name, parentOpt)
      do! setUserState (Some (dir :> IDirectory))
      let! (subfiles, subdirs) = fileList
      do! spaces >>. skipChar '}'
      do! setUserState parentOpt
      do dir.AddFiles(subfiles)
      do dir.AddDirectories(subdirs)
      return Dir dir
    }

  let file =
    (attempt directory)
    <|> nondirectoryFile

  fileListRef :=
    sepEndBy file spaces
    |>> (fun files ->
        let subfiles = files |> List.choose (function (File file) -> Some file | _ -> None)
        let subdirs  = files |> List.choose (function (Dir dir) -> Some dir | _ -> None)
        in (subfiles |> List.toArray, subdirs |> List.toArray)
        )

  let fileSystem =
    spaces >>. fileList .>> spaces .>> eof
    |>> (fun (_, roots) -> MockFileSystem(roots))

  let parse name source =
    match runParserOnString fileSystem None name source with
    | Success (fsys, _, _) ->
        fsys
    | Failure (msg, _, _) ->
        failwith msg

module TestData =
  /// This data is used all over Tzix.Model.Test.
  let private fsysSource =
    """
      C { user { .gitconfig } }
      D {
        .git {
          config
          HEAD
        }
        repo {
          tzix {
            Tzix.Model {}
            Tzix.View {
              bin { Debug { tzix.exe ~x } }
            }
          }
        }
        todo.txt
      }
    """

  let fsys =
    fsysSource |> Parser.parse "testData"

  let (./) (dir: IDirectory) (name: string) =
    dir |> Directory.tryFindDirectory name |> Option.get

  let (/.) (dir: IDirectory) (name: string) =
    dir |> Directory.tryFindFile name |> Option.get

  module FileSystem =
    let c = fsys.Roots |> Array.find (fun dir -> dir.Name = "C")
    let d = fsys.Roots |> Array.find (fun dir -> dir.Name = "D")

module MockFileSystemTest =
  open TestData

  let parseTest = test {
    do! fsys.Roots |> Array.map (fun dir -> dir.Name) |> assertEquals [|"C"; "D"|]
  }

module FileSystemTest =
  open TestData

  let ancestorsTest =
    let f dir =
      dir |> Directory.ancestors
      |> List.map (fun dir -> dir.Name)
      |> List.toArray
      |> Path.Combine
    parameterize {
      case (FileSystem.d ./ "repo" ./ "tzix", Path.Combine("D", "repo"))
      run (f |> Persimmon.functionResultEqualityTest)
    }

  let fullNameTest =
    test {
      let file = FileSystem.d /. "todo.txt"
      do! (file |> File.fullName) |> assertEquals (Path.Combine("D", "todo.txt"))
    }
