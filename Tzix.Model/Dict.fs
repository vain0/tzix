namespace Tzix.Model

open System.IO
open Basis.Core

module Dict =
  let createForDebug () =
    let roots =
      [
        @"D:/repo/tzix"
      ]
      |> List.map (fun path -> DirectoryInfo(path) |> FileNode.createFromDirectory)
    {
      Roots     = roots
    }

  let findInfix word dict =
    let rec f node =
      let acc =
        node.Children |> Seq.collect (fun (KeyValue (_, node')) -> f node')
      in
        if node.Name |> Str.contains word
        then Seq.append (Seq.singleton node) acc
        else acc
    in
      dict.Roots |> Seq.collect f
