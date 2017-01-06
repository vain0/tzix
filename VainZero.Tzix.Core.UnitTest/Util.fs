namespace VainZero.Tzix.Core.UnitTest

open Persimmon
open Persimmon.Syntax.UseTestNameByReflection

module Persimmon =
  /// Tests that ``f x`` should equal to `expected`.
  let functionResultEqualityTest f (x, expected) =
    test {
      do! (f x) |> assertEquals expected
    }
