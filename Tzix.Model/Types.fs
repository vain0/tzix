namespace Tzix.Model

open System

[<AutoOpen>]
module Types =
  type Id = int

  type FileNode =
    {
      Id                  : Id
      ParentId            : option<Id>
      Name                : string
      Priority            : int
    }

  type Dict =
    {
      FileNodes           : Map<Id, FileNode>
      Subfiles            : MultiMap<Id, Id>
    }

  type DictSpec =
    {
      Nodes               : array<FileNode>
    }
