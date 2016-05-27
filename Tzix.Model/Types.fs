namespace Tzix.Model

open System

[<AutoOpen>]
module Types =
  type FileNode =
    {
      Id                  : Id
      ParentId            : option<Id>
      Name                : string
      Priority            : int
    }

  type Dict =
    {
      Counter             : Counter
      FileNodes           : Map<Id, FileNode>
      Subfiles            : MultiMap<Id, Id>
    }

  type DictSpec =
    {
      NextId              : Id
      Nodes               : array<FileNode>
    }
