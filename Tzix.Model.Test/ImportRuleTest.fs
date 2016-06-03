namespace Tzix.Model.Test

open System.Text.RegularExpressions
open Persimmon
open Persimmon.Syntax.UseTestNameByReflection
open Tzix.Model

module ImportRuleTest =
  open MockFileSystem.TestData

  module TestData =
    let private ruleSource =
      """
# includes
+ C
+ D

# excludes
- ^\.
- ^~
    """

    let rule = ruleSource |> ImportRule.parse fsys "test"

  open TestData

  let parseTest = test {
    do! rule.Roots |> List.map (fun dir -> dir.Name) |> assertEquals ["C"; "D"]
    do! rule.Exclusions |> List.map string |> assertEquals ["^\\."; "^~"]
  }
