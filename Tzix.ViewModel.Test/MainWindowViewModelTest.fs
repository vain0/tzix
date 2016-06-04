namespace Tzix.ViewModel.Test

open Persimmon
open Persimmon.Syntax.UseTestNameByReflection
open Tzix.Model
open Tzix.Model.Test.DictTest
open Tzix.Model.Test.DictTest.TestData
open Tzix.ViewModel

module MainWindowViewModelTest =
  type MockDispatcher () =
    interface IDispatcher with
      member this.Invoke(f) = f.Invoke()

  let mainWindow () =
    MainWindowViewModel(theDict.Environment, validDictFile, validRuleFile, MockDispatcher())

  let transStateAsyncTest =
    test {
      let mainWindow      = mainWindow ()
      let task =
        async {
          do! Async.Sleep(20)
          return! mainWindow.TransStateAsync(AppState.Loading)
        } |> Async.StartAsTask
      do! mainWindow.PageIndex |> assertEquals PageIndex.MessageView
      task.Wait()
      do! mainWindow.PageIndex |> assertEquals PageIndex.SearchControl
    }
