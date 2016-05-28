namespace Tzix.ViewModel

open System.Collections.ObjectModel
open System.IO
open System.Windows.Threading
open Tzix.Model
open Basis.Core
open Chessie.ErrorHandling
open Dyxi.Util.Wpf

type MainWindowViewModel(dispatcher: Dispatcher) as this =
  inherit ViewModel.Base()
  
  let dictFile = FileInfo(@"tzix.json")
  let importRuleFile = FileInfo(@".tzix_import_rules")

  let mutable _dict =
    Dict.tryLoad dictFile importRuleFile
    |> Trial.returnOrFail

  let _foundListViewModel = FoundListViewModel()

  let mutable _searchText = ""

  let searchIncrementally () =
    async {
      dispatcher.Invoke(fun () ->
        _foundListViewModel.Items <- ObservableCollection()
        )
      let itemListList =
        if _searchText |> Str.isNullOrWhiteSpace
        then Seq.singleton Seq.empty
        else _dict |> Dict.findInfix _searchText
      for items in itemListList do
        let items =
          items
          |> Seq.map (FileNodeViewModel.ofFileNode _dict)
          |> Seq.toObservableCollection
        dispatcher.Invoke(fun () ->
          _foundListViewModel.Items |> ObservableCollection.addRange items
          )
    }
    |> Async.Start

  let _setSearchText v =
    _searchText <- v
    this.RaisePropertyChanged("SearchText")
    searchIncrementally ()

  let _commitCommand =
    Command.create (fun _ -> true) (fun _ ->
      _foundListViewModel.SelectFirstIfNoSelection()
      _foundListViewModel.TrySelectedItem() |> Option.iter (fun item ->
        let node      = _dict |> Dict.findNode item.FileNodeId
        _dict <- _dict |> Dict.execute node
        _setSearchText ""
        ))
    |> fst

  member this.SearchText
    with get () = _searchText
    and  set v  = _setSearchText v

  member this.FoundList = _foundListViewModel

  member this.CommitCommand = _commitCommand

  member this.Save() =
    try
      File.WriteAllText(dictFile.FullName, _dict |> Dict.toJson)
    with | _ ->
      ()
