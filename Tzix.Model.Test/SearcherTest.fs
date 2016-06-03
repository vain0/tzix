namespace Tzix.Model.Test

open Tzix.Model
open Tzix.Model.Test.DictTest.TestData
open Persimmon
open Persimmon.Syntax.UseTestNameByReflection

module SearcherTest =
  let searcher () =
    Searcher(theDict)

  let runSearchTest cases =
    let searcher      = searcher ()
    let prevWord      = ref ""
    let tests         = ref ([]: list<TestCase<unit>>)
    // Test runner.
    let search word expected =
      let onChanged e =
        test {
          let actual = searcher.FoundNodes |> Seq.map (fun node -> node.Name) |> Seq.toList
          do! actual |> assertEquals expected
        }
        |> (fun testCase -> tests := testCase :: (! tests))
      use unsubscriber =
        searcher.FoundNodesChanged.Subscribe(onChanged)
      searcher.SearchAsync(! prevWord, word) |> Async.RunSynchronously
      prevWord := word
    let () =
      cases searcher search
    in
      ! tests

  let searchAsyncTestSimple =
    runSearchTest (fun _ search ->
      search "" []
      search "zix" ["tzix"; "Tzix.Model"; "Tzix.View"; "tzix.exe"]
      )

  let searchAsyncTestComplex =
    runSearchTest (fun _ search ->
      search "Tzix" ["Tzix.Model"; "Tzix.View"]
      search "Tzix.M" ["Tzix.Model"]
      search "Tzix." ["Tzix.Model"; "Tzix.View"]
      )

  let browseDirTest =
    runSearchTest (fun searcher search ->
      let node      = searcher.Dict |> DictTest.tryFirst "tzix" |> Option.get
      do searcher.BrowseDir(node.Id)
      search "zix" ["Tzix.Model"; "Tzix.View"] 
      search "" [] // Stops browsing
      search "zix" ["tzix"; "Tzix.Model"; "Tzix.View"; "tzix.exe"]
      )
