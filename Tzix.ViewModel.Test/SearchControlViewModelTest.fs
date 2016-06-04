namespace Tzix.ViewModel.Test

open Persimmon
open Persimmon.Syntax.UseTestNameByReflection
open Tzix.Model.Test.DictTest.TestData
open Tzix.ViewModel

module SearchControlViewModelTest =
  module TestData =
    let searchControl () = SearchControlViewModel(theDict)
    

