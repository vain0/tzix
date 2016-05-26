namespace Tzix.Model

open System.IO
open Dyxi.Util

[<AutoOpen>]
module Misc =
  let createId: unit -> int =
    let r = ref 0
    fun () ->
      (! r) |> tap (fun k -> r := (k + 1))

module DirectoryInfo =
  let parents: DirectoryInfo -> list<DirectoryInfo> =
    let rec loop acc (dir: DirectoryInfo) =
      if dir.Parent = null
      then acc
      else loop (dir.Parent :: acc) dir.Parent
    in loop []
