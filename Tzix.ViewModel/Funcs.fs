namespace Tzix.ViewModel

open Tzix.Model

module FileNodeViewModel =
  let ofFileNode dict node =
    {
      FileNodeId            = node.Id
      ShortName             = node |> FileNode.shortName dict
    }
