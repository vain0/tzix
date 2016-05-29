namespace Tzix.Model

open System
open System.IO
open System.Text.RegularExpressions

[<AutoOpen>]
module Types =
  type Priority = int

  type ImportRule =
    {
      Roots               : list<DirectoryInfo>
      Exclusions          : list<Regex>
    }

  type FileNode =
    {
      Id                  : Id
      ParentId            : option<Id>
      Name                : string
      Priority            : Priority
    }

  type Dict =
    {
      Counter             : Counter
      FileNodes           : Map<Id, FileNode>
      Subfiles            : MultiMap<Id, Id>
      PriorityIndex       : MultiMap<Priority, Id>
      ImportRule          : ImportRule
    }

  type DictSpec =
    {
      NextId              : Id
      Nodes               : array<FileNode>
    }
