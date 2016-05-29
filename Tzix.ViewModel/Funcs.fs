namespace Tzix.ViewModel

open System.Collections.ObjectModel
open Basis.Core
open Tzix.Model

module FileNodeViewModel =
  let ofFileNode dict node =
    {
      FileNodeId            = node.Id
      Name                  = node.Name
      ShortName             = node |> FileNode.shortName dict
    }
