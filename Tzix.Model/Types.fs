namespace Tzix.Model

open System

[<AutoOpen>]
module Types =
  type FileNode =
    {
      Name                : string
      Children            : Map<string, FileNode>
      mutable Priority    : int
    }

  type Dict =
    {
      Roots               : list<FileNode>
    }
