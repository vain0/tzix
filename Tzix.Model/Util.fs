namespace Tzix.Model

open Dyxi.Util

[<AutoOpen>]
module Misc =
  let createId: unit -> int =
    let r = ref 0
    fun () ->
      (! r) |> tap (fun k -> r := (k + 1))
