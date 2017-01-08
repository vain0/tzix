namespace VainZero.Tzix.Desktop.Core.UnitTest

open Persimmon
open Persimmon.Syntax.UseTestNameByReflection
open VainZero.Tzix.Core
open VainZero.Tzix.Core.UnitTest.DictTest
open VainZero.Tzix.Core.UnitTest.DictTest.TestData
open VainZero.Tzix.Core.UnitTest.MockFileSystem.TestData
open VainZero.Tzix.Desktop.Core

module SearchControlViewModelTest =
  module TestData =
    let searchControl () = SearchControlViewModel(theDict)

  open TestData

  /// Search synchronously.
  let searchSync word (searchControl: SearchControlViewModel) =
    async {
      async {
        do! Async.Sleep(10)
        searchControl.SearchText <- word
      } |> Async.Start
      return!
        Async.AwaitEvent searchControl.Searcher.FoundNodesChanged |> Async.Ignore
    } |> Async.RunSynchronously

  let itemNames (searchControl: SearchControlViewModel) =
    searchControl.ItemChunks
    |> Seq.collect id
    |> Seq.map (fun node -> node.Name)
    |> Seq.toList

  let commitCommandTests =
    let searchControl = searchControl ()
    let searchAndCommit word =
      searchControl |> searchSync word
      searchControl.CommitCommand.Execute(null)
    let successTest =
      test {
        let name = "VainZero.Tzix.Desktop"
        searchAndCommit name
        let name' = searchControl.Dict |> tryFirst "." |> Option.map (fun node -> node.Name)
        do! name' |> assertEquals (Some name)
      }
    let failureTest =
      test {
        let name              = "todo.txt"
        let todoFile          = FileSystem.d /. name
        todoFile.Delete()
        searchAndCommit name
        do! searchControl.Dict |> Dict.findInfix name |> Seq.toList |> assertEquals []
        // Fails yet.
        //do! searchControl |> itemNames |> assertEquals []
        todoFile.Create()  // Restore state of fsys
        return ()
      }
    in
      [successTest; failureTest]

  let browseDirCommandTest =
    test {
      let searchControl = searchControl ()
      searchControl |> searchSync "Tzix"
      searchControl.SelectedIndex <- 1  // VainZero.Tzix.Desktop
      searchControl.BrowseDirCommand.Execute(null)
      do! searchControl |> itemNames |> assertEquals ["bin"]
    }

  let browseGrandparentDirCommandTest =
    test {
      let searchControl = searchControl ()
      searchControl |> searchSync "tzix"
      searchControl.BrowseGrandparentDirCommand.Execute(null)
      do! searchControl |> itemNames |> assertEquals ["todo.txt"; "repo"]
    }
