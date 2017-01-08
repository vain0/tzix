namespace VainZero.Tzix.Desktop.Core

open System.Collections.ObjectModel
open Basis.Core
open VainZero.Tzix.Core

module FileNodeViewModel =
  let ofFileNode dict node =
    {
      FileNodeId            = node.Id
      Name                  = node.Name
      ShortName             = node |> FileNode.shortName dict
    }
