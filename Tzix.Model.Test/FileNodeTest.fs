namespace Tzix.Model.Test

open System.IO
open Persimmon
open Persimmon.Syntax.UseTestNameByReflection
open Tzix.Model
open Tzix.Model.Test.MockFileSystem
open Tzix.Model.Test.MockFileSystem.TestData
open Tzix.Model.Test.ImportRuleTest.TestData
open Tzix.Model.Test.DictTest
open Tzix.Model.Test.DictTest.TestData

module FileNodeTest =
  let excludesTest =
    parameterize {
      case (MockFile("normal_file.txt", None) :> IFile, false)
      case (MockFile(".gitignore", None) :> IFile, true)
      case (MockFile("~tmp", None) :> IFile, true)
      case (MockDirectory(".git", None) :> IFile, true)
      case (MockFile("hidden_file", None, FileAttributes.Hidden) :> IFile, false)
      case (MockFile("t_file", None, FileAttributes.Temporary) :> IFile, true)
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
      let node        = theDict |> tryFirst "Tzix.Model" |> Option.get
      let actual      = node |> FileNode.ancestors theDict |> List.map (fun node -> node.Name)
      do! actual |> assertEquals ["D"; "repo"; "tzix"; "Tzix.Model"]
    }

  let fullPathTest =
    test {
      let node        = theDict |> tryFirst "Tzix.Model" |> Option.get
      let actual      = node |> FileNode.fullPath theDict
      let expected    = Path.Combine("D", "repo", "tzix", "Tzix.Model")
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
      case ("Tzix.Model", ["D"; "repo"; "tzix"; "Tzix.Model"])
      case ("tzix.exe", ["D"; "repo"; "..."; "Debug"; "tzix.exe"])
      run body
    }
