namespace Tzix.ViewModel

open System.Collections.ObjectModel
open Basis.Core
open Tzix.Model

module FileNodeViewModel =
  let ofFileNode dict node =
    {
      FileNodeId            = node.Id
      ShortName             = node |> FileNode.shortName dict
    }
