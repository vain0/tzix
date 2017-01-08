﻿namespace VainZero.Tzix.Core.UnitTest.MockFileSystem

open System.IO
open Persimmon
open Persimmon.Syntax.UseTestNameByReflection
open VainZero.Tzix.Core
open VainZero.Tzix.Core.UnitTest

module TestData =
  /// This data is used all over VainZero.Tzix.Core.UnitTest.
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
            VainZero.Tzix.Core {}
            VainZero.Tzix.Desktop {
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
