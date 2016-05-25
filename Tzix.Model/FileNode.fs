namespace Tzix.Model

open System
open System.IO

module FileNode =
  let ofFile (name: string) =
    {
      Name            = name
      Children        = Map.empty
      Priority        = 0
    }

  let ofDirectory (name: string) (children: seq<FileNode>) =
    {
      Name            = name
      Children        = children |> Seq.map (fun n -> (n.Name, n)) |> Map.ofSeq
      Priority        = 0
    }

  let createFromDirectory: DirectoryInfo -> FileNode =
    let rec walk (dir: DirectoryInfo) =
      let files =
        dir.GetFiles("*.*") |> Array.map (fun file -> file.Name |> ofFile)
      let subdirs =
        dir.GetDirectories() |> Array.map walk
      in
        ofDirectory dir.Name (Array.append subdirs files)
    in walk
