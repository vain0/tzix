namespace Tzix.ViewModel

open Tzix.Model

[<AutoOpen>]
module Types =
  [<RequireQualifiedAccess>]
  type PageIndex =
    | MessageView              = 0
    | SearchControl            = 1

  [<RequireQualifiedAccess>]
  type AppState =
    | Stuck                    of string
    | Loading
    | Running

  type FileNodeViewModel =
    {
      FileNodeId        : Id
      ShortName         : string
    }

  [<RequireQualifiedAccess>]
  type SearchSource =
    | All
    /// 選択しているディレクトリのノードID
    /// およびそのディレクトリの直下にあるノードのリスト (優先度降順)
    | Dir         of Id * seq<FileNodeViewModel>
