namespace Tzix.ViewModel

open System
open System.IO
open System.Windows.Threading
open Basis.Core
open Chessie.ErrorHandling
open Dyxi.Util.Wpf
open Tzix.Model

type MainWindowViewModel(_env: Environment, _dictFile: IFile, _importRuleFile: IFile, _dispatcher: IDispatcher) =
  inherit ViewModel.Base()

  let mutable _pageIndex = PageIndex.MessageView

  let _messageView = MessageViewViewModel()
  let mutable _searchControlOpt = (None: option<SearchControlViewModel>)

  new (dispatcher: IDispatcher) =
    let env =
      {
        FileSystem  = DotNetFileSystem.Instance
        Executor    = DotNetExecutor.Instance
      }
    let fsys = env.FileSystem
    let dictFile = fsys.FileInfo(@"tzix.json")
    let importRuleFile = fsys.FileInfo(@".tzix_import_rules")
    in MainWindowViewModel(env, dictFile, importRuleFile, dispatcher)

  member this.PageIndex
    with get () = _pageIndex
    and  set i  =
      _pageIndex <- i
      _dispatcher.Invoke(fun () -> this.RaisePropertyChanged("PageIndex"))

  member private this.ShowMessage(msg, isInProgress) =
    _messageView.Text <- msg
    _messageView.IsInProgress <- isInProgress
    this.PageIndex <- PageIndex.MessageView

  member this.TransStateAsync(state) =
    async {
      match state with
      | AppState.Stuck msg ->
          this.ShowMessage(msg, (* isInProgress = *) false)
      | AppState.Loading ->
          match _searchControlOpt with
          | None ->
              this.ShowMessage("Now loading/creating the dictionary...", (* isInProgress = *) true)
              return! this.LoadDictAsync()
          | Some _ ->
              return! this.TransStateAsync(AppState.Running)
      | AppState.Running ->
          match _searchControlOpt with
          | None   -> return! this.TransStateAsync(AppState.Loading)
          | Some _ -> this.PageIndex <- PageIndex.SearchControl
    }

  member this.TransStateTo(state) =
    this.TransStateAsync(state) |> Async.Start

  member this.LoadDictAsync() =
    async {
      let! result = Dict.tryLoadAsync _env _dictFile _importRuleFile
      let state =
        match result with
        | Pass dict
        | Warn (dict, _) ->
            this.SearchControlViewModelOpt <- SearchControlViewModel(dict) |> Some
            AppState.Running
        | Fail es ->
            let msg =
              es |> List.map (fun e -> e.Message)
              |> String.concat Environment.NewLine
            in AppState.Stuck msg
      return! this.TransStateAsync(state)
    }

  member this.Save() =
    _searchControlOpt |> Option.iter (fun searchControl ->
      try
        _dictFile.WriteTextAsync(searchControl.Dict |> Dict.toJson) |> Async.RunSynchronously
      with | _ ->
        ()
      )

  member this.MessageViewViewModel =
    _messageView

  member this.SearchControlViewModelOpt
    with get () = _searchControlOpt
    and  set v  = _searchControlOpt <- v; this.RaisePropertyChanged("SearchControlViewModelOpt")
