namespace Tzix.Model.Test

open System.Text.RegularExpressions
open Persimmon
open Persimmon.Syntax.UseTestNameByReflection
open Tzix.Model

module DictTest =
  module TestData =
    open Tzix.Model.Test.MockFileSystem.TestData
    open Tzix.Model.Test.ImportRuleTest.TestData

    let emptyDict =
      Dict.empty

    let theDict =
      Dict.empty fsys |> Dict.import rule

  open TestData

  let tryFirst word dict =
    dict |> Dict.findInfix word |> Seq.collect id |> Seq.tryHead

  //// Add new node named `name` under the node with name `parentName`.
  let addNewNode parentName name dict =
    let parentNodeOpt =
      dict |> tryFirst parentName |> Option.map (fun node -> node.Id)
    let newNode =
      FileNode.create dict name parentNodeOpt
    in
      (dict |> Dict.addNode newNode, newNode)

  let assertDictEquals (expected: Dict) (actual: Dict) =
    test {
      do! actual.FileNodes    |> assertEquals expected.FileNodes
      do! actual.Subfiles     |> assertEquals expected.Subfiles
    }

  let importDirectoryTest =
    test {
      let name        = "tzix.exe"
      let node        = theDict |> tryFirst name |> Option.map (fun node -> node.Name)
      do! node |> assertEquals (Some name)
    }

  let findInfixTest =
    let f word =
      theDict |> Dict.findInfix word
      |> Seq.collect id
      |> Seq.map (fun node -> node.Name)
      |> Set.ofSeq
    parameterize {
      case (".git", set [])
      case ("D", set ["D"; "Debug"])
      case ("zix", set ["tzix"; "Tzix.Model"; "Tzix.View"; "tzix.exe"])
      run (f |> Persimmon.functionResultEqualityTest)
    }

  let addNodeTest =
    let body parentName =
      test {
        let name              = "new_file"
        let (dict', node)     = theDict |> addNewNode parentName name
        do! (dict' |> tryFirst name) |> assertEquals (Some node)
      }
    parameterize {
      case "Tzix.Model"
      case "NO PARENT"
      run body
    }

  let removeNodeTest =
    test {
      let node        = theDict |> tryFirst "Tzix.Model" |> Option.get
      let dict'       = theDict |> Dict.removeNode node.Id
      let actual      = dict' |> tryFirst node.Name
      do! actual |> assertEquals None
    }

  let incrementPriorityTest =
    test {
      let name        = "tzix.exe"
      let node        = theDict |> tryFirst name |> Option.get
      let dict'       = theDict |> Dict.incrementPriority node
      let node'       = dict' |> tryFirst name
      // The priority should change.
      do! node' |> Option.map (fun node -> node.Priority) |> assertEquals (Some 1)
      // Nodes with high priority are priorized listing.
      do! dict' |> tryFirst "." |> assertEquals node'
    }

  let ``ofSpec and toSpec test`` =
    test {
      let actual      = theDict |> Dict.toSpec |> Dict.ofSpec theDict.FileSystem
      do! actual |> assertDictEquals theDict
    }

  let ``ofJson and toJson test`` =
    test {
      let actual    = theDict |> Dict.toJson |> Dict.ofJson theDict.FileSystem
      do! actual |> assertDictEquals theDict
    }

  let selectDirectoryNodeTest =
    test {
      let node      = theDict |> tryFirst "tzix" |> Option.get
      let dict'     =
        theDict
        |> addNewNode "tzix" "Tzix.Model.Test" |> fst
        |> Dict.removeNode (theDict |> tryFirst "Tzix.View" |> Option.get).Id
      let (dict'', actualSubnodeIds) =
        dict' |> Dict.selectDirectoryNode node.Id
      // Nodes that actually exists under the directory added.
      do! dict'' |> tryFirst "Tzix.View" |> Option.isSome |> assertPred
      // Nodes that actually don't exists under the directory removed.
      do! dict'' |> tryFirst "Tzix.Model.Test" |> Option.isNone |> assertPred
      // Enumerates subfiles of the directory.
      let subnodeNames =
        actualSubnodeIds
        |> Set.map (fun nodeId -> (dict'' |> Dict.findNode nodeId).Name)
      do! subnodeNames |> assertEquals (set ["Tzix.Model"; "Tzix.View"])
    }
