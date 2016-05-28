namespace Tzix.ViewModel

open System
open System.IO
open System.Windows
open System.Windows.Threading
open Basis.Core
open Chessie.ErrorHandling
open Dyxi.Util.Wpf
open Tzix.Model

type MainWindowViewModel(dispatcher: Dispatcher) =
  inherit ViewModel.Base()

  let mutable _selectedIndex = -1

  let rectFile = FileInfo(@"tzix_window_rect.json")
  let dictFile = FileInfo(@"tzix.json")
  let importRuleFile = FileInfo(@".tzix_import_rules")

  let _messageView = MessageViewViewModel()
  let mutable _searchControlOpt = (None: option<SearchControlViewModel>)

  let _rect =
    try
      rectFile |> FileInfo.readTextAsync |> Async.RunSynchronously
      |> Serialize.Json.deserialize<Rect>
    with
    | _ -> Rect(0.0, 0.0, 320.0, 0.0)

  member this.SelectedIndex
    with get () = _selectedIndex
    and  set i  =
      _selectedIndex <- i
      dispatcher.Invoke(fun () -> this.RaisePropertyChanged("SelectedIndex"))

  member private this.ShowMessage(msg, isInProgress) =
    _messageView.Text <- msg
    _messageView.IsInProgress <- isInProgress
    this.SelectedIndex <- PageIndex.MessageView |> int

  member this.TransStateTo(state) =
    match state with
    | AppState.Stuck msg ->
        this.ShowMessage(msg, (* isInProgress = *) false)
    | AppState.Loading ->
        match _searchControlOpt with
        | None ->
              this.ShowMessage("Now loading/creating the dictionary...", (* isInProgress = *) true)
              this.LoadDictAsync() |> Async.Start
        | Some _ ->
            this.TransStateTo(AppState.Running)
    | AppState.Running ->
        match _searchControlOpt with
        | None   -> this.TransStateTo(AppState.Loading)
        | Some _ -> this.SelectedIndex <- PageIndex.SearchControl |> int

  member this.LoadDictAsync() =
    async {
      let! result = Dict.tryLoadAsync dictFile importRuleFile
      let state =
        match result with
        | Pass dict
        | Warn (dict, _) ->
            this.SearchControlViewModelOpt <- SearchControlViewModel(dict, dispatcher) |> Some
            AppState.Running
        | Fail es ->
            let msg =
              es |> List.map (fun e -> e.Message)
              |> String.concat Environment.NewLine
            in AppState.Stuck msg
      this.TransStateTo(state)
    }

  member this.Save() =
    _searchControlOpt |> Option.iter (fun searchControl ->
      try
        File.WriteAllText(dictFile.FullName, searchControl.Dict |> Dict.toJson)
      with | _ ->
        ()
      )
    try
      File.WriteAllText(rectFile.FullName, Serialize.Json.serialize this.Rect)
    with
    | _ -> ()

  member this.MessageViewViewModel =
    _messageView

  member this.SearchControlViewModelOpt
    with get () = _searchControlOpt
    and  set v  = _searchControlOpt <- v; this.RaisePropertyChanged("SearchControlViewModelOpt")

  /// Height is ignored
  member this.Rect = _rect
