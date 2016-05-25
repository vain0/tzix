namespace Tzix.Model

open System
open System.ComponentModel.DataAnnotations
open System.ComponentModel.DataAnnotations.Schema

module Types =
  type Id = int64

  type FileIcon() =
    [<Key>]
    member val Id = 0L with get, set

    [<Index(IsUnique = true)>]
    member val Extension = "" with get, set

    member val IconImage = (null: byte[]) with get, set
  
  type FileNode() =
    [<Key>]
    member val Id = 0L with get, set

    [<Index>]
    member val ParentId = 0L with get, set

    [<Index>]
    member val Name = "" with get, set

    member val IconId = (Nullable(): Nullable<Id>) with get, set
