﻿namespace VainZero.Tzix.Core

open System
open System.IO
open System.Text.RegularExpressions

[<AutoOpen>]
module Types =
  type Priority = int

  type Environment =
    {
      FileSystem          : IFileSystem
      Executor            : IExecutor
    }

  type ImportRule =
    {
      Roots               : list<IDirectory>
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
      Environment         : Environment
    }

  type DictSpec =
    {
      NextId              : Id
      Nodes               : array<FileNode>
    }

  [<RequireQualifiedAccess>]
  type SearchSource =
    | All
    /// 選択しているディレクトリのノードID
    /// およびそのディレクトリの直下にあるノードのリスト (優先度降順)
    | Dir         of Id * list<FileNode>
