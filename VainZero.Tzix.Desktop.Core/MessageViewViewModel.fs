namespace VainZero.Tzix.Desktop.Core

open Basis.Core
open Dyxi.Util.Wpf

type MessageViewViewModel() =
  inherit ViewModel.Base()

  let mutable _text = ""

  let mutable _isInProgress = false

  member this.Text
    with get () = _text
    and  set v  = _text <- v; this.RaisePropertyChanged("Text")

  member this.IsInProgress
    with get () = _isInProgress
    and  set v  = _isInProgress <- v; this.RaisePropertyChanged("IsInProgress")
