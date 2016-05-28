namespace Tzix.ViewModel

open Tzix.Model

[<AutoOpen>]
module Types =
  // The indexes of the pages of the tab control in the main window.
  [<RequireQualifiedAccess>]
  type TabPageIndex =
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
