namespace Tzix.Model

open System
open System.IO
open System.Text.RegularExpressions

[<AutoOpen>]
module Types =
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
