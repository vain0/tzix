namespace Tzix.Model

open System.IO
open Dyxi.Util

[<AutoOpen>]
module Misc =
  let createId: unit -> int =
    let r = ref 0
    fun () ->
      (! r) |> tap (fun k -> r := (k + 1))

module List =
  /// Take `h` elements from head and `t` elements from tail
  /// then join the two lists with `infix`.
  /// If ``h + t`` >= the length of the list, just returns the list.
  let takeEnds (infix: list<'x>) (h: int) (t: int) (xs: list<'x>): list<'x> =
    assert (h >= 0 && t >= 0)
    let len = xs |> List.length
    if h + t >= len
    then xs
    else List.take h xs @ infix @ List.skip (len - t) xs

module DirectoryInfo =
  let parents: DirectoryInfo -> list<DirectoryInfo> =
    let rec loop acc (dir: DirectoryInfo) =
      if dir.Parent = null
      then acc
      else loop (dir.Parent :: acc) dir.Parent
    in loop []
