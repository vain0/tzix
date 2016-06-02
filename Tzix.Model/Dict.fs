﻿namespace Tzix.Model

open System.Diagnostics
open System.IO
open System.Runtime.Serialization.Json
open Basis.Core
open Chessie.ErrorHandling
open Dyxi.Util

module Dict =
  let empty fsys =
    {
      Counter             = createCounter 0L
      FileNodes           = Map.empty
      Subfiles            = MultiMap.empty
      PriorityIndex       = MultiMap.empty
      ImportRule          = ImportRule.empty
      FileSystem          = fsys
    }

  let findNode nodeId (dict: Dict) =
    dict.FileNodes |> Map.find nodeId

  let addNode node dict =
    let dict          = { dict with FileNodes = dict.FileNodes |> Map.add node.Id node }
    let dict =
      match node.ParentId with
      | Some parentId -> { dict with Subfiles = dict.Subfiles |> MultiMap.add parentId node.Id }
      | None          -> dict
    let dict =
      { dict with PriorityIndex = dict.PriorityIndex |> MultiMap.add node.Priority node.Id }
    in dict

  let addNodes nodes dict =
    dict |> fold' nodes addNode

  let removeNode nodeId dict =
    { dict with FileNodes = dict.FileNodes |> Map.remove nodeId }

  let importDirectory dir dict =
    match dir |> FileNode.enumParents dict with
    | None -> dict
    | Some parents ->
      let parentId      = parents |> List.tryLast |> Option.map (fun node -> node.Id)
      let files         = dir |> FileNode.enumFromDirectory dict parentId
      in
        dict |> addNodes (parents @ files)

  let import rule dict =
    { dict with ImportRule = rule }
    |> fold' rule.Roots importDirectory

  let incrementPriority node dict =
    let node'         = { node with Priority = node.Priority + 1 }
    let dict          = { dict with FileNodes = dict.FileNodes |> Map.add node.Id node' }
    let priorityIndex =
      dict.PriorityIndex
      |> MultiMap.removeOne node.Priority node.Id
      |> MultiMap.add node'.Priority node.Id
    let dict          = { dict with PriorityIndex = priorityIndex }
    in dict

  let tryExecute node dict =
    let path          = node |> FileNode.fullPath dict
    try
      Process.Start(path) |> ignore
      dict |> incrementPriority node |> pass
    with
    | e -> fail e

  let toSpec (dict: Dict) =
    {
      NextId          = dict.Counter ()
      Nodes           = dict.FileNodes |> Map.values |> Seq.toArray
    }

  let ofSpec fsys (spec: DictSpec) =
    let dict = empty fsys |> addNodes spec.Nodes
    in { dict with Counter = createCounter spec.NextId }

  let toJson dict =
    dict |> toSpec |> Serialize.Json.serialize<DictSpec>

  let ofJson fsys (json: string) =
    json |> Serialize.Json.deserialize<DictSpec> |> ofSpec fsys

  let loadImportRule fsys file =
    async {
      let! text = file |> FileInfo.readTextAsync
      return ImportRule.parse fsys file.Name text
    }

  /// Tries to load dictionary from `dictFile`.
  /// If it fails, creates new dictionary based on the rule written in `importRuleFile`.
  let tryLoadAsync (dictFile: FileInfo) (importRuleFile: FileInfo) =
    async {
      let fsys = DotNetFileSystem.Instance
      try
        let! rule     = loadImportRule fsys importRuleFile
        let! jsonText = dictFile |> FileInfo.readTextAsync
        let dict      = jsonText |> ofJson fsys
        let dict      = { dict with ImportRule = rule }
        return dict |> pass
      with | e1 ->
        try
          let! rule = loadImportRule fsys importRuleFile
          let dict = empty fsys |> import rule
          return dict |> pass
        with | e2 ->
          return Result.Bad [e1; e2]
    }

  let findInfix word dict =
    let find nodeIds =
      nodeIds |> Seq.choose (fun nodeId ->
        let node = dict |> findNode nodeId
        if node.Name |> Str.contains word
        then Some node
        else None
        )
    in
      dict.PriorityIndex
      |> MultiMap.toMap
      |> Seq.rev
      |> Seq.map (fun (KeyValue (_, nodeIds)) ->
          find nodeIds
          )

  /// ディレクトリに注目した状態になる。
  /// ディレクトリの直下にある各ファイルと、その子ノードを照合して不整合を正す。
  /// 実在するノードのリストを優先度降順で返す。
  let selectDirectoryNode nodeId dict =
    let node = dict |> findNode nodeId
    let dir =
      dict.FileSystem.DirectoryInfo(node |> FileNode.fullPath dict)
    /// Subfiles and subdirs actually exist inside the directory.
    let (subfiles, subdirs) =
      dir |> FileNode.enumSubfiles dict.ImportRule
    /// A map from file names to node id's.
    let subnodes =
      dict.Subfiles
      |> MultiMap.findAll nodeId
      |> Seq.map (fun subnodeId ->
          let node = dict |> findNode subnodeId
          in (node.Name, node.Id)
          )
      |> Map.ofSeq
    /// 直下に実在する各ファイルと、ノードIDを対消滅させていき、
    /// 対応していないノードとファイルを列挙する。
    let (unknownSubnodes, unknownSubfiles) =
      Seq.append
        (subfiles |> Seq.cast<FileSystemInfo>)
        (subdirs |> Seq.cast<FileSystemInfo>)
      |> Seq.fold (fun (uns, ufs) file ->
          match uns |> Map.tryFind file.Name with
          | Some nodeId ->
              (uns |> Map.remove file.Name, ufs)
          | None ->
              (uns, file :: ufs)
          ) (subnodes, [])
    let newNodes =
      unknownSubfiles
      |> List.map (fun file -> FileNode.create dict file.Name (Some nodeId))
    // 実在しないノードを削除して、未登録のファイルを登録する。
    let dict =
      dict
      |> fold' (unknownSubnodes |> Map.values) removeNode
      |> fold' newNodes addNode
    let actualSubnodes =
      ( (subnodes |> Map.values |> Set.ofSeq)
      + (newNodes |> List.map (fun node -> node.Id) |> Set.ofList)
      - (unknownSubnodes |> Map.values |> Set.ofSeq)
      )
    let subnodes =
      actualSubnodes
      |> Set.toList
      |> List.sortByDescending (fun nodeId -> (dict |> findNode nodeId).Priority)
    in
      (dict, actualSubnodes)
