namespace Tzix.Model

open Dyxi.Util

[<AutoOpen>]
module Misc =
  type Id = int64

  type Counter = unit -> Id

  let createCounter (n: Id): Counter =
    let r = ref n
    fun () ->
      (! r) |> tap (fun k -> r := (k + 1L))

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
  open System.IO

  let parents: DirectoryInfo -> list<DirectoryInfo> =
    let rec loop acc (dir: DirectoryInfo) =
      if dir.Parent = null
      then acc
      else loop (dir.Parent :: acc) dir.Parent
    in loop []

module Serialize =
  open System.IO
  open System.Runtime.Serialization.Json
  open System.Text
  
  module Json =
    let serialize<'x> (x: 'x) = 
      let jsonSerializer = new DataContractJsonSerializer(typedefof<'x>)
      use stream = new MemoryStream()
      jsonSerializer.WriteObject(stream, x)
      UTF8Encoding.UTF8.GetString(stream.ToArray())

    let deserialize<'x> (json : string) =
      let jsonSerializer = new DataContractJsonSerializer(typedefof<'x>)
      use stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(json))
      jsonSerializer.ReadObject(stream) :?> 'x
