﻿namespace VainZero.Tzix.Core.UnitTest

open System.IO
open Persimmon
open Persimmon.Syntax.UseTestNameByReflection
open VainZero.Tzix.Core
open VainZero.Tzix.Core.UnitTest.MockFileSystem
open VainZero.Tzix.Core.UnitTest.MockFileSystem.TestData
open VainZero.Tzix.Core.UnitTest.ImportRuleTest.TestData
open VainZero.Tzix.Core.UnitTest.DictTest
open VainZero.Tzix.Core.UnitTest.DictTest.TestData

module FileNodeTest =
  let excludesTest =
    let dir = fsys.Roots.[0]
    parameterize {
      case (MockFile("normal_file.txt", dir) :> IFileBase, false)
      case (MockFile(".gitignore", dir) :> IFileBase, true)
      case (MockFile("~tmp", dir) :> IFileBase, true)
      case (MockDirectory(".git", None) :> IFileBase, true)
      case (MockFile("hidden_file", dir, FileAttributes.Hidden) :> IFileBase, false)
      case (MockFile("t_file", dir, FileAttributes.Temporary) :> IFileBase, true)
      run (FileNode.excludes rule |> Persimmon.functionResultEqualityTest)
    }

  let enumSubfilesTest =
    let f dir =
      dir |> FileNode.enumSubfiles rule
      |> (fun (files, dirs) ->
          (files |> Array.map (fun file -> file.Name)
          , dirs |> Array.map (fun dir -> dir.Name)))
    parameterize {
      case (FileSystem.c, ([||], [|"user"|]))
      case (FileSystem.d, ([|"todo.txt"|], [|"repo"|]))  // .git is ignored
      run (f |> Persimmon.functionResultEqualityTest)
    }

  let enumFromDirectoryTest = ()

  let ancestorsTest =
    test {
      let node        = theDict |> tryFirst "VainZero.Tzix.Core" |> Option.get
      let actual      = node |> FileNode.ancestors theDict |> List.map (fun node -> node.Name)
      do! actual |> assertEquals ["D"; "repo"; "tzix"; "VainZero.Tzix.Core"]
    }

  let fullPathTest =
    test {
      let node        = theDict |> tryFirst "VainZero.Tzix.Core" |> Option.get
      let actual      = node |> FileNode.fullPath theDict
      let expected    = Path.Combine("D", "repo", "tzix", "VainZero.Tzix.Core")
      do! actual |> assertEquals expected
    }

  let shortNameTest =
    let body (name, expectedNames) =
      test {
        let node      = theDict |> tryFirst name |> Option.get
        let actual    = node |> FileNode.shortName theDict
        let expected  = Path.Combine(expectedNames |> List.toArray)
        do! actual |> assertEquals expected
      }
    parameterize {
      case ("D", ["D"])
      case ("VainZero.Tzix.Core", ["D"; "repo"; "tzix"; "VainZero.Tzix.Core"])
      case ("tzix.exe", ["D"; "repo"; "..."; "Debug"; "tzix.exe"])
      run body
    }
