namespace Tzix.ViewModel

open System.Collections.ObjectModel
open Tzix.Model
open Dyxi.Util.Wpf

type FoundListViewModel() =
  inherit ViewModel.Base()

  let mutable _selectedIndex = -1

  let mutable _items =
    (Seq.empty: seq<FileNodeViewModel>)

  let trySelectedItem () =
    _items |> Seq.tryItem _selectedIndex

  member this.ItemChunks
    with get () =
      this.Items |> Seq.chunkBySize 100
    and  set (itemChunks: seq<array<FileNodeViewModel>>) =
      this.Items <- itemChunks |> Seq.collect id
      for name in ["Items"; "ItemChunks"] do
        this.RaisePropertyChanged(name)

  member this.Items
    with get () = _items
    and  set v  =
      _items <- v
      for name in ["Items"; "ItemChunks"] do
        this.RaisePropertyChanged(name)

  member this.SelectedIndex
    with get () = _selectedIndex
    and  set v  =
      _selectedIndex <- v
      for name in ["SelectedIndex"; "TrySelectedItem"] do
        this.RaisePropertyChanged(name)

  member this.TrySelectedItem() =
    trySelectedItem ()

  member this.SelectFirstIfNoSelection() =
    if this.SelectedIndex < 0 && (this.Items |> Seq.isEmpty |> not) then
      this.SelectedIndex <- 0
