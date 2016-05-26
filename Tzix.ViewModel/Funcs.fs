namespace Tzix.ViewModel

open Tzix.Model

module FileNodeViewModel =
  let ofFileNode dict node =
    {
      FileNodeId            = node.Id
      FullName              = node |> FileNode.fullPath dict
    }
