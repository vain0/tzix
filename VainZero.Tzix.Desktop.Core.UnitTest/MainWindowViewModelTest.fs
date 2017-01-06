namespace VainZero.Tzix.Desktop.Core.UnitTest

open Persimmon
open Persimmon.Syntax.UseTestNameByReflection
open VainZero.Tzix.Core
open VainZero.Tzix.Core.UnitTest.DictTest
open VainZero.Tzix.Core.UnitTest.DictTest.TestData
open VainZero.Tzix.Desktop.Core

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
